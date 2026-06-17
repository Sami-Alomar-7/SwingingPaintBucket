using UnityEngine;

namespace SwingingPaintBucket.Features.Pendulum.Data
{

    public struct PendulumState
    {
        public float Theta;

        public float Omega;

        public Vector3 BucketPosition;

        public Vector3 Velocity;

        public static PendulumState Resting(float length, Vector3 pivotPosition)
            => new PendulumState
            {
                Theta = 0f,
                Omega = 0f,
                BucketPosition = pivotPosition + new Vector3(0f, -length, 0f),
                Velocity = Vector3.zero
            };
    }
}