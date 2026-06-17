using UnityEngine;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{
    public interface IPendulumView
    {
        void UpdateBucketPosition(Vector3 worldPosition);
        void UpdateRope(Vector3 pivotPos, Vector3 bucketPos);
    }
}
