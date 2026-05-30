using UnityEngine;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;
using System.Collections.Generic;

namespace SwingingPaintBucket.Features.Paint.Services
{
    /// <summary>
    /// Simulates paint particle physics using semi-implicit Euler integration.
    ///
    /// Each frame:
    ///   1. Gravity accelerates the particle downward.
    ///   2. Aerodynamic drag reduces horizontal velocity (particles lose forward momentum).
    ///   3. Position is updated by velocity.
    ///   4. Lifetime ticks down.
    ///   5. Particles that hit the surface (y ≤ surfaceY) or expire are flagged for removal.
    ///
    /// The returned list contains indices (descending) so the caller can safely
    /// remove them from the list without index shifting.
    /// </summary>
    public class ParticlePhysicsService : IParticlePhysicsService
    {
        /// <summary>
        /// Aerodynamic drag coefficient for paint particles.
        /// Higher values cause particles to lose horizontal momentum faster.
        /// Paint droplets experience high air resistance due to small size and surface tension.
        /// </summary>
        private const float ParticleDragCoefficient = 8.0f;

        /// <inheritdoc />
        public List<int> UpdateParticles(
            List<ParticleData> particles,
            float deltaTime,
            float gravity,
            float surfaceY)
        {
            var toRemove = new List<int>();

            for (int i = 0; i < particles.Count; i++)
            {
                ParticleData p = particles[i];

                // 1. Apply gravity (downward = negative Y)
                p.velocity.y -= gravity * deltaTime;

                // 2. Apply aerodynamic drag to horizontal velocity components
                // This causes particles to lose forward momentum and drop naturally
                p.velocity.x -= p.velocity.x * ParticleDragCoefficient * deltaTime;
                p.velocity.z -= p.velocity.z * ParticleDragCoefficient * deltaTime;

                // 3. Integrate position
                p.position += p.velocity * deltaTime;

                // 4. Tick lifetime
                p.lifeRemaining -= deltaTime;

                // 5. Check termination conditions
                if (p.lifeRemaining <= 0f || p.position.y <= surfaceY)
                {
                    // Clamp to surface so the paint mark appears exactly on the plane
                    if (p.position.y < surfaceY)
                        p.position.y = surfaceY;

                    toRemove.Add(i);
                }

                // Write updated struct values back to the collection instance
                particles[i] = p;
            }

            // Return in descending order so the caller can RemoveAt safely
            toRemove.Sort((a, b) => b.CompareTo(a));
            return toRemove;
        }
    }
}