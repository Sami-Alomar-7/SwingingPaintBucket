using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.Rope.Interfaces;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Paint.Components;
using SwingingPaintBucket.Features.Paint.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Services;
using SwingingPaintBucket.Features.Rope.Data;
using SwingingPaintBucket.Features.Surface.Components; // تم إضافة فضاء الأسماء الخاص بنظام السطح الجديد

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

        // ─── PUBLIC COMPOSITION PROPERTIES FOR EDITOR COMPOSITION ───
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

            if (_paintEmitter == null) _paintEmitter = FindAnyObjectByType<PaintEmitter>();

            // إصلاح الربط التلقائي لنظام الطلاء المعتمد على البنية الجديدة المفككة
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
                _angleAcceleration = PhysicsEngine.ComputeAngularAcceleration(_state, _config, totalMass, ForceProviders);
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

            if (_config != null && _pivotTransform != null)
            {
                _config.PivotPosition = _pivotTransform.position;
            }

            Vector3 pivotPos = _pivotTransform != null ? _pivotTransform.position : new Vector3(0f, _pivotY, 0f);
            float length = _config != null ? _config.RopeLength : 3f;
            _state.BucketPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);

            Vector3 tangentialDir = new Vector3(Mathf.Cos(_state.Theta), Mathf.Sin(_state.Theta), 0f);
            _state.Velocity = tangentialDir * (_state.Omega * length);

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

            if (_config != null && _config.CurrentRopeType != RopeType.Rigid)
            {
                RopeTypePresets.ApplyPreset(_config.RopeConfig, _config.CurrentRopeType);
            }

            _isRunning = true;

            if (_paintEmitter != null)
            {
                _paintEmitter.StartSimulation();
            }
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

            if (_bucketTransform != null)
            {
                Renderer bucketRenderer = _bucketTransform.GetComponent<Renderer>();
                if (bucketRenderer != null)
                {
                    bucketRenderer.material.color = Color.black;
                }
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

            Vector3 gravityForce = totalMass * _config.Gravity * Vector3.down;
            Vector3 airDragForce = -_config.DampingCoefficient * _state.Velocity;

            Vector3 totalForce = gravityForce + springForce + ropeDampingForce + airDragForce;

            _state.Velocity += (totalForce / totalMass) * dt;
            _state.BucketPosition += _state.Velocity * dt;

            _state.BucketPosition.z = 0f;
            _state.Velocity.z = 0f;

            _state.Theta = Mathf.Atan2(_state.BucketPosition.x - pivotPos.x, pivotPos.y - _state.BucketPosition.y);
            _state.Omega = Vector3.Dot(_state.Velocity, new Vector3(Mathf.Cos(_state.Theta), Mathf.Sin(_state.Theta), 0f)) / currentLength;
        }

        public void UpdateBucketPosition(Vector3 worldPosition)
        {
            if (_bucketTransform != null) _bucketTransform.position = worldPosition;
        }

        public void UpdateRope(Vector3 pivotPos, Vector3 bucketPos)
        {
            if (Rope != null)
            {
                Rope.UpdateRope(pivotPos, bucketPos);
            }
        }

        private void ValidateDependencies()
        {
            if (PhysicsEngine == null) Debug.LogError("[PendulumController] PhysicsEngine is unassigned.", this);
        }

        public bool IsRunning() => _isRunning;
    }
}