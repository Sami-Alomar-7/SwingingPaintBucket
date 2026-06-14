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

        private void Awake()
        {
            // إنشاء كائن أب لجمع الجسيمات
            var parentGO = new GameObject("SPH_ParticlePool");
            _particleParent = parentGO.transform;

            // السطر السحري: إخفاء الكرات والـ Pool تماماً من نافذة الـ Hierarchy لمنع الازدحام
            parentGO.hideFlags = HideFlags.HideInHierarchy;
        }

        public void RenderParticles(List<ParticleData> particles, float defaultSize, Color defaultColor)
        {
            // توسيع الـ Pool إذا زاد عدد الجسيمات الحالية
            while (_pool.Count < particles.Count)
                _pool.Add(CreateParticleObject());

            for (int i = 0; i < particles.Count; i++)
            {
                ParticleData p = particles[i];
                GameObject go = _pool[i];

                if (!go.activeSelf) go.SetActive(true);

                go.transform.position = p.position;

                // التحكم في الحجم بناءً على إعدادات الـ Config أو خصائص الجسيم
                float size = p.size > 0f ? p.size : defaultSize;
                go.transform.localScale = Vector3.one * Mathf.Max(size, 0.01f);

                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    // تلوين الجسيم بلونه الخاص (أو الافتراضي)
                    rend.material.color = (p.color != Color.clear) ? p.color : defaultColor;
                }
            }

            // إخفاء الجسيمات الزائدة غير المستخدمة حالياً
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

                // تدمير الـ Collider لمنع تصادم الجسيمات ببعضها فيزيائياً عبر محرك يوينيتي (لأننا نحسب الـ SPH بأنفسنا)
                Collider col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);

                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = (particleMaterial != null) ? particleMaterial : new Material(Shader.Find("Unlit/Color"));
                }
            }
            go.SetActive(false);
            return go;
        }
    }
}