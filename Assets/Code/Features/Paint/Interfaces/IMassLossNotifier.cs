using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Interfaces
{
    /// <summary>
    /// Contract for any component or service that emits mass when a particle leaves
    /// the bucket. The Paint subscription passes the delta to the Pendulum section
    /// via this call so that loss of paint is properly reflected in the system inertia.
    /// </summary>
    public interface IMassLossNotifier
    {
        /// <summary>
        /// Called on the main thread each time a particle is emitted.
        /// </summary>
        /// <param name="particleMass">Mass of the particle that just left the bucket (kg).</param>
        void NotifyParticleEmitted(float particleMass);
    }
}
