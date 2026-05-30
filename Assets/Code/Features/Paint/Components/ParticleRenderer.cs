using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;

namespace SwingingPaintBucket.Features.Paint.Components
{
    /// <summary>
    /// Renders paint particles as small sphere GameObjects.
    /// Creates its own sphere primitives if no prefab is assigned —
    /// so particles are ALWAYS visible without any manual setup.
    /// Uses a pooling strategy: objects are reused, never destroyed mid-frame.
    /// </summary>
    public class ParticleRenderer : MonoBehaviour
    {
        [Header("Particle Visual Settings")]
        [Tooltip("Optional prefab. If null, a sphere primitive is created automatically.")]
        public GameObject particlePrefab;

        [Tooltip("Optional shared material. If null, a new unlit material is created per particle.")]
        public Material particleMaterial;

        // Pool of reusable GameObjects
        private readonly List<GameObject> _pool = new List<GameObject>();

        // Parent transform to keep hierarchy clean
        private Transform _particleParent;

        private void Awake()
        {
            // Create a dedicated parent so particles don't clutter the root hierarchy
            var parentGO = new GameObject("ParticlePool");
            parentGO.transform.SetParent(null);
            _particleParent = parentGO.transform;
        }

        /// <summary>
        /// Syncs the pool to match the current particle list.
        /// Each particle's position, scale, and color are updated every frame.
        /// </summary>
        public void RenderParticles(List<ParticleData> particles, float defaultSize, Color defaultColor)
        {
            // Grow pool if needed
            while (_pool.Count < particles.Count)
                _pool.Add(CreateParticleObject());

            // Update active particles
            for (int i = 0; i < particles.Count; i++)
            {
                ParticleData p = particles[i];
                GameObject go = _pool[i];

                go.SetActive(true);
                go.transform.position   = p.position;
                go.transform.localScale = Vector3.one * Mathf.Max(p.size, 0.01f);

                // Apply per-particle color
                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = p.color;
            }

            // Hide excess pool objects
            for (int i = particles.Count; i < _pool.Count; i++)
                _pool[i].SetActive(false);
        }

        /// <summary>Hides all pooled objects without destroying them.</summary>
        public void Clear()
        {
            foreach (var go in _pool)
                if (go != null) go.SetActive(false);
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private GameObject CreateParticleObject()
        {
            GameObject go;

            if (particlePrefab != null)
            {
                go = Instantiate(particlePrefab, _particleParent);
            }
            else
            {
                // Auto-create a sphere so particles are always visible
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.SetParent(_particleParent);

                // Remove collider — particles are purely visual
                Destroy(go.GetComponent<Collider>());

                // Create a fresh unlit material so each particle can have its own color
                // without modifying any shared asset
                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    // Use Unlit/Color if available, otherwise Standard
                    Shader shader = Shader.Find("Unlit/Color");
                    if (shader == null) shader = Shader.Find("Standard");
                    if (shader != null)
                        rend.material = new Material(shader);
                }
            }

            go.SetActive(false);
            return go;
        }
    }
}
