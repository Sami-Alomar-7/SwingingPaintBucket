using UnityEngine;

namespace SwingingPaintBucket.Features.Pendulum.Data
{
    /// <summary>
    /// Runtime state of a pendulum system.
    /// Supports both angular (rigid) and Cartesian (elastic) physics models.
    /// </summary>
    public struct PendulumState
    {
        /// <summary>Angle from vertical downward direction (radians). Used for rigid physics.</summary>
        public float Theta;

        /// <summary>Angular velocity (rad/s). Used for rigid physics.</summary>
        public float Omega;

        /// <summary>World-space position of the bucket.</summary>
        public Vector3 BucketPosition;

        /// <summary>3D linear velocity vector of the bucket (m/s). Used for elastic physics.</summary>
        public Vector3 Velocity;

        /// <summary>Returns a state initialised to the resting position.</summary>
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
