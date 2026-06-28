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

            // تحسين: توزيع جزيئات الطلاء بشكل دائري عشوائي بناءً على قطر فتحة الدلو holeDiameter
            float radius = config.holeDiameter * 0.5f;
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            // إسقاط التوزيع العشوائي على المستوي الأفقي (X, Z) للخروج الطبيعي لأسفل
            Vector3 offsetPosition = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            float randomX = Random.Range(-config.splashRange.x, config.splashRange.x);
            float randomY = Random.Range(-config.splashRange.y, 0f);
            float randomZ = Random.Range(-config.splashRange.z, config.splashRange.z);
            Vector3 splashVelocity = new Vector3(randomX, randomY, randomZ);

            Vector3 initialVelocity = bucketVelocity + splashVelocity;

            ParticleData newParticle = new ParticleData(
                offsetPosition, // استخدام الموضع الموزع الجديد
                initialVelocity,
                config.particleLife,
                config.particleColor,
                config.particleSize
            );

            particles.Add(newParticle);
        }
    }
}