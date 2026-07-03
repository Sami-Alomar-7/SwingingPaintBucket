using UnityEngine;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Data;

namespace SwingingPaintBucket.Features.ExternalForces.Services
{

    public class DragForce : IForceProvider
    {
        private readonly float _fallbackDragCoefficient;

        public DragForce(float dragCoefficient = 0.1f)
        {
            _fallbackDragCoefficient = dragCoefficient;
        }

        public Vector3 GetForce(in PendulumState state, in PendulumConfig config)
        {
            float c = config.DampingCoefficient > 0f ? config.DampingCoefficient : _fallbackDragCoefficient;

            Vector3 bucketVelocity;

            if (state.Velocity != Vector3.zero)
            {
                bucketVelocity = state.Velocity;
            }
            else
            {
                float tangentialSpeed = state.Omega * config.RopeLength;
                bucketVelocity = new Vector3(
                    tangentialSpeed * Mathf.Cos(state.Theta),
                    tangentialSpeed * Mathf.Sin(state.Theta),
                    0f
                );
            }

            return -c * bucketVelocity;
        }
    }
}