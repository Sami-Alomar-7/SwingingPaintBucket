using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;

namespace SwingingPaintBucket.Features.Pendulum.Services
{
    public class PendulumPhysicsService : IPendulumPhysics
    {
        public float ComputeAngularAcceleration(
            PendulumState state,
            PendulumConfig config,
            float totalMass,
            IList<IForceProvider> forceProviders)
        {
            if (totalMass <= 0.001f) totalMass = 0.001f;
            float length = config.RopeLength;

            Vector3 tangentialDirection = new Vector3(Mathf.Cos(state.Theta), Mathf.Sin(state.Theta), 0f).normalized;
            Vector3 netForceWorld = Vector3.zero;

            // الدوران على كل مزودي القوى لجمع المتجهات الشاملة بدقة
            if (forceProviders != null && forceProviders.Count > 0)
            {
                for (int i = 0; i < forceProviders.Count; i++)
                {
                    if (forceProviders[i] != null)
                    {
                        netForceWorld += forceProviders[i].GetForce(state, config);
                    }
                }
            }

            // حساب مسقط القوة المماسية المؤثرة على الذراع
            float netTangentialForce = Vector3.Dot(netForceWorld, tangentialDirection);

            // معادلة نيوتن الزاوية التسارعية (Alpha = Torque / Inertia) حيث الـ Inertia = m * L^2
            return netTangentialForce / (totalMass * length);
        }
    }
}