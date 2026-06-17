using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;

namespace SwingingPaintBucket.Features.Paint.Components
{
    public class ParticleRenderer : MonoBehaviour
    {
        [Header("Particle Visual Settings")]
        public GameObject particlePrefab;
        public Material particleMaterial;

        private readonly List<GameObject> _pool = new List<GameObject>();
        private Transform _particleParent;

        private MaterialPropertyBlock _propertyBlock;
        private static readonly int ColorShaderID = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            var parentGO = new GameObject("SPH_ParticlePool");
            _particleParent = parentGO.transform;
            parentGO.hideFlags = HideFlags.HideInHierarchy;
        }

        public void RenderParticles(List<ParticleData> particles, float defaultSize, Color defaultColor)
        {
            while (_pool.Count < particles.Count)
                _pool.Add(CreateParticleObject());

            for (int i = 0; i < particles.Count; i++)
            {
                ParticleData p = particles[i];
                GameObject go = _pool[i];

                if (!go.activeSelf) go.SetActive(true);

                go.transform.position = p.position;

                float size = p.size > 0f ? p.size : defaultSize;
                go.transform.localScale = Vector3.one * Mathf.Max(size, 0.01f);

                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.GetPropertyBlock(_propertyBlock);
                    Color targetColor = (p.color != Color.clear) ? p.color : defaultColor;
                    _propertyBlock.SetColor(ColorShaderID, targetColor);
                    rend.SetPropertyBlock(_propertyBlock);
                }
            }

            for (int i = particles.Count; i < _pool.Count; i++)
            {
                if (_pool[i].activeSelf) _pool[i].SetActive(false);
            }
        }

        public void Clear()
        {
            foreach (var go in _pool)
            {
                if (go != null && go.activeSelf) go.SetActive(false);
            }
        }

        private GameObject CreateParticleObject()
        {
            GameObject go;
            if (particlePrefab != null)
            {
                go = Instantiate(particlePrefab, _particleParent);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.SetParent(_particleParent);

                Collider col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);

                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.sharedMaterial = (particleMaterial != null) ? particleMaterial : new Material(Shader.Find("Unlit/Color"));
                }
            }
            go.SetActive(false);
            return go;
        }
    }
}