using UnityEngine;

namespace SwingingPaintBucket.Features.ExternalForces.Interfaces
{
    /// <summary>
    /// Contract for a force that can be evaluated given the current pendulum state
    /// and configuration.
    /// </summary>
    public interface IForceProvider
    {
        /// <summary>
        /// Returns the force vector (in world-space Newtons) exerted by this provider.
        /// </summary>
        /// <param name="state">Current pendulum state.</param>
        /// <param name="config">Current pendulum configuration.</param>
        Vector3 GetForce(in SwingingPaintBucket.Features.Pendulum.Data.PendulumState state,
                         in SwingingPaintBucket.Features.Pendulum.Data.PendulumConfig  config);
    }
}
