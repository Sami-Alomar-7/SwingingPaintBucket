using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Data
{
    public class ParticleData
    {
        public Vector3 position;
        public Vector3 velocity;
        public float lifeRemaining;
        public Color color;
        public float size;

        // خصائص محرك SPH
        public float density;
        public float inverseDensity;
        public float pressure;
        public Vector3 forcePhysics;

        public Vector3 colorFieldGradient;
        public float colorFieldLaplacian;

        public ParticleData(Vector3 position, Vector3 velocity, float lifeRemaining, Color color, float size)
        {
            this.position = position;
            this.velocity = velocity;
            this.lifeRemaining = lifeRemaining;
            this.color = color;
            this.size = size;

            this.density = 1000f;
            this.inverseDensity = 0.001f;
            this.pressure = 0f;
            this.forcePhysics = Vector3.zero;
            this.colorFieldGradient = Vector3.zero;
            this.colorFieldLaplacian = 0f;
        }
    }
}