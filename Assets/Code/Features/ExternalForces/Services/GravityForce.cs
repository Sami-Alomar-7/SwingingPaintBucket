using UnityEngine;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Data;

namespace SwingingPaintBucket.Features.ExternalForces.Services
{
    /// <summary>
    /// Returns the gravitational force F = m·g in the downward direction.
    /// </summary>
    public class GravityForce : IForceProvider
    {
        public Vector3 GetForce(in PendulumState state, in PendulumConfig config)
        {
            return new Vector3(0f, -config.BaseMass * config.Gravity, 0f);
        }
    }
}
