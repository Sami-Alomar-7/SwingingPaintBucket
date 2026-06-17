using UnityEngine;

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
        [Header("Surface Resolution")]
        public int textureSize = 512;

        [Header("Base Material Properties")]
        [Range(0f, 1f)] public float defaultAbsorption = 0.4f;
        [Range(0f, 1f)] public float defaultRoughness = 0.2f;
        public Color baseColor = Color.white;

        [Range(0.05f, 1f)] public float paintSizeMultiplier = 0.2f;

        private SurfaceCell[,] _surfaceGrid;
        private Texture2D _texture;
        private Renderer _rend;
        private Material _instanceMaterial;

        private void Awake()
        {
            InitializeSurface();
        }

        public void InitializeSurface()
        {
            _rend = GetComponent<Renderer>();
            _surfaceGrid = new SurfaceCell[textureSize, textureSize];

            _texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[textureSize * textureSize];
            int idx = 0;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    _surfaceGrid[x, y].Absorption = defaultAbsorption;
                    _surfaceGrid[x, y].Roughness = defaultRoughness;
                    _surfaceGrid[x, y].PaintAccumulation = 0f;
                    _surfaceGrid[x, y].CurrentColor = baseColor;

                    pixels[idx++] = baseColor;
                }
            }

            _texture.SetPixels(pixels);
            _texture.Apply();

            if (_rend != null)
            {
                _instanceMaterial = new Material(_rend.sharedMaterial);
                _rend.material = _instanceMaterial;

                if (_instanceMaterial.HasProperty("_BaseMap")) _instanceMaterial.SetTexture("_BaseMap", _texture);
                if (_instanceMaterial.HasProperty("_MainTex")) _instanceMaterial.SetTexture("_MainTex", _texture);
            }
        }

        public void PaintAtUV(Vector2 uv, Color liquidColor, int baseRadius)
        {
            PaintAtUVCoordinates(uv, liquidColor, baseRadius);
        }

        public void PaintAtUVCoordinates(Vector2 uv, Color liquidColor, int baseRadius)
        {
            if (_texture == null) return;

            int cx = Mathf.Clamp((int)(uv.x * textureSize), 0, textureSize - 1);
            int cy = Mathf.Clamp((int)(uv.y * textureSize), 0, textureSize - 1);

            SurfaceCell targetCell = _surfaceGrid[cx, cy];

            float surfaceEffectFactor = 1f + (targetCell.Absorption * 0.4f) - (targetCell.Roughness * 0.2f);
            int finalRadius = Mathf.RoundToInt(baseRadius * surfaceEffectFactor * paintSizeMultiplier);
            finalRadius = Mathf.Max(1, finalRadius);

            int r2 = finalRadius * finalRadius;
            bool contentAltered = false;

            for (int dx = -finalRadius; dx <= finalRadius; dx++)
            {
                for (int dy = -finalRadius; dy <= finalRadius; dy++)
                {
                    if (dx * dx + dy * dy > r2) continue;

                    int px = cx + dx;
                    int py = cy + dy;

                    if (px >= 0 && px < textureSize && py >= 0 && py < textureSize)
                    {
                        _surfaceGrid[px, py].PaintAccumulation += 0.1f;

                        Color blendedColor = Color.Lerp(_surfaceGrid[px, py].CurrentColor, liquidColor, 0.8f);
                        _surfaceGrid[px, py].CurrentColor = blendedColor;

                        _texture.SetPixel(px, py, blendedColor);
                        contentAltered = true;
                    }
                }
            }

            if (contentAltered)
            {
                _texture.Apply(false);
            }
        }
    }
}