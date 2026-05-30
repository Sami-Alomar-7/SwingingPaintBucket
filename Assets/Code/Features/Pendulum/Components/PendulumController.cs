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

namespace SwingingPaintBucket.Features.Pendulum.Components
{
    [RequireComponent(typeof(Transform))]
    public class PendulumController : MonoBehaviour, IPendulumView
    {
        [Header("Pendulum Services")]
        [SerializeField] private PendulumInputHandler _inputHandler;
        public IPendulumInputHandler InputHandler { get => _inputHandler; set => _inputHandler = value as PendulumInputHandler; }
        public IPendulumPhysics      PhysicsEngine;
        public IMassProvider         MassProvider;
        public IRope                 Rope;
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
        private PendulumState  _state;
        private float          _angleAcceleration;
        private bool           _isRunning;

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
            // Wire the concrete surface painter implementation into the emitter module contract interface wrapper
            if (_paintEmitter != null && _paintEmitter.surfaceService == null)
            {
                _paintEmitter.surfaceService = new SwingingPaintBucket.Features.Surface.Services.PaintSurfaceService();
            }
            if (_inputHandler == null) _inputHandler = new PendulumInputHandler();
            if (PhysicsEngine == null) PhysicsEngine = new PendulumPhysicsService();
            if (MassProvider == null) MassProvider = new MassSystem();

            if (_paintEmitter == null) _paintEmitter = FindAnyObjectByType<PaintEmitter>();
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

            // CRITICAL FIX: Convert initial Theta angle into a physical Cartesian coordinate for elastic simulation loops
            // Elastic physics (Nylon/Bungee) depends on BucketPosition for spring force calculations
            Vector3 pivotPos = _pivotTransform != null ? _pivotTransform.position : new Vector3(0f, _pivotY, 0f);
            float length = _config != null ? _config.RopeLength : 3f;
            _state.BucketPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);

            // Convert initial angular velocity (Omega) into a Cartesian linear velocity vector
            // Tangential direction is perpendicular to radial direction: (cos(θ), sin(θ), 0)
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

            // Apply rope type presets for elastic physics
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

            // CRITICAL FIX: Recalculate BucketPosition from Theta for elastic physics compatibility
            Vector3 pivotPos = _pivotTransform != null ? _pivotTransform.position : new Vector3(0f, _pivotY, 0f);
            float length = _config != null ? _config.RopeLength : 3f;

            _state.BucketPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);
            _state.Omega = 0f;
            _state.Velocity = Vector3.zero;
            _angleAcceleration = 0f;

            UpdateBucketPosition(_state.BucketPosition);
            UpdateRope(pivotPos, _state.BucketPosition);

            // FIX 1: Explicitly force MassSystem back into alignment with the UI Config parameters
            if (MassProvider is MassSystem massSystem && _config != null)
            {
                massSystem.BaseMass = _config.BaseMass;
                float currentPaint = massSystem.PaintMass;
                massSystem.AdjustPaintMass(_config.InitialPaintMass - currentPaint);
            }

            // FIX 2: Explicitly restore bucket material renderer state back to standard black capacity configuration
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
                // Rigid physics: Use angular theta/omega integration
                _state.Omega += _angleAcceleration * dt;
                _state.Theta += _state.Omega * dt;
                _state.Theta = Mathf.Clamp(_state.Theta, -Mathf.PI, Mathf.PI);

                _state.BucketPosition = pivotPos + length * new Vector3(Mathf.Sin(_state.Theta), -Mathf.Cos(_state.Theta), 0f);
            }
            else
            {
                // Elastic physics: Use Cartesian spring-damper integration
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

            // Use RopeConfig.RestLength as the natural length for elastic physics
            float restLength = _config.RopeConfig.RestLength > 0f ? _config.RopeConfig.RestLength : length;

            // Calculate rope vector from pivot to bucket
            Vector3 ropeVector = _state.BucketPosition - pivotPos;
            float currentLength = ropeVector.magnitude;

            // Calculate unit radial direction
            Vector3 radialDir = currentLength > 0.0001f ? ropeVector.normalized : Vector3.down;

            // Calculate rope stretch (positive = stretched, negative = compressed)
            float deltaL = currentLength - restLength;

            // Hooke's Law spring force - behavior depends on rope type
            Vector3 springForce = Vector3.zero;
            if (_config.CurrentRopeType == RopeType.Nylon)
            {
                // Elastic Rope: Spring force applies in BOTH directions (stretch and compression)
                springForce = -_config.RopeConfig.SpringStiffness * deltaL * radialDir;
            }
            else if (_config.CurrentRopeType == RopeType.Bungee)
            {
                // Bungee Cord: Spring force ONLY when stretched, with smooth transition near threshold
                // Implement smooth transition over first 0.05 meters to eliminate discontinuity spikes
                const float transitionZone = 0.05f; // 5cm transition zone
                
                if (deltaL > 0f)
                {
                    // Calculate smooth transition factor (sigmoid-like)
                    float transitionFactor = Mathf.Clamp01(deltaL / transitionZone);
                    // Use smoothstep for even smoother transition: 3x^2 - 2x^3
                    float smoothFactor = transitionFactor * transitionFactor * (3f - 2f * transitionFactor);
                    
                    springForce = -_config.RopeConfig.SpringStiffness * deltaL * smoothFactor * radialDir;
                }
                // When deltaL <= 0, springForce remains zero (cord goes slack)
            }

            // Internal rope damping (radial velocity component)
            float vRadial = Vector3.Dot(_state.Velocity, radialDir);
            Vector3 ropeDampingForce = -_config.RopeConfig.RopeDamping * vRadial * radialDir;

            // Gravity force
            Vector3 gravityForce = totalMass * _config.Gravity * Vector3.down;

            // Rotational air drag force
            Vector3 airDragForce = -_config.DampingCoefficient * _state.Velocity;

            // Total force
            Vector3 totalForce = gravityForce + springForce + ropeDampingForce + airDragForce;

            // Semi-implicit Euler integration
            _state.Velocity += (totalForce / totalMass) * dt;
            _state.BucketPosition += _state.Velocity * dt;

            // Clamp to X-Y plane (must be done before orientation synchronization)
            _state.BucketPosition.z = 0f;
            _state.Velocity.z = 0f;

            // Update theta for compatibility with UI and other systems
            _state.Theta = Mathf.Atan2(_state.BucketPosition.x - pivotPos.x, pivotPos.y - _state.BucketPosition.y);
            
            // Angular velocity calculation: divide by current dynamic length, not static rest length
            // This ensures accurate angular velocity tracking as the rope stretches and contracts
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