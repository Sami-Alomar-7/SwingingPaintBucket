using UnityEngine;
using SwingingPaintBucket.Features.Surface.Interfaces;

namespace SwingingPaintBucket.Features.Surface.Services
{

    public class PaintApplicationService : IPaintSurfaceService
    {
        public void Paint(Texture2D texture, Vector2 uv, Color color, int brushRadius)
        {
            if (texture == null) return;

            // 1. Transform normalized UV coordinates into discrete pixel space coordinates.
            // Maps naturally to world space without coordinate inversion
            int centerPixelX = Mathf.FloorToInt(uv.x * texture.width);
            int centerPixelY = Mathf.FloorToInt(uv.y * texture.height);

            // 2. Establish safe, clamped iteration bounds to prevent out-of-bounds array crashes.
            int startX = Mathf.Max(0, centerPixelX - brushRadius);
            int endX = Mathf.Min(texture.width - 1, centerPixelX + brushRadius);
            int startY = Mathf.Max(0, centerPixelY - brushRadius);
            int endY = Mathf.Min(texture.height - 1, centerPixelY + brushRadius);

            bool contentAltered = false;
            float maxRadiusSquared = brushRadius * brushRadius;

            // 3. Radial spatial bounding pass loop
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    float dx = x - centerPixelX;
                    float dy = y - centerPixelY;

                    if (dx * dx + dy * dy <= maxRadiusSquared)
                    {
                        texture.SetPixel(x, y, color);
                        contentAltered = true;
                    }
                }
            }

            if (contentAltered)
            {
                texture.Apply();
            }
        }
    }
}