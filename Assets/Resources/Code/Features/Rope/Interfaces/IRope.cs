using UnityEngine;

namespace SwingingPaintBucket.Features.Rope.Interfaces
{
    /// <summary>
    /// Rope abstraction. Decouples the pendulum controller from any particular
    /// rope implementation (fixed-length, elastic, etc.).
    /// </summary>
    public interface IRope
    {
        /// <summary>
        /// Updates the rope so the segment runs from pivotPos to bucketPos.
        /// Called every frame from the PendulumController update loop.
        /// </summary>
        void UpdateRope(Vector3 pivotPos, Vector3 bucketPos);
    }
}
