using UnityEngine;

namespace SwingingPaintBucket.Features.Rope.Interfaces
{

    public interface IRope
    {
        void UpdateRope(Vector3 pivotPos, Vector3 bucketPos);
    }
}