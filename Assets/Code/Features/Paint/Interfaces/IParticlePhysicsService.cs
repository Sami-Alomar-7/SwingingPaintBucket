using UnityEngine;
using SwingingPaintBucket.Features.Paint.Data;
using System.Collections.Generic;

namespace SwingingPaintBucket.Features.Paint.Interfaces
{
    /// <summary>
    /// Service for simulating paint particle physics
    /// </summary>
    public interface IParticlePhysicsService
    {
        /// <summary>
        /// Updates all particles based on physics
        /// </summary>
        /// <param name="particles">List of particles to update</param>
        /// <param name="deltaTime">Time elapsed since last update</param>
        /// <param name="gravity">Gravity acceleration</param>
        /// <param name="surfaceY">Y position of the surface (for collision detection)</param>
        /// <returns>List of indices of particles that hit the surface and should be removed</returns>
        System.Collections.Generic.List<int> UpdateParticles(
            System.Collections.Generic.List<ParticleData> particles, 
            float deltaTime, 
            float gravity,
            float surfaceY);
    }
}