using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Data
{
    /// <summary>
    /// Data representing a paint particle (without Unity GameObject dependency)
    /// </summary>
    public class ParticleData
    {
        /// <summary>
        /// Current position of the particle
        /// </summary>
        public Vector3 position;
        
        /// <summary>
        /// Current velocity of the particle
        /// </summary>
        public Vector3 velocity;
        
        /// <summary>
        /// Remaining lifetime of the particle in seconds
        /// </summary>
        public float lifeRemaining;
        
        /// <summary>
        /// Color of the particle
        /// </summary>
        public Color color;
        
        /// <summary>
        /// Size of the particle
        /// </summary>
        public float size;
        
        /// <summary>
        /// Creates a new particle data
        /// </summary>
        /// <param name="position">Initial position</param>
        /// <param name="velocity">Initial velocity</param>
        /// <param name="lifeRemaining">Initial lifetime</param>
        /// <param name="color">Particle color</param>
        /// <param name="size">Particle size</param>
        public ParticleData(Vector3 position, Vector3 velocity, float lifeRemaining, Color color, float size)
        {
            this.position = position;
            this.velocity = velocity;
            this.lifeRemaining = lifeRemaining;
            this.color = color;
            this.size = size;
        }
    }
}