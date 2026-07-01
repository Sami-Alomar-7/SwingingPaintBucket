using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Components;
using SwingingPaintBucket.Features.Pendulum.Data;

namespace SwingingPaintBucket.Features.Paint.Components
{
    public class BucketLiquidVolume : MonoBehaviour
    {
        [Header("Glass Box Visual")]
        [SerializeField] private Transform _glassOuterBox;
        [SerializeField] private Material _glassMaterial;
        [SerializeField] private Material _liquidMaterial;

        [Header("Liquid Physics")]
        [SerializeField] private int _particleCount = 55;
        [SerializeField] private float _particleSize = 0.035f;
        [SerializeField] private float _cohesionForce = 12f;
        [SerializeField] private float _viscosity = 15f;
        [SerializeField] private float _damping = 0.55f;
        [SerializeField] private float _inertiaResponse = 1.6f;

        [Header("References")]
        [SerializeField] private PendulumController _pendulum;
        [SerializeField] private Transform _bucketTransform;

        private List<LiquidParticle> _particles = new List<LiquidParticle>();
        private List<Transform> _visuals = new List<Transform>();

        private Vector3 _boxHalfExtents;
        private float _maxLiquidHeight;

        private float _currentLiquidLevel = 1f;
        private Vector3 _previousBucketPos;
        private Vector3 _bucketVelocity;
        private Vector3 _bucketAcceleration;

        [System.Serializable]
        private class LiquidParticle
        {
            public Vector3 localPosition;
            public Vector3 velocity;
            public bool isActive = true;
        }

        private void Start()
        {
            if (_pendulum == null) _pendulum = FindObjectOfType<PendulumController>();
            if (_bucketTransform == null && _pendulum != null)
                _bucketTransform = _pendulum.BucketTransform;

            SetupGlassBox();
            InitializeParticles();

            if (_bucketTransform != null)
                _previousBucketPos = _bucketTransform.position;
        }

        private void SetupGlassBox()
        {
            if (_glassOuterBox == null) return;

            _boxHalfExtents = _glassOuterBox.localScale * 0.5f;
            _maxLiquidHeight = _boxHalfExtents.y * 1.8f;

            if (_glassMaterial == null)
            {
                _glassMaterial = new Material(Shader.Find("Standard"));
                _glassMaterial.SetFloat("_Mode", 3);
                _glassMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _glassMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _glassMaterial.SetInt("_ZWrite", 0);
                _glassMaterial.EnableKeyword("_ALPHABLEND_ON");
                _glassMaterial.color = new Color(0.85f, 0.92f, 1f, 0.12f);
                _glassMaterial.SetFloat("_Glossiness", 0.95f);
            }

            Renderer rend = _glassOuterBox.GetComponent<Renderer>();
            if (rend != null) rend.material = _glassMaterial;
        }

        private void InitializeParticles()
        {
            if (_glassOuterBox == null) return;

            for (int i = 0; i < _particleCount; i++)
            {
                LiquidParticle p = new LiquidParticle();
                float yBias = Random.value;
                p.localPosition = new Vector3(
                    Random.Range(-_boxHalfExtents.x * 0.8f, _boxHalfExtents.x * 0.8f),
                    -_boxHalfExtents.y + yBias * _maxLiquidHeight * 0.85f,
                    Random.Range(-_boxHalfExtents.z * 0.8f, _boxHalfExtents.z * 0.8f)
                );
                p.velocity = Vector3.zero;
                _particles.Add(p);

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"LiquidParticle_{i}";
                sphere.transform.SetParent(_glassOuterBox);
                sphere.transform.localPosition = p.localPosition;
                sphere.transform.localScale = Vector3.one * _particleSize;

                Collider col = sphere.GetComponent<Collider>();
                if (col != null) Destroy(col);

                Renderer rend = sphere.GetComponent<Renderer>();
                if (rend != null)
                {
                    if (_liquidMaterial == null)
                    {
                        _liquidMaterial = new Material(Shader.Find("Standard"));
                        _liquidMaterial.color = new Color(0.85f, 0.15f, 0.2f, 0.85f); // لون طلاء أحمر مائي واضح للجنة التحكيم
                        _liquidMaterial.SetFloat("_Glossiness", 0.9f);
                    }
                    rend.material = _liquidMaterial;
                    rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                _visuals.Add(sphere.transform);
            }
        }

        private void Update()
        {
            if (_bucketTransform == null || _glassOuterBox == null) return;

            Vector3 currentPos = _bucketTransform.position;
            if (Time.deltaTime > 0f)
            {
                Vector3 newVel = (currentPos - _previousBucketPos) / Time.deltaTime;
                _bucketAcceleration = (newVel - _bucketVelocity) / Time.deltaTime;
                _bucketVelocity = newVel;
            }
            _previousBucketPos = currentPos;

            UpdateLiquidLevel();
            UpdateParticlePhysics();
            UpdateVisuals();
        }

        private void UpdateLiquidLevel()
        {
            if (_pendulum != null && _pendulum.MassProvider != null)
            {
                float totalMass = _pendulum.MassProvider.GetTotalMass();
                float baseMass = 1f;
                float maxPaint = 0.5f;
                _currentLiquidLevel = Mathf.Clamp01((totalMass - baseMass) / maxPaint);
            }
        }

        private void UpdateParticlePhysics()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.015f);
            Vector3 localGravity = new Vector3(0f, -9.81f, 0f);

            Vector3 localInertia = _glassOuterBox.InverseTransformDirection(_bucketAcceleration) * _inertiaResponse;
            Vector3 effectiveGravity = localGravity + localInertia;

            if (_pendulum != null)
            {
                PendulumState? nullableState = GetPendulumState();
                if (nullableState.HasValue)
                {
                    PendulumState state = nullableState.Value;
                    float theta = state.Theta;
                    float omega = state.Omega;
                    float length = GetPendulumLength();
                    float centripetal = length * omega * omega;
                    float g = 9.81f;
                    float direction = Mathf.Cos(theta);
                    float gEff = Mathf.Max(0.1f, g * Mathf.Abs(direction) + direction * centripetal);
                    effectiveGravity.y = -gEff;
                }
            }

            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                if (!p.isActive) continue;

                Vector3 cohesion = Vector3.zero;
                Vector3 viscosity = Vector3.zero;
                int neighborCount = 0;

                for (int j = 0; j < _particles.Count; j++)
                {
                    if (i == j || !_particles[j].isActive) continue;

                    Vector3 diff = _particles[j].localPosition - p.localPosition;
                    float dist = diff.magnitude;
                    float radius = _particleSize * 3.5f;

                    if (dist < radius && dist > 0.001f)
                    {
                        float strength = (1f - dist / radius) * _cohesionForce;
                        cohesion += diff.normalized * strength;
                        viscosity += (_particles[j].velocity - p.velocity) * _viscosity;
                        neighborCount++;
                    }
                }

                if (neighborCount > 0)
                {
                    cohesion /= neighborCount;
                    viscosity /= neighborCount;
                }

                Vector3 totalForce = effectiveGravity + cohesion + viscosity;
                p.velocity += totalForce * dt;
                p.velocity *= _damping;
                p.localPosition += p.velocity * dt;

                float bounce = 0.05f;

                // X Bounds collision
                if (p.localPosition.x > _boxHalfExtents.x * 0.88f) { p.localPosition.x = _boxHalfExtents.x * 0.88f; p.velocity.x = -p.velocity.x * bounce; }
                else if (p.localPosition.x < -_boxHalfExtents.x * 0.88f) { p.localPosition.x = -_boxHalfExtents.x * 0.88f; p.velocity.x = -p.velocity.x * bounce; }

                // Y Bounds collision (تأثر تراقص السطح العلوي)
                float liquidTop = -_boxHalfExtents.y + _currentLiquidLevel * _maxLiquidHeight;
                if (p.localPosition.y > liquidTop) { p.localPosition.y = liquidTop; p.velocity.y = -p.velocity.y * bounce; }
                else if (p.localPosition.y < -_boxHalfExtents.y * 0.92f) { p.localPosition.y = -_boxHalfExtents.y * 0.92f; p.velocity.y = -p.velocity.y * bounce; }

                // Z Bounds collision
                if (p.localPosition.z > _boxHalfExtents.z * 0.88f) { p.localPosition.z = _boxHalfExtents.z * 0.88f; p.velocity.z = -p.velocity.z * bounce; }
                else if (p.localPosition.z < -_boxHalfExtents.z * 0.88f) { p.localPosition.z = -_boxHalfExtents.z * 0.88f; p.velocity.z = -p.velocity.z * bounce; }
            }
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < _particles.Count && i < _visuals.Count; i++)
            {
                var p = _particles[i];
                var visual = _visuals[i];
                if (visual == null) continue;

                visual.gameObject.SetActive(p.isActive);
                if (p.isActive)
                {
                    visual.localPosition = p.localPosition;
                    visual.localScale = Vector3.one * _particleSize;
                }
            }
        }

        public void RemoveParticle()
        {
            int topIndex = -1;
            float maxY = float.MinValue;

            for (int i = 0; i < _particles.Count; i++)
            {
                if (_particles[i].isActive && _particles[i].localPosition.y > maxY)
                {
                    maxY = _particles[i].localPosition.y;
                    topIndex = i;
                }
            }

            if (topIndex >= 0)
            {
                _particles[topIndex].isActive = false;
            }
        }

        private PendulumState? GetPendulumState()
        {
            if (_pendulum == null) return null;
            var stateField = _pendulum.GetType().GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stateField != null) return (PendulumState)stateField.GetValue(_pendulum);
            return null;
        }

        private float GetPendulumLength()
        {
            if (_pendulum == null) return 3f;
            var configField = _pendulum.GetType().GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (configField != null)
            {
                var config = configField.GetValue(_pendulum) as PendulumConfig;
                if (config != null) return config.RopeLength;
            }
            return 3f;
        }
    }
}