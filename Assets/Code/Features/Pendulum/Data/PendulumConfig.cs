using UnityEngine;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.Pendulum.Data
{
    public class PendulumConfig
    {
        public float RopeLength = 3f;
        public float Gravity = 9.81f;
        public float DampingCoefficient = 0.1f;
        public Vector3 PivotPosition;
        public float BaseMass = 1f;
        public float InitialPaintMass = 0.5f;

        // --- الإضافة الفيجوال والفيزيائية الجديدة للثقب ---
        [Header("Bucket Aperture Settings")]
        public float ApertureDiameter = 0.01f; // قطر الثقب بالمتر (0.01 تعني 1 سم وهي القيمة الافتراضية المناسبة)

        public RopeType CurrentRopeType = RopeType.Rigid;
        public RopeConfig RopeConfig = new RopeConfig();

        public void SetField(int fieldId, string textValue)
        {
            if (!float.TryParse(textValue, out float value)) return;

            switch (fieldId)
            {
                case 0: RopeLength = Mathf.Max(value, 0.01f); break;
                case 1: Gravity = Mathf.Max(value, 0f); break;
                case 2: DampingCoefficient = Mathf.Max(value, 0f); break;
                case 3: BaseMass = Mathf.Max(value, 0.01f); break;
                case 4: InitialPaintMass = Mathf.Max(value, 0f); break;
                case 5: ApertureDiameter = Mathf.Max(value, 0.001f); break; // ربط الخيار رقم 5 بقطر ثقب الدلو
            }
        }

        public void UpdateField(PendulumConfig other)
        {
            if (other == null) return;
            RopeLength = other.RopeLength;
            Gravity = other.Gravity;
            DampingCoefficient = other.DampingCoefficient;
            BaseMass = other.BaseMass;
            InitialPaintMass = other.InitialPaintMass;
            ApertureDiameter = other.ApertureDiameter; // تحديث القطر ديناميكياً عند انتقال البيانات
            CurrentRopeType = other.CurrentRopeType;
            RopeConfig.RestLength = other.RopeConfig.RestLength;
            RopeConfig.RopeDamping = other.RopeConfig.RopeDamping;
            RopeConfig.SpringStiffness = other.RopeConfig.SpringStiffness;
        }
    }
}