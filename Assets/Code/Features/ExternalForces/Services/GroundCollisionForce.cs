using UnityEngine;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.ExternalForces.Services
{
    public class GroundCollisionForce : IForceProvider
    {
        private readonly float _groundLevel;
        private readonly float _groundStiffness; // صلابة الأرض (دفع نابضي صلب)
        private readonly float _groundDamping;   // امتصاص الصدمة (الارتداد)
        private readonly float _dynamicFriction; // معامل الاحتكاك لمنع الزحف السطحي

        public GroundCollisionForce(float groundLevel = 0.2f, float stiffness = 15000f, float damping = 250f, float friction = 0.5f)
        {
            _groundLevel = groundLevel;
            _groundStiffness = stiffness;
            _groundDamping = damping;
            _dynamicFriction = friction;
        }

        public Vector3 GetForce(in PendulumState state, in PendulumConfig config)
        {
            // التحقق مما إذا كان الدلو قد تداخل واخترق مستوى الأرض
            float penetration = _groundLevel - state.BucketPosition.y;

            if (penetration <= 0f) return Vector3.zero; // لا يوجد اصطدام حالياً

            // 1. حساب قوة رد الفعل العمودية الصلبة للأرض (قوة مستعيدة + قوة إخماد السرعة الشاقولية الهابطة)
            float springForceY = penetration * _groundStiffness;

            // نأخذ مركبة السرعة الشاقولية فقط للإخماد
            float dampingForceY = -state.Velocity.y * _groundDamping;

            float totalNormalForceY = Mathf.Max(0f, springForceY + dampingForceY);
            Vector3 normalForce = new Vector3(0f, totalNormalForceY, 0f);

            // 2. حساب قوة الاحتكاك السطحي (عكس اتجاه الحركة الأفقية X-Z ونسبية لقوة رد الفعل العمودي)
            Vector3 horizontalVelocity = new Vector3(state.Velocity.x, 0f, state.Velocity.z);
            Vector3 frictionForce = Vector3.zero;

            if (horizontalVelocity.magnitude > 0.001f)
            {
                // قوة الاحتكاك = معامل الاحتكاك * الناظمي * عكس اتجاه السرعة الأفقية
                frictionForce = -horizontalVelocity.normalized * (_dynamicFriction * totalNormalForceY);
            }

            // محصلة القوى المؤثرة الناتجة عن اصطدام الدلو بالأرض
            return normalForce + frictionForce;
        }
    }
}