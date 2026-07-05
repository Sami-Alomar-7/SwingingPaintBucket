using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;

namespace SwingingPaintBucket.Features.Paint.Services
{
    public class DynamicPaintEmissionService : IPaintEmissionService
    {
        public float CalculateEmissionRate(Vector3 bucketVelocity, PaintEmissionConfig config)
        {
            float radius = config.holeDiameter * 0.5f;
            float apertureArea = Mathf.PI * radius * radius;

            float g = config.gravity;
            float fluidHeight = 0.3f;
            float effluxVelocity = Mathf.Sqrt(2f * g * fluidHeight);
            float flowRate = apertureArea * effluxVelocity;

            float calculatedRate = flowRate * 120000f;

            if (config.useDynamicEmission)
            {
                calculatedRate += bucketVelocity.magnitude * config.velocityMultiplier;
            }

            return Mathf.Clamp(calculatedRate, config.minSpawnRate, config.maxSpawnRate);
        }

        public void EmitParticle(PaintEmissionConfig config, Vector3 spawnPosition, Vector3 initialVelocity, List<ParticleData> particles)
        {
            if (particles == null) return;

            // Offset is already calculated accurately in PaintEmitter, so we use spawnPosition directly.
            // Also, splashVelocity is removed to ensure realistic fluid flow directly from the hole without random horizontal scattering.
            
            ParticleData newParticle = new ParticleData(
                spawnPosition,
                initialVelocity,
                config.particleLife,
                config.particleColor,
                config.particleSize
            );

            particles.Add(newParticle);
        }

        public void EmitPhysicalParticle(PaintEmissionConfig config, Vector3 worldPosition, Vector3 fluidVelocity, List<ParticleData> particles)
        {
            if (particles == null) return;

            Vector3 finalVelocity = fluidVelocity;
            if (finalVelocity.y > 0f) finalVelocity.y = -finalVelocity.y * 0.2f;

            ParticleData newPhysicalParticle = new ParticleData(
                worldPosition,
                finalVelocity,
                config.particleLife,
                config.particleColor,
                config.particleSize
            );

            particles.Add(newPhysicalParticle);
        }
    }
}
