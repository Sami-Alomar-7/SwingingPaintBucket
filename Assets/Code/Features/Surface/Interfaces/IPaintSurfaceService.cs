
using UnityEngine;
using SwingingPaintBucket.Features.Surface.Components;

namespace SwingingPaintBucket.Features.Surface.Interfaces
{
    public interface IPaintSurfaceService
    {
        void HandleParticleInteraction(PaintSurfaceSystem surfaceSystem, Vector3 worldPosition, Color color, int brushRadius);
        void Paint(Texture2D texture, Vector2 uv, Color color, int brushRadius);
    }
}