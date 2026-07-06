using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Paint.Services;
using SwingingPaintBucket.Features.Pendulum.Components;

namespace SwingingPaintBucket.Features.Paint.Components
{
    [RequireComponent(typeof(ParticleRenderer))]
    public class PaintEmitter : MonoBehaviour
    {
        [Header("Scene References")]
        public Transform spawnPoint;
        public Transform bucket;
        public Transform surfaceSurface;
        public PaintSurfaceSystem paintSurfaceSystem;
        [SerializeField] private BucketLiquidVolume _bucketLiquidVolume;
        [SerializeField] private PendulumController _pendulumController;

        public object surfaceService
        {
            get
            {
                if (paintSurfaceSystem != null) return paintSurfaceSystem;
                if (surfaceSurface != null) return surfaceSurface.GetComponent<PaintSurfaceSystem>();
                return null;
            }
            set
            {
                if (value is PaintSurfaceSystem system) paintSurfaceSystem = system;
                else if (surfaceSurface != null) paintSurfaceSystem = surfaceSurface.GetComponent<PaintSurfaceSystem>();
            }
        }

        [Header("Abstractions & Core Interfaces")]
        public IPaintEmissionService emissionService;
        public IParticlePhysicsService particlePhysicsService;

        [Header("Mass & Component References")]
        public IMassLossNotifier massLossNotifier;
        public ParticleRenderer particleRenderer;

        [Header("Emission Configurations")]
        public PaintEmissionConfig emissionConfig = new PaintEmissionConfig();

        [Header("Sieve & Holes Customization")]
        [Tooltip("المسافة بين الفتحات بوحدة الـ Unity (مثلاً 0.05 تعني 5 سانتي على الأقل)")]
        [Range(0.02f, 0.2f)]
        public float holeSpacing = 0.05f; // حقل للتحكم بالتباعد (متقاربة أو متباعدة) من الـ Inspector

        [Header("UV Mapping Adjustments")]
        public bool invertX = false;
        public bool invertZ = false;

        private readonly List<ParticleData> _particles = new List<ParticleData>();
        private float _emissionTimer;
        private Vector3 _previousBucketPosition;
        private bool _isRunning;
        private bool _emissionCutoff;
        private Renderer _surfaceRenderer;

        // مصفوفة لتخزين مواقع الثقوب بصيغة المنخلة بالنسبة لمركز الدلو
        private Vector3[] _holeOffsets;
        private int _lastHoleIndex = 0;
        private int _cachedHolesCount = -1;
        private float _cachedHoleSpacing = -1f; // كاش لمراقبة تغير المسافة أيضاً

        private void Awake()
        {
            if (particleRenderer == null) particleRenderer = GetComponent<ParticleRenderer>();
            if (emissionService == null) emissionService = new DynamicPaintEmissionService();
            if (particlePhysicsService == null) particlePhysicsService = new ParticlePhysicsService();
            if (_bucketLiquidVolume == null) _bucketLiquidVolume = FindAnyObjectByType<BucketLiquidVolume>();
            if (_pendulumController == null) _pendulumController = FindAnyObjectByType<PendulumController>();

            if (surfaceSurface != null)
            {
                _surfaceRenderer = surfaceSurface.GetComponent<Renderer>();
                if (paintSurfaceSystem == null) paintSurfaceSystem = surfaceSurface.GetComponent<PaintSurfaceSystem>();
            }
        }

        private void Update()
        {
            if (!_isRunning) return;

            if (_bucketLiquidVolume != null && _bucketLiquidVolume.IsEmpty)
            {
                _emissionCutoff = true;
                ClearParticles();
                return;
            }

            Vector3 bucketVelocity = Vector3.zero;
            if (Time.deltaTime > 0f && bucket != null)
            {
                bucketVelocity = (bucket.position - _previousBucketPosition) / Time.deltaTime;
                _previousBucketPosition = bucket.position;
            }

            if (!_emissionCutoff && emissionService != null)
            {
                _emissionTimer += Time.deltaTime;

                // جلب القيم الحالية من كود التحكم بالبندول (القطر وعدد الثقوب الفعلي)
                float currentDiameter = _pendulumController != null ? _pendulumController.CurrentApertureDiameter : 0.01f;
                int holesCount = _pendulumController != null ? _pendulumController.HolesCount : 1;

                // إذا تغير عدد الثقوب أو تغيرت قيمة التباعد أثناء التشغيل، نعيد حساب التوزيع فوراً
                if (holesCount != _cachedHolesCount || !Mathf.Approximately(holeSpacing, _cachedHoleSpacing))
                {
                    GenerateSievePattern(holesCount);
                }

                // حساب مساحة الثقب الواحد مقارنة بالأساسي لتحديد دقة التدفق
                float radius = currentDiameter * 0.5f;
                float baseRadius = 0.005f;
                float singleHoleAreaRatio = (radius * radius) / (baseRadius * baseRadius);

                // حساب معدل التدفق الإجمالي (معدل التدفق للثقب الواحد مضروباً في عدد الثقوب المتوفرة)
                float baseSpawnRate = emissionService.CalculateEmissionRate(bucketVelocity, emissionConfig);
                float totalSpawnRate = baseSpawnRate * singleHoleAreaRatio * holesCount;
                float spawnRate = Mathf.Clamp(totalSpawnRate, 30f, 5000f);

                float interval = spawnRate > 0f ? 1f / spawnRate : float.MaxValue;

                while (_emissionTimer >= interval)
                {
                    _emissionTimer -= interval;

                    if (_particles.Count < 5000) // حد الجزيئات الأقصى
                    {
                        Vector3 origin = spawnPoint != null ? spawnPoint.position : (bucket != null ? bucket.position - bucket.up * 0.3f : transform.position);

                        // اختيار الثقب التالي بالدور
                        Vector3 chosenOffset = Vector3.zero;
                        if (_holeOffsets != null && _holeOffsets.Length > 0)
                        {
                            _lastHoleIndex = (_lastHoleIndex + 1) % _holeOffsets.Length;
                            chosenOffset = _holeOffsets[_lastHoleIndex];
                        }

                        // توزيع عشوائي طفيف جداً داخل حدود الثقب الصغير نفسه
                        Vector2 innerCircle = Random.insideUnitCircle * radius;
                        Vector3 preciseOffset = chosenOffset + new Vector3(innerCircle.x, 0f, innerCircle.y);

                        Vector3 finalSpawnPos = origin + (spawnPoint != null ? spawnPoint.TransformDirection(preciseOffset) : (bucket != null ? bucket.TransformDirection(preciseOffset) : preciseOffset));

                        emissionConfig.particleSize = 0.035f;

                        Vector3 exitDir = bucket != null ? -bucket.up : Vector3.down;
                        float effluxSpeed = Mathf.Sqrt(2f * 9.81f * 0.3f);
                        Vector3 particleInitialVelocity = bucketVelocity + (exitDir * effluxSpeed);

                        emissionService.EmitParticle(emissionConfig, finalSpawnPos, particleInitialVelocity, _particles);
                        massLossNotifier?.NotifyParticleEmitted(emissionConfig.particleMass);

                        if (_bucketLiquidVolume != null)
                        {
                            _bucketLiquidVolume.RemoveParticle();
                        }
                    }
                }
            }

            RenderParticles();
        }

        /// <summary>
        /// توليد توزيع هندسي دائري منتظم للثقوب بتباعد دقيق ومتحكم به
        /// </summary>
        private void GenerateSievePattern(int count)
        {
            _cachedHolesCount = count;
            _cachedHoleSpacing = holeSpacing;
            _holeOffsets = new Vector3[count];

            if (count <= 1)
            {
                _holeOffsets[0] = Vector3.zero;
                return;
            }

            // الثقب الأول دائماً في المركز
            _holeOffsets[0] = Vector3.zero;

            int remaining = count - 1;
            int currentHoleIndex = 1;
            int ringIndex = 1;

            // الحلقات تتوزع بناءً على التباعد المحدد من قبل المستخدم
            while (remaining > 0)
            {
                float ringRadius = ringIndex * holeSpacing;

                // لحساب محيط الدائرة ومعرفة كم ثقب يتسع بمسافة أمان تساوي holeSpacing:
                // المحيط = 2 * PI * ringRadius. نقسمه على holeSpacing ليعطينا السعة القصوى الهندسية للحلقة
                int maxHolesInRing = Mathf.FloorToInt((2f * Mathf.PI * ringRadius) / holeSpacing);

                // نضمن ألا يقل عدد الثقوب بالحلقة عن 6 لتكوين شكل هندسي متناسق إلا إذا كان المتبقي أقل
                int holesInRing = Mathf.Clamp(maxHolesInRing, 6, remaining);
                holesInRing = Mathf.Min(holesInRing, remaining);

                for (int i = 0; i < holesInRing; i++)
                {
                    float angle = i * (2f * Mathf.PI / holesInRing);
                    float x = Mathf.Cos(angle) * ringRadius;
                    float z = Mathf.Sin(angle) * ringRadius;

                    _holeOffsets[currentHoleIndex] = new Vector3(x, 0f, z);
                    currentHoleIndex++;
                }

                remaining -= holesInRing;
                ringIndex++;
            }
        }

        private void FixedUpdate()
        {
            if (!_isRunning) return;

            if (particlePhysicsService != null)
            {
                float surfaceY = surfaceSurface != null ? surfaceSurface.position.y : 0f;
                List<int> toRemove = particlePhysicsService.UpdateParticles(_particles, Time.fixedDeltaTime, surfaceY, emissionConfig);

                float currentDiameter = _pendulumController != null ? _pendulumController.CurrentApertureDiameter : 0.01f;

                for (int i = 0; i < _particles.Count; i++)
                {
                    ParticleData p = _particles[i];

                    if (p.position.y <= surfaceY + 0.04f && paintSurfaceSystem != null)
                    {
                        Vector2 uv = WorldToSurfaceUV(p.position);
                        int baseBrush = Mathf.Max(1, (int)(currentDiameter * 150f));
                        paintSurfaceSystem.PaintAtUV(uv, emissionConfig.particleColor, baseBrush);
                    }
                }

                toRemove.Sort((a, b) => b.CompareTo(a));
                for (int i = 0; i < toRemove.Count; i++)
                {
                    int index = toRemove[i];
                    if (index >= 0 && index < _particles.Count)
                    {
                        _particles.RemoveAt(index);
                    }
                }
            }
        }

        private Vector2 WorldToSurfaceUV(Vector3 worldPosition)
        {
            if (_surfaceRenderer == null) return Vector2.zero;
            Bounds bounds = _surfaceRenderer.bounds;

            float u = (worldPosition.x - bounds.min.x) / bounds.size.x;
            float z = (worldPosition.z - bounds.min.z) / bounds.size.z;

            if (invertX) u = 1f - u;
            if (invertZ) z = 1f - z;

            return new Vector2(Mathf.Clamp01(u), Mathf.Clamp01(z));
        }

        private void RenderParticles()
        {
            if (particleRenderer != null)
                particleRenderer.RenderParticles(_particles, emissionConfig.particleSize, emissionConfig.particleColor);
        }

        public void StartSimulation()
        {
            _emissionCutoff = false;
            _isRunning = true;
            if (bucket != null) _previousBucketPosition = bucket.position;
        }

        public void StopSimulation() => _isRunning = false;
        public void ClearParticles()
        {
            _particles.Clear();
            if (particleRenderer != null) particleRenderer.Clear();
        }
    }
}