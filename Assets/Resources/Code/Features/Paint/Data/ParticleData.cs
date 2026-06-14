using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Data
{
    public class ParticleData
    {
        // الخصائص الحركية الأساسية
        public Vector3 position;
        public Vector3 velocity;
        public float lifeRemaining;
        public Color color;
        public float size;

        // خصائص محرك SPH الفيزيائي
        public float density;
        public float pressure;
        public Vector3 forcePhysics;

        // خصائص التوتر السطحي (Müller 2003)
        public Vector3 colorFieldGradient;
        public float colorFieldLaplacian;

        public ParticleData(Vector3 position, Vector3 velocity, float lifeRemaining, Color color, float size)
        {
            this.position = position;
            this.velocity = velocity;
            this.lifeRemaining = lifeRemaining;
            this.color = color;
            this.size = size;

            this.density = 100f;
            this.pressure = 0f;
            this.forcePhysics = Vector3.zero;
            this.colorFieldGradient = Vector3.zero;
            this.colorFieldLaplacian = 0f;
        }
    }
}