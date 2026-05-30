using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;
using SwingingPaintBucket.Features.Surface.Interfaces;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Paint.Services;
using SwingingPaintBucket.Features.Pendulum.Services;

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

        [Header("Abstractions & Core Interfaces")]
        public IPaintEmissionService emissionService;
        public IParticlePhysicsService particlePhysicsService;
        public IPaintSurfaceService surfaceService; // Injected contract reference

        [Header("Mass System References")]
        public IMassLossNotifier massLossNotifier;
        public ParticleRenderer particleRenderer;

        [Header("Emission Configurations")]
        public PaintEmissionConfig emissionConfig = new PaintEmissionConfig();

        private readonly List<ParticleData> _particles = new();
        private float _emissionTimer;
        private Vector3 _previousBucketPosition;
        private bool _isRunning;
        private bool _emissionCutoff;
        private Renderer _surfaceRenderer;

        private void Awake()
        {
            if (particleRenderer == null) particleRenderer = GetComponent<ParticleRenderer>();
            
            // Local feature fallbacks are acceptable since they belong directly to the 'Paint' assembly block
            if (emissionService == null) emissionService = new DynamicPaintEmissionService();
            if (particlePhysicsService == null) particlePhysicsService = new ParticlePhysicsService();
            
            if (surfaceSurface != null) _surfaceRenderer = surfaceSurface.GetComponent<Renderer>();
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

            // Monitor empty bucket threshold triggers
            if (massLossNotifier is MassSystem massSystem && massSystem.PaintMass <= 0f)
            {
                _emissionCutoff = true;
                if (bucket != null)
                {
                    Renderer bucketRenderer = bucket.GetComponent<Renderer>();
                    if (bucketRenderer != null) bucketRenderer.material.color = Color.white;
                }
            }

            // Spawn loop cycle execution
            if (!_emissionCutoff && emissionService != null)
            {
                _emissionTimer += Time.deltaTime;
                float spawnRate = emissionService.CalculateEmissionRate(bucketVelocity, emissionConfig);
                float interval = spawnRate > 0f ? 1f / spawnRate : float.MaxValue;

                while (_emissionTimer >= interval)
                {
                    _emissionTimer -= interval;

                    // CRITICAL FIX: Check paint mass BEFORE each particle emission
                    // This prevents multiple particles from being emitted in a single frame when paint runs out
                    if (massLossNotifier is MassSystem ms && ms.PaintMass <= 0f)
                    {
                        _emissionCutoff = true;
                        if (bucket != null)
                        {
                            Renderer bucketRenderer = bucket.GetComponent<Renderer>();
                            if (bucketRenderer != null) bucketRenderer.material.color = Color.white;
                        }
                        break; // Stop emitting immediately
                    }

                    if (spawnPoint != null)
                    {
                        emissionService.EmitParticle(emissionConfig, spawnPoint.position, bucketVelocity, _particles);
                        massLossNotifier?.NotifyParticleEmitted(emissionConfig.particleMass);
                    }
                }
            }

            // Physical integration loop updates
            if (particlePhysicsService != null)
            {
                float surfaceY = surfaceSurface != null ? surfaceSurface.position.y : 0f;
                List<int> toRemove = particlePhysicsService.UpdateParticles(_particles, Time.deltaTime, emissionConfig.gravity, surfaceY);

                foreach (int index in toRemove)
                {
                    if (index >= 0 && index < _particles.Count)
                    {
                        // Clean decoupled interaction path between separate domain contexts
                        if (surfaceService != null && paintSurfaceSystem != null)
                        {
                            Texture2D targetTex = paintSurfaceSystem.GetTexture();
                            if (targetTex != null)
                            {
                                Vector2 uvCoords = WorldToSurfaceUV(_particles[index].position);
                                
                                // Convert the structural particle config size into absolute pixel scaling dimensions
                                int calculatedRadius = Mathf.Max(1, Mathf.RoundToInt(emissionConfig.particleSize * 150f));
                                
                                // Pure abstraction invocation: No downcasting or stateful Initialization needed!
                                surfaceService.Paint(targetTex, uvCoords, _particles[index].color, calculatedRadius);
                            }
                        }
                        _particles.RemoveAt(index);
                    }
                }
            }

            RenderParticles();
        }

        private Vector2 WorldToSurfaceUV(Vector3 worldPosition)
        {
            if (_surfaceRenderer == null) return Vector2.zero;
            Bounds bounds = _surfaceRenderer.bounds;

            // Pendulum swings in vertical X-Y plane at Z=0
            // Ground is horizontal X-Z plane
            // Particles fall from (x, y, 0) to ground at (x, groundY, 0)
            // Since all particles have Z=0, we map X to U and lock V to center

            // Calculate half-size for X dimension
            float halfSizeX = bounds.size.x / 2f;

            // Map world X to texture U (horizontal position on ground)
            // Invert X mapping to match camera/view orientation
            float u = Mathf.Clamp01((bounds.center.x - worldPosition.x + halfSizeX) / bounds.size.x);

            // Lock V to center of texture (0.5f) since pendulum doesn't move in Z
            // The brush radius will handle the vertical thickness
            float v = 0.5f;

            return new Vector2(u, v);
        }

        private void RenderParticles()
        {
            if (particleRenderer == null) return;
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
            if (bucket != null) _previousBucketPosition = bucket.position;
        }
    }
}