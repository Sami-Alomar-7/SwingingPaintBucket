using UnityEngine;

namespace SwingingPaintBucket.Features.Surface.Components
{
    /// <summary>
    /// Manages a paintable texture on the ground plane.
    ///
    /// Key fix: creates a NEW material instance (not sharedMaterial) so we never
    /// corrupt the project asset. The texture is applied to that instance only.
    /// </summary>
    public class PaintSurfaceSystem : MonoBehaviour
    {
        [Header("Surface Settings")]
        [Tooltip("Resolution of the paint texture (higher = more detail, more memory)")]
        public int textureSize = 512;

        [Tooltip("Base color of the surface before any paint is applied")]
        public Color baseColor = Color.white;

        // Internal state
        private Texture2D _texture;
        private Renderer  _rend;
        private Material  _instanceMaterial;   // owned by this component, not shared

        /// <summary>Returns the live texture being painted on.</summary>
        public Texture2D GetTexture() => _texture;

        // ─── Unity callbacks ─────────────────────────────────────────────────────

        private void Awake()
        {
            InitializeSurface();
        }

        // ─── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// (Re)creates the texture and resets it to the base color.
        /// Call this to clear all paint marks.
        /// </summary>
        public void InitializeSurface()
        {
            _rend = GetComponent<Renderer>();
            if (_rend == null)
                _rend = GetComponentInChildren<Renderer>();

            // Create a fresh texture
            _texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            _texture.wrapMode   = TextureWrapMode.Clamp;
            _texture.filterMode = FilterMode.Bilinear;
            FillTexture(baseColor);

            // Create an INSTANCE material so we never touch the shared project asset
            if (_rend != null)
            {
                // Instantiate from the current shared material (preserves shader/settings)
                _instanceMaterial = new Material(_rend.sharedMaterial);
                _rend.material    = _instanceMaterial;   // assign instance, not shared

                // Apply our texture to the instance
                ApplyTexture();
            }
        }

        /// <summary>
        /// Paints a splat at the given UV coordinate with the given color.
        /// </summary>
        /// <param name="uv">UV in [0,1]×[0,1]</param>
        /// <param name="color">Paint color</param>
        public void Paint(Vector2 uv, Color color)
        {
            if (_texture == null) return;

            int cx = Mathf.Clamp((int)(uv.x * textureSize), 0, textureSize - 1);
            int cy = Mathf.Clamp((int)(uv.y * textureSize), 0, textureSize - 1);

            DrawCircle(cx, cy, radius: 8, color);
            _texture.Apply();
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private void FillTexture(Color fill)
        {
            Color[] pixels = new Color[textureSize * textureSize];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = fill;
            _texture.SetPixels(pixels);
            _texture.Apply();
        }

        private void ApplyTexture()
        {
            if (_instanceMaterial == null || _texture == null) return;

            // URP uses _BaseMap; Built-in uses _MainTex — set both to be safe
            if (_instanceMaterial.HasProperty("_BaseMap"))
                _instanceMaterial.SetTexture("_BaseMap", _texture);
            if (_instanceMaterial.HasProperty("_MainTex"))
                _instanceMaterial.SetTexture("_MainTex", _texture);
        }

        private void DrawCircle(int cx, int cy, int radius, Color color)
        {
            int r2 = radius * radius;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy > r2) continue;

                    int px = cx + dx;
                    int py = cy + dy;
                    if (px < 0 || px >= textureSize || py < 0 || py >= textureSize) continue;

                    // Alpha-blend new color over existing pixel for a softer look
                    Color existing = _texture.GetPixel(px, py);
                    Color blended  = Color.Lerp(existing, color, 0.85f);
                    _texture.SetPixel(px, py, blended);
                }
            }
        }
    }
}
