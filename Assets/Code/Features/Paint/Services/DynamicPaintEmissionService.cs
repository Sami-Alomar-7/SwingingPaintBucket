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
            // 1. حساب مساحة الفتحة بدقة: A = pi * r^2
            float radius = config.holeDiameter * 0.5f;
            float apertureArea = Mathf.PI * radius * radius;

            // 2. تقدير ارتفاع السائل (h) بشكل ديناميكي تقريبي بناءً على الجزيئات المتبقية في الزجاج الشفاف أو كتلة الطلاء
            // للحفاظ على البنية الحالية، سنفترض وجود علاقة طردية بين الارتفاع والمعاملات المتاحة، أو نضع قيمة افتراضية تتناقص
            float g = config.gravity;
            float fluidHeight = 0.3f; // يمكن ربطه لاحقاً بنسبة السائل المتبقي في BucketLiquidVolume

            // 3. تطبيق معادلة توريشيللي لسرعة الاندفاع: v = sqrt(2 * g * h)
            float effluxVelocity = Mathf.Sqrt(2f * g * fluidHeight);

            // 4. معدل التدفق الحجمي الكلي الناتج
            float flowRate = apertureArea * effluxVelocity;

            // 5. تحويل معدل التدفق إلى عدد جزيئات في الثانية (Spawn Rate)
            // نضرب بمعامل تحجيم (Scale Factor) ليتناسب مع الحد الأقصى للجزيئات في مشروعكِ (مثلاً 600 جزيء)
            float calculatedRate = flowRate * 120000f;

            // إضافة تأثير ديناميكي اختياري عند تأرجح الدلو بسرعة ليحاكي قوى الطرد المركزي
            if (config.useDynamicEmission)
            {
                calculatedRate += bucketVelocity.magnitude * config.velocityMultiplier;
            }

            // حصر النتيجة بين الحد الأدنى والأقصى المسموح به في الإعدادات لمنع انفجار الفيزيائي
            return Mathf.Clamp(calculatedRate, config.minSpawnRate, config.maxSpawnRate);
        }
        // تم الإبقاء عليها للمحافظة على التوافقية العامة للمشروع
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

        /// <summary>
        /// التنفيذ الفعلي لحقن جزيئات الطلاء الخارجة حركياً من أسفل الصندوق الزجاجي
        /// </summary>
        public void EmitPhysicalParticle(PaintEmissionConfig config, Vector3 worldPosition, Vector3 fluidVelocity, List<ParticleData> particles)
        {
            if (particles == null) return;

            // إضافة تأثير بسيط جداً يعبر عن الاحتكاك الجانبي للفتحة بناءً على الـ holeDiameter
            float radius = config.holeDiameter * 0.2f;
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 spreadOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);

            // السرعة الابتدائية للجزيء المتساقط هي تماماً سرعته اللحظية داخل السائل أثناء الاندفاع
            Vector3 finalVelocity = fluidVelocity;

            // في حال كانت السرعة العمودية متجهة للأعلى بسبب الارتداد، نجبرها للأسفل لتبدو كقطرة تخرج بقوة الجاذبية
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