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
            if (!config.useDynamicEmission) return config.baseSpawnRate;

            float speed = bucketVelocity.magnitude;
            float dynamicRate = config.minSpawnRate + (speed * config.velocityMultiplier);
            return Mathf.Clamp(dynamicRate, config.minSpawnRate, config.maxSpawnRate);
        }

        public void EmitParticle(PaintEmissionConfig config, Vector3 spawnPosition, Vector3 bucketVelocity, List<ParticleData> particles)
        {
            if (particles == null) return;

            float randomX = Random.Range(-config.splashRange.x, config.splashRange.x);
            float randomY = Random.Range(-config.splashRange.y, 0f);
            float randomZ = Random.Range(-config.splashRange.z, config.splashRange.z);
            Vector3 splashVelocity = new Vector3(randomX, randomY, randomZ);

            Vector3 initialVelocity = bucketVelocity + splashVelocity;

            ParticleData newParticle = new ParticleData(
                spawnPosition,
                initialVelocity,
                config.particleLife,
                config.particleColor,
                config.particleSize
            );

            particles.Add(newParticle);
        }
    }
}