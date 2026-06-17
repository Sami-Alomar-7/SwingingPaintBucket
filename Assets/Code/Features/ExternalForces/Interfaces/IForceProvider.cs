using UnityEngine;
using SwingingPaintBucket.Features.Pendulum.Data;

namespace SwingingPaintBucket.Features.ExternalForces.Interfaces
{

    public interface IForceProvider
    {
        Vector3 GetForce(in PendulumState state, in PendulumConfig config);
    }
}