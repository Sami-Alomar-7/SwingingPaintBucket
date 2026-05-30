using UnityEngine;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{
    /// <summary>
    /// Contract for UI-facing visuals: updating the bucket's world position and
    /// refreshing the rope rendering.
    /// </summary>
    public interface IPendulumView
    {
        /// <summary>
        /// Updates the bucket's transform to the given world position.
        /// </summary>
        void UpdateBucketPosition(Vector3 worldPosition);

        /// <summary>
        /// Updates the rope so it connects pivotPos → bucketPos.
        /// </summary>
        void UpdateRope(Vector3 pivotPos, Vector3 bucketPos);
    }
}
