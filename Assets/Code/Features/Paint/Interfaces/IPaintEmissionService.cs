using UnityEngine;
using SwingingPaintBucket.Features.Paint.Data;
using System.Collections.Generic;

namespace SwingingPaintBucket.Features.Paint.Interfaces
{
    /// <summary>
    /// Service for handling paint emission based on bucket velocity and configuration
    /// </summary>
    public interface IPaintEmissionService
    {
        /// <summary>
        /// Calculates the emission rate (particles per second) based on bucket velocity and configuration
        /// </summary>
        /// <param name="bucketVelocity">Current velocity of the bucket</param>
        /// <param name="config">Paint emission configuration</param>
        /// <returns>Emission rate in particles per second</returns>
        float CalculateEmissionRate(Vector3 bucketVelocity, PaintEmissionConfig config);

        /// <summary>
        /// Emits a new paint particle based on the current state
        /// </summary>
        /// <param name="config">Paint emission configuration</param>
        /// <param name="spawnPosition">Position to spawn the particle</param>
        /// <param name="bucketVelocity">Current velocity of the bucket (to inherit motion)</param>
        /// <param name="particles">List to add the new particle data to</param>
        void EmitParticle(PaintEmissionConfig config, Vector3 spawnPosition, Vector3 bucketVelocity, System.Collections.Generic.List<ParticleData> particles);
    }
}