using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;

namespace SwingingPaintBucket.Features.Paint.Services
{
    public class ParticlePhysicsService : IParticlePhysicsService
    {
        private const float PI = Mathf.PI;

        public List<int> UpdateParticles(List<ParticleData> particles, float deltaTime, float surfaceY, PaintEmissionConfig config)
        {
            List<int> toRemoveIndices = new List<int>();
            if (particles == null || particles.Count == 0 || config == null) return toRemoveIndices;

            float h = config.smoothingRadius;
            float h2 = h * h;
            float mass = config.particleMass;
            float restDensity = config.restDensity;
            float k = config.pressureStiffness;
            float mu = config.viscosity;
            float sigma = config.surfaceTension;
            float gravity = config.gravity;

            int count = particles.Count;

            // حساب الثوابت خارج الحلقات تماماً
            float poly6Constant = 315f / (64f * PI * Mathf.Pow(h, 9));
            float spikyGradientConstant = -45f / (PI * Mathf.Pow(h, 6));
            float viscLaplacianConstant = 45f / (PI * Mathf.Pow(h, 6));

            float poly6GradConst = -945f / (32f * PI * Mathf.Pow(h, 9));
            float poly6LapConst = -945f / (32f * PI * Mathf.Pow(h, 9));

            // 1. حساب الكثافة والضغط + حساب المقلوب (Inverse)
            for (int i = 0; i < count; i++)
            {
                ParticleData pi = particles[i];
                pi.density = 0f;

                for (int j = 0; j < count; j++)
                {
                    Vector3 diff = pi.position - particles[j].position;
                    float r2 = diff.sqrMagnitude;

                    if (r2 < h2)
                    {
                        float term = h2 - r2;
                        pi.density += mass * poly6Constant * term * term * term;
                    }
                }

                if (pi.density < restDensity)
                    pi.density = restDensity;

                pi.pressure = k * (pi.density - restDensity);

                // الحل السحري للأداء الفيزيائي: احسب القسمة مرة واحدة هنا فقط!
                pi.inverseDensity = 1f / pi.density;
            }

            // 2. حساب القوى المشتركة المتماثلة (بدون أي عمليات قسمة داخل الحلقة!)
            for (int i = 0; i < count; i++)
            {
                ParticleData pi = particles[i];
                Vector3 forcePressure = Vector3.zero;
                Vector3 forceViscosity = Vector3.zero;
                Vector3 colorFieldGradient = Vector3.zero;
                float colorFieldLaplacian = 0f;

                for (int j = 0; j < count; j++)
                {
                    if (i == j) continue;

                    ParticleData pj = particles[j];
                    Vector3 diff = pi.position - pj.position;
                    float r2 = diff.sqrMagnitude;

                    if (r2 < h2 && r2 > 0.00001f)
                    {
                        float r = Mathf.Sqrt(r2);
                        Vector3 dir = diff / r;
                        float hMinusR = h - r;

                        // استبدال القسمة بالضرب في مقلوب الكثافة للمادة الجارة (pj.inverseDensity)
                        float gradW = spikyGradientConstant * hMinusR * hMinusR;
                        forcePressure += -mass * (pi.pressure + pj.pressure) * 0.5f * pj.inverseDensity * gradW * dir;

                        float lapW = viscLaplacianConstant * hMinusR;
                        forceViscosity += mu * mass * (pj.velocity - pi.velocity) * pj.inverseDensity * lapW;

                        float h2MinusR2 = h2 - r2;
                        float poly6GradTerm = poly6GradConst * h2MinusR2 * h2MinusR2;
                        colorFieldGradient += mass * pj.inverseDensity * poly6GradTerm * diff;

                        float poly6LapTerm = poly6LapConst * h2MinusR2 * (3f * h2 - 7f * r2);
                        colorFieldLaplacian += mass * pj.inverseDensity * poly6LapTerm;
                    }
                }

                Vector3 forceSurfaceTension = Vector3.zero;
                float normalMagnitude = colorFieldGradient.magnitude;
                if (normalMagnitude > 0.1f)
                {
                    forceSurfaceTension = -sigma * colorFieldLaplacian * colorFieldGradient / normalMagnitude;
                }

                pi.forcePhysics = forcePressure + forceViscosity + forceSurfaceTension;
            }

            // 3. التكامل الحركي وفحص الحدود الأرضية للرسم
            for (int i = 0; i < count; i++)
            {
                ParticleData p = particles[i];
                Vector3 acceleration = (p.forcePhysics * p.inverseDensity) + new Vector3(0f, -gravity, 0f);

                if (acceleration.magnitude > 150f)
                    acceleration = acceleration.normalized * 150f;

                p.velocity += acceleration * deltaTime;
                p.position += p.velocity * deltaTime;
                p.lifeRemaining -= deltaTime;

                // فحص دقيق للحدود
                if (p.position.y <= surfaceY)
                {
                    p.position.y = surfaceY;
                    p.velocity = Vector3.zero;

                    if (!toRemoveIndices.Contains(i))
                        toRemoveIndices.Add(i);
                }
                else if (p.lifeRemaining <= 0f)
                {
                    if (!toRemoveIndices.Contains(i))
                        toRemoveIndices.Add(i);
                }
            }

            return toRemoveIndices;
        }
    }
}