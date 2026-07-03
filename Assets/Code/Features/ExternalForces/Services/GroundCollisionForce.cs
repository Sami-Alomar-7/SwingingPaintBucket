using UnityEngine;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.ExternalForces.Services
{
    public class GroundCollisionForce : IForceProvider
    {
        private readonly float _groundLevel;
        private readonly float _groundStiffness;
        private readonly float _groundDamping;
        private readonly float _dynamicFriction;

        public GroundCollisionForce(float groundLevel = 0.2f, float stiffness = 15000f, float damping = 250f, float friction = 0.5f)
        {
            _groundLevel = groundLevel;
            _groundStiffness = stiffness;
            _groundDamping = damping;
            _dynamicFriction = friction;
        }

        public Vector3 GetForce(in PendulumState state, in PendulumConfig config)
        {
            float penetration = _groundLevel - state.BucketPosition.y;

            if (penetration <= 0f) return Vector3.zero;

            float springForceY = penetration * _groundStiffness;

            float dampingForceY = -state.Velocity.y * _groundDamping;

            float totalNormalForceY = Mathf.Max(0f, springForceY + dampingForceY);
            Vector3 normalForce = new Vector3(0f, totalNormalForceY, 0f);

            Vector3 horizontalVelocity = new Vector3(state.Velocity.x, 0f, state.Velocity.z);
            Vector3 frictionForce = Vector3.zero;

            if (horizontalVelocity.magnitude > 0.001f)
            {
                frictionForce = -horizontalVelocity.normalized * (_dynamicFriction * totalNormalForceY);
            }

            return normalForce + frictionForce;
        }
    }
}