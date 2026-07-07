using UnityEngine;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;

namespace SwingingPaintBucket.Features.ExternalForces.Services
{
    public class DragForce : IForceProvider
    {
        private readonly float _fallbackDragCoefficient;
        private readonly IMassProvider _massProvider;

        // مشيد يستقبل مزود الكتلة لمنع التناسب العكسي الخاطئ
        public DragForce(IMassProvider massProvider, float dragCoefficient = 0.1f)
        {
            _massProvider = massProvider;
            _fallbackDragCoefficient = dragCoefficient;
        }

        // مشيد احتياطي فارغ لحماية التوافقية
        public DragForce()
        {
            _fallbackDragCoefficient = 0.1f;
        }

        public Vector3 GetForce(in PendulumState state, in PendulumConfig config)
        {
            float c = config.DampingCoefficient > 0f ? config.DampingCoefficient : _fallbackDragCoefficient;
            float currentMass = _massProvider != null ? _massProvider.GetTotalMass() : config.BaseMass;

            Vector3 bucketVelocity;

            if (state.Velocity != Vector3.zero)
            {
                bucketVelocity = state.Velocity;
            }
            else
            {
                float tangentialSpeed = state.Omega * config.RopeLength;
                bucketVelocity = new Vector3(
                    tangentialSpeed * Mathf.Cos(state.Theta),
                    tangentialSpeed * Mathf.Sin(state.Theta),
                    0f
                );
            }

            // حساب القوة الفيزيائية متناسبة طردياً مع الكتلة الكلية الحالية للسطل
            return -c * currentMass * bucketVelocity;
        }
    }
}