using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Paint.Services;

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
        [Tooltip("فعلي هذا الخيار إذا كانت البقع معكوسة يميناً ويساراً")]
        public bool invertX = false;
        [Tooltip("فعلي هذا الخيار إذا كانت البقع معكوسة للأمام والخلف")]
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

            if (surfaceSurface != null)
            {
                _surfaceRenderer = surfaceSurface.GetComponent<Renderer>();
                if (paintSurfaceSystem == null) paintSurfaceSystem = surfaceSurface.GetComponent<PaintSurfaceSystem>();
            }
        }

        private void Update()
        {
            if (!_isRunning) return;

            Vector3 bucketVelocity = Vector3.zero;
            if (Time.deltaTime > 0f && bucket != null)
            {
                bucketVelocity = (bucket.position - _previousBucketPosition) / Time.deltaTime;
                _previousBucketPosition = bucket.position;
            }

            if (!_emissionCutoff && emissionService != null)
            {
                _emissionTimer += Time.deltaTime;
                float spawnRate = emissionService.CalculateEmissionRate(bucketVelocity, emissionConfig);
                float interval = spawnRate > 0f ? 1f / spawnRate : float.MaxValue;

                while (_emissionTimer >= interval)
                {
                    _emissionTimer -= interval;

                    if (spawnPoint != null && _particles.Count < 400)
                    {
                        emissionService.EmitParticle(emissionConfig, spawnPoint.position, bucketVelocity, _particles);
                        massLossNotifier?.NotifyParticleEmitted(emissionConfig.particleMass);
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

                toRemove.Sort((a, b) => b.CompareTo(a));

                for (int i = 0; i < toRemove.Count; i++)
                {
                    int index = toRemove[i];
                    if (index >= 0 && index < _particles.Count)
                    {
                        ParticleData targetParticle = _particles[index];

                        if (targetParticle.position.y <= surfaceY + 0.05f && paintSurfaceSystem != null)
                        {
                            Vector2 uvCoords = WorldToSurfaceUV(targetParticle.position);

                            paintSurfaceSystem.PaintAtUV(uvCoords, targetParticle.color, 1);
                        }

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