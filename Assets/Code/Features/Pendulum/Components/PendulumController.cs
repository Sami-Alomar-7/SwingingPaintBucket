using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.Rope.Interfaces;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.ExternalForces.Services;
using SwingingPaintBucket.Features.Paint.Components;
using SwingingPaintBucket.Features.Rope.Data;
using SwingingPaintBucket.Features.Pendulum.Services;
using SwingingPaintBucket.Features.Surface.Components;

namespace SwingingPaintBucket.Features.Pendulum.Components
{
    [RequireComponent(typeof(Transform))]
    public class PendulumController : MonoBehaviour, IPendulumView
    {
        [Header("Pendulum Services")]
        [SerializeField] private PendulumInputHandler _inputHandler;
        public IPendulumInputHandler InputHandler { get => _inputHandler; set => _inputHandler = value as PendulumInputHandler; }
        public IPendulumPhysics PhysicsEngine;
        public IMassProvider MassProvider;
        public IRope Rope;
        public IList<IForceProvider> ForceProviders { get; set; } = new List<IForceProvider>();

        [Header("Scene References")]
        [SerializeField] private PaintEmitter _paintEmitter;
        [SerializeField] private Transform _pivotTransform;
        [SerializeField] private Transform _bucketTransform;
        [SerializeField] private float _pivotY;
        [SerializeField] private UnityEngine.UI.Button _startButton;
        [SerializeField] private UnityEngine.UI.Button _resetButton;

        [Header("Ground Physical Settings")]
        [SerializeField] private float _groundLevel = 0.2f;

        public float PivotY { get => _pivotY; set => _pivotY = value; }
        public Transform PivotTransform { get => _pivotTransform; set => _pivotTransform = value; }
        public Transform BucketTransform { get => _bucketTransform; set => _bucketTransform = value; }
        public PaintEmitter PaintEmitter { get => _paintEmitter; set => _paintEmitter = value; }
        public UnityEngine.UI.Button StartButton { get => _startButton; set => _startButton = value; }
        public UnityEngine.UI.Button ResetButton { get => _resetButton; set => _resetButton = value; }

        private PendulumConfig _config;
        private PendulumState _state;
        private float _angleAcceleration;
        private bool _isRunning;

        public float CurrentApertureDiameter => _config != null ? _config.ApertureDiameter : 0.01f;

        private void Awake()
        {
            if (_startButton != null) _startButton.onClick.AddListener(StartSimulation);
            if (_resetButton != null) _resetButton.onClick.AddListener(ResetSimulation);

            InitializeDependencies();
        }

        private void Start()
        {
            ResetSimulation();
        }

        private void InitializeDependencies()
        {
            if (_inputHandler == null) _inputHandler = new PendulumInputHandler();
            if (PhysicsEngine == null) PhysicsEngine = new PendulumPhysicsService();
            if (MassProvider == null) MassProvider = new MassSystem();

            ForceProviders = new List<IForceProvider>
            {
                new GravityForce(MassProvider),
                new DragForce(),
                new GroundCollisionForce(_groundLevel, stiffness: 15000f, damping: 150f, friction: 0.5f)
            };

            if (_paintEmitter == null) _paintEmitter = FindAnyObjectByType<PaintEmitter>();
            if (_paintEmitter != null && _paintEmitter.paintSurfaceSystem == null)
            {
                _paintEmitter.paintSurfaceSystem = FindAnyObjectByType<PaintSurfaceSystem>();
            }

            if (_bucketTransform == null && _paintEmitter != null) _bucketTransform = _paintEmitter.bucket;
            if (_pivotTransform == null)
            {
                GameObject pivotGO = GameObject.Find("Pivot");
                if (pivotGO != null) _pivotTransform = pivotGO.transform;
            }

            if (Rope == null) Rope = FindAnyObjectByType<SwingingPaintBucket.Features.Rope.Components.RopeRenderer>();
        }

        private void FixedUpdate()
        {
            if (!_isRunning) return;

            float dt = Time.fixedDeltaTime;
            Vector3 pivotPos = _pivotTransform != null ? _pivotTransform.position : new Vector3(0f, _pivotY, 0f);
            float totalMass = MassProvider != null ? MassProvider.GetTotalMass() : 1f;

            if (PhysicsEngine != null && _config != null)
            {
                if (_config.CurrentRopeType == RopeType.Rigid)
                {

                    var rigidForces = new List<IForceProvider>();
                    foreach (var fp in ForceProviders)
                    {
                        if (!(fp is GroundCollisionForce)) rigidForces.Add(fp);
                    }
                    _angleAcceleration = PhysicsEngine.ComputeAngularAcceleration(_state, _config, totalMass, rigidForces);
                }
                else
                {

                    _angleAcceleration = PhysicsEngine.ComputeAngularAcceleration(_state, _config, totalMass, ForceProviders);
                }
            }

            IntegrateState(dt, pivotPos, _config != null ? _config.RopeLength : 3f);
        }

        public void StartSimulation()
        {
            if (_inputHandler != null)
            {
                _config = _inputHandler.ReadPendulumConfig();
                _state = _inputHandler.ReadPendulumState();
            }

            Vector3 pivotPos = _pivotTransform != null ? _pivotTransform.position : new Vector3(0f, _pivotY, 0f);
            float length = _config != null ? _config.RopeLength : 3f;

            if (_config != null)
            {
                _config.PivotPosition = pivotPos;
                _config.RopeConfig.RestLength = length;
            }

            Vector3 proposedPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);

            if (proposedPosition.y < _groundLevel)
            {
                proposedPosition.y = _groundLevel;

                float dx = proposedPosition.x - pivotPos.x;
                float dy = pivotPos.y - _groundLevel;
                _state.Theta = Mathf.Atan2(dx, dy);
            }

            _state.BucketPosition = proposedPosition;

            Vector3 tangentialDir = new Vector3(Mathf.Cos(_state.Theta), Mathf.Sin(_state.Theta), 0f);
            _state.Velocity = tangentialDir * (_state.Omega * length);

            if (_config != null && _config.CurrentRopeType != RopeType.Rigid)
            {
                _state.Velocity.z = 1.5f;
            }

            if (MassProvider is MassSystem massSystem && _config != null)
            {
                massSystem.BaseMass = _config.BaseMass;
                float currentPaint = massSystem.PaintMass;
                massSystem.AdjustPaintMass(_config.InitialPaintMass - currentPaint);
            }

            if (_bucketTransform != null)
            {
                Renderer bucketRenderer = _bucketTransform.GetComponent<Renderer>();
                if (bucketRenderer != null) bucketRenderer.material.color = Color.black;
            }

            if (_config != null)
            {
                RopeTypePresets.ApplyPreset(_config.RopeConfig, _config.CurrentRopeType);
                if (Rope is SwingingPaintBucket.Features.Rope.Components.RopeRenderer visualRope)
                {
                    visualRope.ApplyVisualConfig(_config.RopeConfig);
                }
            }

            _isRunning = true;
            if (_paintEmitter != null) _paintEmitter.StartSimulation();
        }

        public void ResetSimulation()
        {
            _isRunning = false;

            if (_inputHandler != null)
            {
                _config = _inputHandler.ReadPendulumConfig();
                _state = _inputHandler.ReadPendulumState();
            }

            Vector3 pivotPos = _pivotTransform != null ? _pivotTransform.position : new Vector3(0f, _pivotY, 0f);
            float length = _config != null ? _config.RopeLength : 3f;

            _state.BucketPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);
            _state.Omega = 0f;
            _state.Velocity = Vector3.zero;
            _angleAcceleration = 0f;

            UpdateBucketPosition(_state.BucketPosition);
            UpdateRope(pivotPos, _state.BucketPosition);

            if (MassProvider is MassSystem massSystem && _config != null)
            {
                massSystem.BaseMass = _config.BaseMass;
                float currentPaint = massSystem.PaintMass;
                massSystem.AdjustPaintMass(_config.InitialPaintMass - currentPaint);
            }

            if (_paintEmitter != null)
            {
                _paintEmitter.StopSimulation();
                _paintEmitter.ClearParticles();
            }
        }

        private void IntegrateState(float dt, Vector3 pivotPos, float length)
        {
            if (_config != null && _config.CurrentRopeType == RopeType.Rigid)
            {

                _state.Omega += _angleAcceleration * dt;
                _state.Theta += _state.Omega * dt;
                _state.Theta = Mathf.Clamp(_state.Theta, -Mathf.PI, Mathf.PI);

                _state.BucketPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);

                if (_state.BucketPosition.y < _groundLevel)
                {
                    float verticalDistance = pivotPos.y - _groundLevel;
                    float maxAllowedTheta = Mathf.Acos(Mathf.Clamp(verticalDistance / length, 0f, 1f));

                    _state.Theta = Mathf.Sign(_state.Theta) * maxAllowedTheta;

                    _state.Omega = -_state.Omega * 0.3f;

                    _state.BucketPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);
                }

                _state.Velocity = new Vector3(Mathf.Cos(_state.Theta), Mathf.Sin(_state.Theta), 0f) * (_state.Omega * length);
            }
            else
            {

                IntegrateElasticPhysics(dt, pivotPos, length);
            }

            UpdateBucketPosition(_state.BucketPosition);
            UpdateRope(pivotPos, _state.BucketPosition);
        }

        private void IntegrateElasticPhysics(float dt, Vector3 pivotPos, float length)
        {
            if (_config == null) return;
            float totalMass = MassProvider != null ? MassProvider.GetTotalMass() : 1f;
            if (totalMass < 0.001f) totalMass = 0.001f;

            float restLength = _config.RopeConfig.RestLength > 0f ? _config.RopeConfig.RestLength : length;

            Vector3 ropeVector = _state.BucketPosition - pivotPos;
            float currentLength = ropeVector.magnitude;

            Vector3 radialDir = currentLength > 0.0001f ? ropeVector.normalized : Vector3.down;
            float deltaL = currentLength - restLength;

            Vector3 springForce = Vector3.zero;
            if (_config.CurrentRopeType == RopeType.Nylon)
            {
                springForce = -_config.RopeConfig.SpringStiffness * deltaL * radialDir;
            }
            else if (_config.CurrentRopeType == RopeType.Bungee)
            {
                const float transitionZone = 0.05f;
                if (deltaL > 0f)
                {
                    float transitionFactor = Mathf.Clamp01(deltaL / transitionZone);
                    float smoothFactor = transitionFactor * transitionFactor * (3f - 2f * transitionFactor);
                    springForce = -_config.RopeConfig.SpringStiffness * deltaL * smoothFactor * radialDir;
                }
            }

            float vRadial = Vector3.Dot(_state.Velocity, radialDir);
            Vector3 ropeDampingForce = -_config.RopeConfig.RopeDamping * vRadial * radialDir;

            Vector3 environmentalForces = Vector3.zero;
            if (ForceProviders != null)
            {
                for (int i = 0; i < ForceProviders.Count; i++)
                {
                    if (ForceProviders[i] != null)
                    {
                        environmentalForces += ForceProviders[i].GetForce(_state, _config);
                    }
                }
            }

            Vector3 totalForce = springForce + ropeDampingForce + environmentalForces;

            _state.Velocity += (totalForce / totalMass) * dt;
            _state.BucketPosition += _state.Velocity * dt;

            if (_state.BucketPosition.y < _groundLevel)
            {
                _state.BucketPosition.y = _groundLevel;
                if (_state.Velocity.y < 0f) _state.Velocity.y = -_state.Velocity.y * 0.1f;
            }

            Vector3 planarProj = new Vector3(_state.BucketPosition.x - pivotPos.x, 0f, _state.BucketPosition.z - pivotPos.z);
            _state.Theta = Mathf.Atan2(planarProj.magnitude * Mathf.Sign(planarProj.x), pivotPos.y - _state.BucketPosition.y);
            _state.Omega = Vector3.Dot(_state.Velocity, new Vector3(Mathf.Cos(_state.Theta), Mathf.Sin(_state.Theta), 0f)) / (currentLength > 0.01f ? currentLength : 1f);
        }

        public void UpdateBucketPosition(Vector3 worldPosition)
        {
            if (_bucketTransform != null)
            {
                _bucketTransform.position = worldPosition;
                if (_pivotTransform != null)
                {
                    Vector3 pivotPos = _pivotTransform.position;
                    Vector3 toPivot = (pivotPos - worldPosition).normalized;
                    if (toPivot != Vector3.zero) _bucketTransform.up = toPivot;
                }
            }
        }

        public void UpdateRope(Vector3 pivotPos, Vector3 bucketPos)
        {
            if (Rope != null) Rope.UpdateRope(pivotPos, bucketPos);
        }

        public bool IsRunning() => _isRunning;
    }
}