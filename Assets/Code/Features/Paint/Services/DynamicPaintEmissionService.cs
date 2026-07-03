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

        public void EmitParticle(PaintEmissionConfig config, Vector3 spawnPosition, Vector3 bucketVelocity, List<ParticleData> particles)
        {
            if (particles == null) return;

            float radius = config.holeDiameter * 0.5f;
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 offsetPosition = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            float randomX = Random.Range(-config.splashRange.x, config.splashRange.x);
            float randomY = Random.Range(-config.splashRange.y, 0f);
            float randomZ = Random.Range(-config.splashRange.z, config.splashRange.z);
            Vector3 splashVelocity = new Vector3(randomX, randomY, randomZ);

            Vector3 initialVelocity = bucketVelocity + splashVelocity;

            ParticleData newParticle = new ParticleData(
                offsetPosition,
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

            float radius = config.holeDiameter * 0.2f;
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 spreadOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);

            Vector3 finalVelocity = fluidVelocity;

            if (finalVelocity.y > 0f) finalVelocity.y = -finalVelocity.y * 0.2f;

            ParticleData newPhysicalParticle = new ParticleData(
                worldPosition + spreadOffset,
                finalVelocity,
                config.particleLife,
                config.particleColor,
                config.particleSize
            );

            particles.Add(newPhysicalParticle);
        }
    }
}