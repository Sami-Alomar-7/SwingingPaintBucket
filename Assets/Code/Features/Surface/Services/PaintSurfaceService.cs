using UnityEngine;
using SwingingPaintBucket.Features.Surface.Interfaces;
using SwingingPaintBucket.Features.Surface.Components;

namespace SwingingPaintBucket.Features.Surface.Services
{
    public class PaintSurfaceService : IPaintSurfaceService
    {
        public void HandleParticleInteraction(PaintSurfaceSystem surfaceSystem, Vector3 worldPosition, Color color, int brushRadius)
        {
            if (surfaceSystem == null) return;

            if (surfaceSystem.WorldToGridCoords(worldPosition, out int gridX, out int gridY))
            {
                surfaceSystem.ApplyPhysicalPaint(gridX, gridY, color, brushRadius);
            }
        }

        public void Paint(Texture2D texture, Vector2 uv, Color color, int brushRadius)
        {
            var surface = Object.FindObjectOfType<PaintSurfaceSystem>();
            if (surface != null) surface.PaintAtUV(uv, color, brushRadius);
        }
    }
}