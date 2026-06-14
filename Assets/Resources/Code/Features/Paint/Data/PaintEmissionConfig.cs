using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Data
{
    [System.Serializable]
    public class PaintEmissionConfig
    {
        [Header("Standard Emission Settings")]
        public float baseSpawnRate = 150f; // زيادة العدد الافتراضي لجعله يتدفق كالمائع
        public bool useDynamicEmission = true;
        public float minSpawnRate = 50f;
        public float maxSpawnRate = 350f;
        public float velocityMultiplier = 12f;
        public Color particleColor = Color.red;
        public float particleSize = 0.08f;
        public float particleLife = 6f;
        public Vector3 splashRange = new Vector3(0.05f, 0.2f, 0.05f); // تقليل التشتت لجعله يسقط كخيط سائل متصل
        public float particleMass = 0.02f;
        public float gravity = 9.81f;

        // الحقل المطلوب لحل مشكلة السطر 279 في ملف الـ SceneBuilder
        [Tooltip("Hole diameter used by SceneBuilder initialization")]
        public float holeDiameter = 0.05f;

        [Header("SPH Fluid Physics Settings (Müller 2003)")]
        public float smoothingRadius = 0.25f; // رفع النطاق لتشعر الجسيمات ببعضها أثناء السقوط
        public float restDensity = 1000f;     // كثافة الطلاء الواقعية
        public float pressureStiffness = 400f;
        public float viscosity = 4.5f;        // رفع اللزوجة ليظهر بشكل متماسك مثل الطلاء بدلاً من التناثر كالماء
        public float surfaceTension = 1.2f;
    }
}