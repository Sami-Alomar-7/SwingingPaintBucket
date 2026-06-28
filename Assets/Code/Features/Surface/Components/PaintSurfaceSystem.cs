// الملف: PaintSurfaceSystem.cs
using UnityEngine;
using SwingingPaintBucket.Features.Surface.Data;

namespace SwingingPaintBucket.Features.Surface.Components
{
    public struct SurfaceCell
    {
        public float Absorption;
        public float Roughness;
        public float PaintAccumulation;
        public Color CurrentColor;
    }

    [RequireComponent(typeof(Renderer))]
    public class PaintSurfaceSystem : MonoBehaviour
    {
        [Header("Surface Configuration")]
        public SurfaceMaterialType materialType = SurfaceMaterialType.Wood;
        public int textureSize = 512;
        public Color baseColor = Color.white;
        [Range(0.05f, 2f)] public float paintSizeMultiplier = 1f;

        [Header("PBR Material Presets (Visual Appearance)")]
        [SerializeField] private Material woodMaterial;
        [SerializeField] private Material metalMaterial;
        [SerializeField] private Material paperMaterial; // هنا نضع المادة البيضاء الافتراضية للأرضية
        [SerializeField] private Material glassMaterial; // هنا نضع مادة الزجاج الشفاف

        [Header("Base Textures For Code (Must have Read/Write enabled)")]
        [SerializeField] private Texture2D woodBaseTexture;
        [SerializeField] private Texture2D metalBaseTexture;
        [SerializeField] private Texture2D paperBaseTexture;
        [SerializeField] private Texture2D glassBaseTexture;

        private SurfaceCell[,] _surfaceGrid;
        private Texture2D _texture;
        private Renderer _rend;
        private Material _instanceMaterial;
        private Bounds _surfaceBounds;
        private bool _isDirty = false;

        private void Awake()
        {
            InitializeSurface();
        }

        public void InitializeSurface()
        {
            _rend = GetComponent<Renderer>();

            // جلب حدود السطح الفيزيائية بدقة في الفراغ ثلاثي الأبعاد
            var col = GetComponent<Collider>();
            _surfaceBounds = col != null ? col.bounds : _rend.bounds;

            _surfaceGrid = new SurfaceCell[textureSize, textureSize];

            // 1. تحديد قيم الخواص البرمجية واختيار خامة السطح المناسبة ديناميكياً
            float absorption = 0.4f;
            float roughness = 0.2f;
            Texture2D selectedBaseTex = woodBaseTexture;
            Material selectedMaterial = paperMaterial; // افتراضياً يبدأ بالورق/الأرضية البيضاء

            switch (materialType)
            {
                case SurfaceMaterialType.Wood:
                    absorption = 0.3f; roughness = 0.7f;
                    selectedBaseTex = woodBaseTexture;
                    selectedMaterial = woodMaterial;
                    break;
                case SurfaceMaterialType.Metal:
                    absorption = 0.0f; roughness = 0.1f;
                    selectedBaseTex = metalBaseTexture;
                    selectedMaterial = metalMaterial;
                    break;
                case SurfaceMaterialType.Paper:
                    absorption = 0.8f; roughness = 0.2f;
                    selectedBaseTex = paperBaseTexture;
                    selectedMaterial = paperMaterial;
                    break;
                case SurfaceMaterialType.Glass:
                    absorption = 0.0f; roughness = 0.0f;
                    selectedBaseTex = glassBaseTexture;
                    selectedMaterial = glassMaterial;
                    break;
            }

            // 2. تطبيق الماتيريال المختار على الأرضية بصرياً فوراً
            if (_rend != null && selectedMaterial != null)
            {
                // إنشاء نسخة فريدة من المادة حتى لا نعدل على الملف الأصلي في المشروع
                _instanceMaterial = new Material(selectedMaterial);
                _rend.material = _instanceMaterial;
            }
            else if (_rend != null)
            {
                _instanceMaterial = new Material(_rend.sharedMaterial);
                _rend.material = _instanceMaterial;
            }

            // إنشاء التكستشر الديناميكي الذي سيعرض النتيجة فوراً
            _texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[textureSize * textureSize];
            int idx = 0;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    _surfaceGrid[x, y].Absorption = absorption;
                    _surfaceGrid[x, y].Roughness = roughness;
                    _surfaceGrid[x, y].PaintAccumulation = 0f;

                    // قراءة تفاصيل الألوان الأصلية للتكستشر المحمل إن وجد، وإلا نعتمد الـ baseColor
                    Color cellColor = baseColor;
                    if (selectedBaseTex != null)
                    {
                        cellColor = selectedBaseTex.GetPixelBilinear((float)x / textureSize, (float)y / textureSize);
                    }

                    _surfaceGrid[x, y].CurrentColor = cellColor;
                    pixels[idx++] = cellColor;
                }
            }

            _texture.SetPixels(pixels);
            _texture.Apply();

            // ربط التكستشر الجديد بالـ Material لظهور النتيجة فوراً للعين
            if (_instanceMaterial != null)
            {
                if (_instanceMaterial.HasProperty("_BaseMap")) _instanceMaterial.SetTexture("_BaseMap", _texture);
                if (_instanceMaterial.HasProperty("_MainTex")) _instanceMaterial.SetTexture("_MainTex", _texture);
            }
        }

        // تحويل الموضع ثلاثي الأبعاد (World Position) للجزيء المصطدم إلى إحداثيات مصفوفة (X, Y)
        public bool WorldToGridCoords(Vector3 worldPos, out int x, out int y)
        {
            x = 0; y = 0;

            // حساب مكان النقطة بنسبة مئوية بين حدود السطح الدنيا والعليا
            float pctX = Mathf.InverseLerp(_surfaceBounds.min.x, _surfaceBounds.max.x, worldPos.x);
            float pctZ = Mathf.InverseLerp(_surfaceBounds.min.z, _surfaceBounds.max.z, worldPos.z);

            // التحقق من أن الاصطدام يقع ضمن الحدود الفعلية للسطح
            if (pctX < 0f || pctX > 1f || pctZ < 0f || pctZ > 1f) return false;

            x = Mathf.Clamp((int)(pctX * textureSize), 0, textureSize - 1);
            y = Mathf.Clamp((int)(pctZ * textureSize), 0, textureSize - 1);
            return true;
        }

        // النواة الحسابية: رسم وتعديل سلوك البقعة حسب الخواص الفيزيائية للخلية بالزمن الحقيقي (لم تتغير)
        public void ApplyPhysicalPaint(int cx, int cy, Color liquidColor, int baseRadius)
        {
            if (_texture == null) return;

            SurfaceCell targetCell = _surfaceGrid[cx, cy];

            // معادلة الانتشار: الامتصاص العالي (مثل الورق) يكبر البقعة، والخشونة (مثل الخشب) تقيدها
            float radiusFactor = 1f + (targetCell.Absorption * 0.6f) - (targetCell.Roughness * 0.3f);
            int finalRadius = Mathf.RoundToInt(baseRadius * radiusFactor * paintSizeMultiplier);
            finalRadius = Mathf.Max(2, finalRadius);

            int r2 = finalRadius * finalRadius;

            for (int dx = -finalRadius; dx <= finalRadius; dx++)
            {
                for (int dy = -finalRadius; dy <= finalRadius; dy++)
                {
                    int px = cx + dx;
                    int py = cy + dy;

                    // التأكد من عدم الخروج عن حدود المصفوفة
                    if (px >= 0 && px < textureSize && py >= 0 && py < textureSize)
                    {
                        float sqrDistance = dx * dx + dy * dy;
                        if (sqrDistance > r2) continue;

                        // تأثير الخشونة (Roughness): تشويه حواف البقعة عشوائياً لتبدو واقعية وغير دائرية تماماً
                        if (sqrDistance > r2 * 0.6f && Random.value < targetCell.Roughness * 0.5f) continue;

                        _surfaceGrid[px, py].PaintAccumulation += 0.1f;

                        // دمج الألوان بسلاسة (Lerp)
                        Color blendedColor = Color.Lerp(_surfaceGrid[px, py].CurrentColor, liquidColor, 0.75f);
                        _surfaceGrid[px, py].CurrentColor = blendedColor;

                        // الرسم المباشر على التكستشر
                        _texture.SetPixel(px, py, blendedColor);
                        _isDirty = true;
                    }
                }
            }
        }

        public void PaintAtUV(Vector2 uv, Color liquidColor, int baseRadius)
        {
            int cx = Mathf.Clamp((int)(uv.x * textureSize), 0, textureSize - 1);
            int cy = Mathf.Clamp((int)(uv.y * textureSize), 0, textureSize - 1);
            ApplyPhysicalPaint(cx, cy, liquidColor, baseRadius);
        }

        public void PaintAtUVCoordinates(Vector2 uv, Color liquidColor, int baseRadius)
        {
            PaintAtUV(uv, liquidColor, baseRadius);
        }

        private void LateUpdate()
        {
            if (_isDirty && _texture != null)
            {
                _texture.Apply(false);
                _isDirty = false;
            }
        }
    }
}