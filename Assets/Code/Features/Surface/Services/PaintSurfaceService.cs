using UnityEngine;
using SwingingPaintBucket.Features.Surface.Interfaces;

namespace SwingingPaintBucket.Features.Surface.Services
{
    /// <summary>
    /// A pure, stateless utility service handling texture canvas manipulation.
    /// Follows the Single Responsibility Principle: strictly alters raw pixel arrays on localized target coordinates.
    /// </summary>
    public class PaintSurfaceService : IPaintSurfaceService
    {
        /// <inheritdoc />
        public void Paint(Texture2D texture, Vector2 uv, Color color, int brushRadius)
        {
            if (texture == null) return;

            // 1. Transform normalized UV coordinates into exact, discrete pixel addresses
            int centerPixelX = Mathf.FloorToInt(uv.x * texture.width);
            int centerPixelY = Mathf.FloorToInt(uv.y * texture.height);

            // 2. Compute explicit bounding box fields to prevent out-of-bounds array allocation crashes
            int startX = Mathf.Max(0, centerPixelX - brushRadius);
            int endX = Mathf.Min(texture.width - 1, centerPixelX + brushRadius);
            int startY = Mathf.Max(0, centerPixelY - brushRadius);
            int endY = Mathf.Min(texture.height - 1, centerPixelY + brushRadius);

            bool contentAltered = false;
            float maxRadiusSquared = brushRadius * brushRadius;

            // 3. Vector-bounded radial pixel fill pass
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    float distanceSquared = (x - centerPixelX) * (x - centerPixelX) + 
                                            (y - centerPixelY) * (y - centerPixelY);

                    // Check if current iteration index resides safely inside the brush circumference
                    if (distanceSquared <= maxRadiusSquared)
                    {
                        texture.SetPixel(x, y, color);
                        contentAltered = true;
                    }
                }
            }

            // 4. Force direct memory transfer allocation from CPU buffer to GPU texture memory VRAM
            if (contentAltered)
            {
                texture.Apply();
            }
        }
    }
}