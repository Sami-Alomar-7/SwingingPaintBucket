
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Components
{
    public class ParticleRenderer : MonoBehaviour
    {
        [Header("Particle Visual Settings")]
        public GameObject particlePrefab;
        public Material particleMaterial;

        [Header("Liquid Look")]
        [Tooltip("مضاعف الحجم البصري: يكبّر الكرات حتى تتداخل وتبدو كسائل متصل بدل كرات منفصلة")]
        [Range(1f, 3f)] public float liquidScaleMultiplier = 1.8f;
        [Tooltip("نعومة السطح (كلما زادت بدا أكثر بللاً ولمعاناً)")]
        [Range(0f, 1f)] public float smoothness = 0.92f;
        [Tooltip("شدة اللمعة البراقة على السطح")]
        [Range(0f, 4f)] public float specularIntensity = 1.6f;
        [Tooltip("قوة حافة فرينل المضيئة على أطراف الكرة")]
        [Range(0f, 2f)] public float fresnelStrength = 0.7f;
        [Tooltip("شفافية ضوئية: عبور الضوء خلال السائل (Back-scatter)")]
        [Range(0f, 2f)] public float translucency = 0.45f;

        [Header("Velocity Stretch (تمطيط القطرات السريعة)")]
        [Tooltip("تمطيط الجزيء بمحاذاة سرعته فيبدو كخيط سائل متدفق أثناء الحركة")]
        public bool velocityStretch = true;
        [Tooltip("أدنى سرعة يبدأ بعدها التمطيط (تحتها يبقى الجزيء دائرياً)")]
        public float stretchThreshold = 1.5f;
        [Tooltip("معدل نمو التمطيط مع السرعة")]
        public float stretchPerSpeed = 0.12f;
        [Tooltip("أقصى نسبة تمطيط مسموحة")]
        [Range(1f, 5f)] public float maxStretch = 3f;

        [Header("Density Color Depth (عمق اللون حسب الكثافة)")]
        [Tooltip("استخدام كثافة SPH لجعل التجمعات الكثيفة أعمق لوناً والأطراف الرقيقة أفتح")]
        public bool densityColorDepth = true;
        [Tooltip("الكثافة المرجعية للمعايرة (طابقها مع restDensity في إعدادات الانبعاث)")]
        public float referenceDensity = 100f;

        private readonly List<GameObject> _pool = new List<GameObject>();
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private Transform _particleParent;
        private Material _liquidMaterial;

        private MaterialPropertyBlock _propertyBlock;
        private static readonly int ColorShaderID = Shader.PropertyToID("_Color");
        private static readonly int BaseColorShaderID = Shader.PropertyToID("_BaseColor"); // توافق URP Lit البديل

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
            {
                GameObject created = CreateParticleObject();
                _pool.Add(created);
                _renderers.Add(created.GetComponentInChildren<Renderer>());
            }

            for (int i = 0; i < particles.Count; i++)
            {
                ParticleData p = particles[i];
                GameObject go = _pool[i];

                if (!go.activeSelf) go.SetActive(true);

                go.transform.position = p.position;

                // تكبير الحجم البصري ليتداخل الجزيء مع جيرانه فيظهر العنقود ككتلة سائلة متصلة
                float size = p.size > 0f ? p.size : defaultSize;
                float baseScale = Mathf.Max(size * liquidScaleMultiplier, 0.01f);

                ApplyShapeAndOrientation(go.transform, p.velocity, baseScale);
                 Renderer rend = _renderers[i];
                if (rend != null)
                {
                    rend.GetPropertyBlock(_propertyBlock);
                    Color baseColor = (p.color != Color.clear) ? p.color : defaultColor;
                    Color targetColor = densityColorDepth ? ApplyDensityDepth(baseColor, p.density) : baseColor;
                    _propertyBlock.SetColor(ColorShaderID, targetColor);
                    _propertyBlock.SetColor(BaseColorShaderID, targetColor);
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

        // تمطيط الجزيء بمحاذاة سرعته (مع الحفاظ التقريبي على الحجم) فيبدو كخيط سائل متدفق
        private void ApplyShapeAndOrientation(Transform t, Vector3 velocity, float baseScale)
        {
            float speed = velocity.magnitude;

            if (!velocityStretch || speed <= stretchThreshold)
            {
                t.rotation = Quaternion.identity;
                t.localScale = Vector3.one * baseScale;
                return;
            }

            float stretch = Mathf.Min(1f + (speed - stretchThreshold) * stretchPerSpeed, maxStretch);
            float shrink = 1f / Mathf.Sqrt(stretch); // تقليص المحورين العرضيين لتقريب حفظ الحجم

            Vector3 dir = velocity / speed;
            t.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            t.localScale = new Vector3(baseScale * shrink, baseScale * stretch, baseScale * shrink);
        }

        // استخدام كثافة SPH لإضافة عمق لوني: التجمعات الكثيفة أعمق، الأطراف الرقيقة أفتح قليلاً
        private Color ApplyDensityDepth(Color baseColor, float density)
        {
            float refDensity = Mathf.Max(referenceDensity, 0.0001f);
            float depth = Mathf.InverseLerp(refDensity * 0.3f, refDensity * 1.5f, density);
            float brightness = Mathf.Lerp(1.15f, 0.8f, depth); // حد آمن [0.8 , 1.15] لتفادي أي شذوذ

            return new Color(
                Mathf.Clamp01(baseColor.r * brightness),
                Mathf.Clamp01(baseColor.g * brightness),
                Mathf.Clamp01(baseColor.b * brightness),
                baseColor.a);
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
                    // خامة سائل لامعة واحدة مشتركة بين كل الجزيئات (اللون يتغير لكل جزيء عبر PropertyBlock)
                    rend.sharedMaterial = GetOrCreateLiquidMaterial();
                    rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    rend.receiveShadows = false;
                }
            }
            go.SetActive(false);
            return go;
        }

        private Material GetOrCreateLiquidMaterial()
        {
            // احترام أي خامة معيّنة يدوياً من المفتش
            if (particleMaterial != null)
            {
                ApplyLiquidProperties(particleMaterial);
                return particleMaterial;
            }

            if (_liquidMaterial != null) return _liquidMaterial;
           // تسلسل بدائل آمن: الشيدر المخصص ← URP Lit ← Unlit/Color
            Shader liquidShader = Shader.Find("SwingingPaintBucket/LiquidParticle");
            if (liquidShader == null) liquidShader = Shader.Find("Universal Render Pipeline/Lit");
            if (liquidShader == null) liquidShader = Shader.Find("Unlit/Color");

            _liquidMaterial = new Material(liquidShader);
            ApplyLiquidProperties(_liquidMaterial);
            return _liquidMaterial;
        }

        private void ApplyLiquidProperties(Material m)
        {
            if (m == null) return;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_SpecIntensity")) m.SetFloat("_SpecIntensity", specularIntensity);
            if (m.HasProperty("_FresnelStrength")) m.SetFloat("_FresnelStrength", fresnelStrength);
            if (m.HasProperty("_Translucency")) m.SetFloat("_Translucency", translucency);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0.1f); // للبديل URP Lit
        }
    }
}