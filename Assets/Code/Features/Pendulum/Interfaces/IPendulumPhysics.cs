using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{
    /// <summary>
    /// Contract for computing the angular acceleration of the pendulum.
    /// Supports a pluggable force vector pipeline for environmental dynamics.
    /// </summary>
    public interface IPendulumPhysics
    {
        /// <summary>
        /// Computes the angular acceleration (rad/s²) based on mass, configuration, and external forces.
        /// </summary>
        float ComputeAngularAcceleration(
            PendulumState state, 
            PendulumConfig config, 
            float totalMass, 
            IList<IForceProvider> forceProviders);
    }
}