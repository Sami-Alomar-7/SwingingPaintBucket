using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Paint.Services;
using SwingingPaintBucket.Features.Pendulum.Components; // إضافة النطاق الخاص بالمتحكم

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
        [SerializeField] private PendulumController _pendulumController; // مرجع للمتحكم الأساسي لقراءة القطر

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

        [Header("UV Mapping Adjustments")]
        public bool invertX = false;
        public bool invertZ = false;

        private readonly List<ParticleData> _particles = new List<ParticleData>();
        private float _emissionTimer;
        private Vector3 _previousBucketPosition;
        private bool _isRunning;
        private bool _emissionCutoff;
        private Renderer _surfaceRenderer;

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
        // داخل ملف PaintEmitter.cs - دالة Update
        // داخل ملف PaintEmitter.cs - دالة Update المعدلة
        private void Update()
        {
            if (!_isRunning) return;

            // الشرط المنقذ: إذا فرغ المكعب تماماً، اقطعي الرش فوراً ولا تطلقي أي جزيئات خارجية!
            if (_bucketLiquidVolume != null && _bucketLiquidVolume.IsEmpty)
            {
                _emissionCutoff = true;
                ClearParticles(); // تنظيف الجزيئات المعلقة في الهواء ليتوقف الرسم فوراً
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

                float currentDiameter = _pendulumController != null ? _pendulumController.CurrentApertureDiameter : emissionConfig.holeDiameter;
                emissionConfig.holeDiameter = currentDiameter;

                float spawnRate = emissionService.CalculateEmissionRate(bucketVelocity, emissionConfig);
                float interval = spawnRate > 0f ? 1f / spawnRate : float.MaxValue;

                while (_emissionTimer >= interval)
                {
                    _emissionTimer -= interval;

                    if (_particles.Count < 600)
                    {
                        Vector3 origin = spawnPoint != null ? spawnPoint.position : (bucket != null ? bucket.position - bucket.up * 0.3f : transform.position);

                        float r = currentDiameter * 0.5f;
                        Vector3 randomOffset = new Vector3(Random.Range(-r, r), 0f, Random.Range(-r, r));

                        Vector3 finalSpawnPos = origin + (spawnPoint != null ? spawnPoint.TransformDirection(randomOffset) : (bucket != null ? bucket.TransformDirection(randomOffset) : randomOffset));

                        emissionConfig.particleSize = Mathf.Clamp(currentDiameter * 1.5f, 0.02f, 0.25f);

                        emissionService.EmitParticle(emissionConfig, finalSpawnPos, bucketVelocity, _particles);
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
        private void FixedUpdate()
        {
            if (!_isRunning) return;

            if (particlePhysicsService != null)
            {
                float surfaceY = surfaceSurface != null ? surfaceSurface.position.y : 0f;
                List<int> toRemove = particlePhysicsService.UpdateParticles(_particles, Time.fixedDeltaTime, surfaceY, emissionConfig);

                for (int i = 0; i < _particles.Count; i++)
                {
                    ParticleData p = _particles[i];
                    if (p.position.y <= surfaceY + 0.03f && paintSurfaceSystem != null)
                    {
                        Vector2 uv = WorldToSurfaceUV(p.position);
                        int baseBrush = Mathf.Max(1, (int)(emissionConfig.particleSize * 150f));
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