using UnityEngine;

namespace SwingingPaintBucket.Features.Surface.Interfaces
{
    /// <summary>
    /// Contract for applying paint markings onto a 2D canvas texture map.
    /// Fully decoupled from particle lifetime, emission parameters, and swing physics.
    /// </summary>
    public interface IPaintSurfaceService
    {
        /// <summary>
        /// Paints a radial splat stamp onto the provided texture canvas at the specified UV coordinates.
        /// </summary>
        /// <param name="texture">The target render texture to manipulate.</param>
        /// <param name="uv">Normalized texture coordinate space coordinates (0-1).</param>
        /// <param name="color">The color profile of the paint particle droplet.</param>
        /// <param name="brushRadius">The absolute pixel radius of the structural splat brush footprint.</param>
        void Paint(Texture2D texture, Vector2 uv, Color color, int brushRadius);
    }
}