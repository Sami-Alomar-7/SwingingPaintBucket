using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Components;

namespace SwingingPaintBucket.Features.Paint.Components
{
    public class BucketLiquidVolume : MonoBehaviour
    {
        [Header("Cylinder Visual")]
        [SerializeField] private Transform _glassOuterBox; // يمثل جسم الأسطوانة المحيط بالسائل
        [SerializeField] private Material _liquidMaterial;

        [Header("Cylinder Dimensions")]
        [SerializeField] private float _cylinderRadius = 0.5f;  // نصف قطر الأسطوانة (البديل لـ Extents.x و Extents.z)
        [SerializeField] private float _cylinderHeight = 1.0f;  // الارتفاع الكلي للأسطوانة (البديل لـ Extents.y * 2)
        [SerializeField] private float _particleSize = 0.025f;

        [Header("Liquid Physics")]
        [SerializeField] private int _maxParticleCount = 600;
        [SerializeField] private float _repulsionForce = 25f;
        [SerializeField] private float _inertiaResponse = 5.0f;
        [SerializeField] private float _sloshSensitivity = 0.5f;

        [Header("References")]
        [SerializeField] private PendulumController _pendulum;
        [SerializeField] private Transform _bucketTransform;

        private List<LiquidParticle> _particles = new List<LiquidParticle>();
        private List<Transform> _visuals = new List<Transform>();

        private float _maxLiquidHeight;
        private float _currentLiquidLevel = 1f;
        private Vector3 _previousBucketPos;
        private Vector3 _bucketVelocity;
        private Vector3 _bucketAcceleration;
        private int _activeParticlesRemaining;
        private int _currentConfiguredCount;

        public bool IsEmpty => _activeParticlesRemaining <= 0;

        [System.Serializable]
        private class LiquidParticle
        {
            public Vector3 localPosition;
            public Vector3 initialLocalPosition;
            public Vector3 velocity;
            public bool isActive = true;
        }

        private void Start()
        {
            if (_pendulum == null) _pendulum = FindObjectOfType<PendulumController>();
            if (_bucketTransform == null && _pendulum != null)
                _bucketTransform = _pendulum.BucketTransform;

            SetupCylinderLimits();
            CalculateInitialCountFromMass();
            InitializeParticles(_currentConfiguredCount);

            if (_bucketTransform != null)
                _previousBucketPos = _bucketTransform.position;
        }

        private void SetupCylinderLimits()
        {
            // أقصى ارتفاع مسموح به للسائل بالنسبة لمركز الأسطوانة المحلي (عادة نصف الارتفاع)
            _maxLiquidHeight = (_cylinderHeight / 2f) * 1.2f;

            if (_liquidMaterial == null)
            {
                _liquidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (_liquidMaterial == null) _liquidMaterial = new Material(Shader.Find("Standard"));
                _liquidMaterial.color = new Color(0.9f, 0f, 0.9f, 1f);
            }
        }

        private void CalculateInitialCountFromMass()
        {
            if (_pendulum != null && _pendulum.MassProvider != null)
            {
                float totalMass = _pendulum.MassProvider.GetTotalMass();
                float baseMass = 1f;
                float maxPaint = 0.5f;

                float initialRatio = Mathf.Clamp01((totalMass - baseMass) / maxPaint);
                if (Time.timeSinceLevelLoad < 0.5f && initialRatio <= 0.01f) initialRatio = 1f;

                _currentConfiguredCount = Mathf.CeilToInt(initialRatio * _maxParticleCount);
            }
            else
            {
                _currentConfiguredCount = _maxParticleCount;
            }
        }

        public void InitializeParticles(int count)
        {
            if (_glassOuterBox == null) return;

            foreach (var v in _visuals) { if (v != null) Destroy(v.gameObject); }
            _visuals.Clear();
            _particles.Clear();

            _activeParticlesRemaining = count;

            float halfHeight = _cylinderHeight / 2f;

            for (int i = 0; i < count; i++)
            {
                LiquidParticle p = new LiquidParticle();

                // التوليد العشوائي الموزع بانتظام داخل الأسطوانة الدائرية (Cylinder Distribution)
                float angle = Random.Range(0f, Mathf.PI * 2f);
                // استخدام الجذر التربيعي لضمان توزيع متناسق للجزيئات وعدم تكتلها في المركز
                float r = _cylinderRadius * 0.85f * Mathf.Sqrt(Random.Range(0f, 1f));

                float x = r * Mathf.Cos(angle);
                float z = r * Mathf.Sin(angle);
                float y = Random.Range(-halfHeight * 0.85f, halfHeight * 0.2f);

                p.localPosition = new Vector3(x, y, z);
                p.initialLocalPosition = p.localPosition;
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
                if (rend != null) rend.material = _liquidMaterial;

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

                int targetActiveCount = Mathf.CeilToInt(_currentLiquidLevel * _currentConfiguredCount);
                while (_activeParticlesRemaining > targetActiveCount && _activeParticlesRemaining > 0)
                {
                    RemoveParticle();
                }
            }
        }

        private void UpdateParticlePhysics()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.02f);
            Vector3 localGravity = new Vector3(0f, -9.81f, 0f);

            Vector3 localInertia = _glassOuterBox.InverseTransformDirection(_bucketAcceleration) * _inertiaResponse;
            Vector3 effectiveGravity = localGravity - localInertia;

            float halfHeight = _cylinderHeight / 2f;
            float bounce = 0.4f;
            float safeRadius = _cylinderRadius * 0.92f; // حد الأمان لمنع الاختراق الجانبي للأسطوانة

            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                if (!p.isActive) continue;

                p.velocity += effectiveGravity * dt;

                // محرك التنافر البيني
                for (int j = 0; j < _particles.Count; j++)
                {
                    if (i == j || !_particles[j].isActive) continue;
                    Vector3 diff = p.localPosition - _particles[j].localPosition;
                    float dist = diff.magnitude;
                    float targetDist = _particleSize * 2.2f;

                    if (dist < targetDist && dist > 0.001f)
                    {
                        float forceFactor = 1f - (dist / targetDist);
                        p.velocity += diff.normalized * forceFactor * _repulsionForce * dt;
                    }
                }

                p.localPosition += p.velocity * dt;

                // === معالجة الجدران الدائرية للأسطوانة (Radial Collision) ===
                Vector3 horizontalPos = new Vector3(p.localPosition.x, 0f, p.localPosition.z);
                float currentRadius = horizontalPos.magnitude;

                if (currentRadius > safeRadius)
                {
                    // إرجاع الجزيء إلى حدود الجدار الدائري الداخلي
                    Vector3 radialNormal = horizontalPos.normalized;
                    p.localPosition.x = radialNormal.x * safeRadius;
                    p.localPosition.z = radialNormal.z * safeRadius;

                    // عكس مركبة السرعة الأفقية المتجهة نحو الخارج ليرتد للداخل
                    Vector3 horizontalVelocity = new Vector3(p.velocity.x, 0f, p.velocity.z);
                    float normalVelocityDot = Vector3.Dot(horizontalVelocity, radialNormal);

                    if (normalVelocityDot > 0f) // يتحرك باتجاه الخارج
                    {
                        Vector3 reflectedHorizontal = horizontalVelocity - (1f + bounce) * normalVelocityDot * radialNormal;
                        p.velocity.x = reflectedHorizontal.x;
                        p.velocity.z = reflectedHorizontal.z;
                    }
                }

                // === معالجة السقف والقاع (Y Axis Constraints) ===
                float maxAllowedHeight = halfHeight * 0.95f;

                if (p.localPosition.y > maxAllowedHeight)
                {
                    p.localPosition.y = maxAllowedHeight;
                    p.velocity.y = -p.velocity.y * bounce;
                }
                else if (p.localPosition.y < -halfHeight * 0.92f)
                {
                    p.localPosition.y = -halfHeight * 0.92f;
                    p.velocity.y = -p.velocity.y * bounce;
                }
            }
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                if (_visuals[i] != null)
                {
                    _visuals[i].gameObject.SetActive(_particles[i].isActive);
                    if (_particles[i].isActive)
                    {
                        _visuals[i].localPosition = _particles[i].localPosition;
                    }
                }
            }
        }

        public void RemoveParticle()
        {
            float maxY = float.MinValue;
            int targetIndex = -1;

            for (int i = 0; i < _particles.Count; i++)
            {
                if (_particles[i].isActive && _particles[i].localPosition.y > maxY)
                {
                    maxY = _particles[i].localPosition.y;
                    targetIndex = i;
                }
            }

            if (targetIndex >= 0)
            {
                _particles[targetIndex].isActive = false;
                _activeParticlesRemaining--;
            }
        }

        public void ResetVolume()
        {
            _bucketVelocity = Vector3.zero;
            _bucketAcceleration = Vector3.zero;

            if (_bucketTransform != null)
                _previousBucketPos = _bucketTransform.position;

            CalculateInitialCountFromMass();
            InitializeParticles(_currentConfiguredCount);
        }
    }
}