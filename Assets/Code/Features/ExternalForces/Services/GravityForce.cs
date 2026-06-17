using UnityEngine;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;

namespace SwingingPaintBucket.Features.ExternalForces.Services
{

    public class GravityForce : IForceProvider
    {
        private readonly IMassProvider _massProvider;

        public GravityForce(IMassProvider massProvider)
        {
            _massProvider = massProvider;
        }

        public Vector3 GetForce(in PendulumState state, in PendulumConfig config)
        {
            float currentTotalMass = _massProvider != null ? _massProvider.GetTotalMass() : config.BaseMass;

            return new Vector3(0f, -currentTotalMass * config.Gravity, 0f);
        }
    }
}