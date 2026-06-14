using UnityEngine;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Data;

namespace SwingingPaintBucket.Features.ExternalForces.Services
{
    /// <summary>
    /// Returns a drag force proportional to the negative bucket velocity: F = -k·v.
    /// </summary>
    public class DragForce : IForceProvider
    {
        private readonly float _dragCoefficient;

        public DragForce(float dragCoefficient = 0.1f) => _dragCoefficient = dragCoefficient;

        public Vector3 GetForce(in PendulumState state, in PendulumConfig config)
        {
            // Calculate tangential velocity from angular velocity: v = ω * r
            // The bucket moves in an arc, so velocity is perpendicular to the rope
            float tangentialSpeed = state.Omega * config.RopeLength;
            
            // Velocity direction is perpendicular to the angle (tangent to the arc)
            // At angle θ, the tangent direction is (cos(θ), 0, sin(θ)) for motion in X-Z plane
            Vector3 bucketVelocity = new Vector3(
                tangentialSpeed * Mathf.Cos(state.Theta),
                0f,
                tangentialSpeed * Mathf.Sin(state.Theta));

            return -_dragCoefficient * bucketVelocity;
        }
    }
}
