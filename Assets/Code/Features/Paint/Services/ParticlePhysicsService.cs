using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Surface.Data;

namespace SwingingPaintBucket.Features.Paint.Services
{
    public class ParticlePhysicsService : IParticlePhysicsService
    {
        private const float PI = Mathf.PI;

        public List<int> UpdateParticles(List<ParticleData> particles, float deltaTime, float surfaceY, PaintEmissionConfig config)
        {
            List<int> toRemoveIndices = new();
            if (particles == null || particles.Count == 0 || config == null) return toRemoveIndices;

            // جلب نظام السطح الموجود في المشهد ديناميكياً لمعرفة المادة الحالية
            PaintSurfaceSystem surfaceSystem = Object.FindObjectOfType<PaintSurfaceSystem>();
            SurfaceMaterialType material = surfaceSystem != null ? surfaceSystem.materialType : SurfaceMaterialType.Wood;

            // تحديد قيم الاحتكاك (Friction) والامتصاص (Absorption) بناءً على نوع السطح
            float friction = 0.2f;
            float absorption = 0.0f;

            switch (material)
            {
                case SurfaceMaterialType.Wood:
                    friction = 0.8f; absorption = 0.2f; break; // احتكاك قوي يثبت الكرات مكانها
                case SurfaceMaterialType.Metal:
                    friction = 0.05f; absorption = 0.0f; break; // انزلاق سلس للجزيئات
                case SurfaceMaterialType.Paper:
                    friction = 0.4f; absorption = 0.9f; break; // امتصاص هائل يجعل الجزيء ينكمش ويختفي بسرعة
                case SurfaceMaterialType.Glass:
                    friction = 0.01f; absorption = 0.0f; break;
            }

            float h = config.smoothingRadius;
            float h2 = h * h;
            float mass = config.particleMass;
            float restDensity = config.restDensity;
            float k = config.pressureStiffness;
            float mu = config.viscosity;
            float sigma = config.surfaceTension;
            float gravity = config.gravity;

            int count = particles.Count;

            float poly6Constant = 315f / (64f * PI * Mathf.Pow(h, 9));
            float spikyGradientConstant = -45f / (PI * Mathf.Pow(h, 6));
            float viscLaplacianConstant = 45f / (PI * Mathf.Pow(h, 6));

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

                if (pi.density < restDensity * 0.1f) pi.density = restDensity * 0.1f;
                pi.pressure = k * (pi.density - restDensity);
                if (pi.pressure < 0f) pi.pressure = 0f;
                pi.inverseDensity = 1f / pi.density;
            }

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

                    if (r2 < h2 && r2 > 0.000001f)
                    {
                        float r = Mathf.Sqrt(r2);
                        Vector3 dir = diff / r;
                        float hMinusR = h - r;

                        float gradW = spikyGradientConstant * hMinusR * hMinusR;
                        forcePressure += -mass * ((pi.pressure + pj.pressure) / (2f * pj.density)) * gradW * dir;

                        float lapW = viscLaplacianConstant * hMinusR;
                        forceViscosity += mu * mass * (pj.velocity - pi.velocity) / pj.density * lapW;

                        float poly6Term = h2 - r2;
                        colorFieldGradient += (mass / pj.density) * poly6Constant * 3f * poly6Term * poly6Term * (-2f) * diff;
                        colorFieldLaplacian += (mass / pj.density) * poly6Constant * 6f * poly6Term * (r2 - 3f * poly6Term);
                    }
                }

                Vector3 forceSurfaceTension = Vector3.zero;
                float normalMagnitude = colorFieldGradient.magnitude;
                if (normalMagnitude > 0.1f)
                {
                    forceSurfaceTension = -sigma * colorFieldLaplacian * (colorFieldGradient / normalMagnitude);
                }

                pi.forcePhysics = forcePressure + forceViscosity + forceSurfaceTension;
            }

            for (int i = 0; i < count; i++)
            {
                ParticleData p = particles[i];
                Vector3 acceleration = (p.forcePhysics * p.inverseDensity) + new Vector3(0f, -gravity, 0f);

                if (acceleration.magnitude > 200f)
                    acceleration = acceleration.normalized * 200f;

                p.velocity += acceleration * deltaTime;
                p.position += p.velocity * deltaTime;
                p.lifeRemaining -= deltaTime;

                if (p.isGrounded && absorption > 0f)
                {
                    p.size = Mathf.MoveTowards(p.size, 0f, absorption * deltaTime * 0.04f);
                    if (p.size <= 0.01f)
                    {
                        if (!toRemoveIndices.Contains(i)) toRemoveIndices.Add(i);
                        continue;
                    }
                }

                if (p.position.y <= surfaceY + 0.02f)
                {
                    p.position.y = surfaceY + 0.01f;
                    p.isGrounded = true;

                    p.velocity.x *= (1f - friction);
                    p.velocity.z *= (1f - friction);
                    p.velocity.y = 0f;
                }

                if (p.lifeRemaining <= 0f && !p.isGrounded)
                {
                    if (!toRemoveIndices.Contains(i))
                        toRemoveIndices.Add(i);
                }
            }

            return toRemoveIndices;
        }
    }
}