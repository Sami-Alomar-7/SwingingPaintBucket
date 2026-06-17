using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{

    public interface IPendulumPhysics
    {
        float ComputeAngularAcceleration(
            PendulumState state,
            PendulumConfig config,
            float totalMass,
            IList<IForceProvider> forceProviders);
    }
}