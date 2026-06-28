using UnityEngine;

namespace SwingingPaintBucket.Features.Rope.Data
{
    public static class RopeTypePresets
    {
        public static float GetSpringStiffness(RopeType ropeType)
        {
            return ropeType switch
            {
                RopeType.Rigid => 8000f,  // صلب جداً لمنع التمدد
                RopeType.Nylon => 400f,   // خفضناها من 800 ليصبح مطاطياً بشكل واضح وممتع
                RopeType.Bungee => 120f,  // قيمة مثالية للمطاطية العالية
                _ => 5000f
            };
        }

        public static float GetRopeDamping(RopeType ropeType)
        {
            return ropeType switch
            {
                RopeType.Rigid => 120f,
                RopeType.Nylon => 15f,   // تقليل الخماد لكي يستمر بالاهتزاز والمط لفترة أطول قبل الاستقرار
                RopeType.Bungee => 8f,    // رفعناها قليلاً من 5 إلى 8 لمنع الحبل من الانهيار والاستطالة للانهاية
                _ => 100f
            };
        }

        public static void ApplyPreset(RopeConfig config, RopeType ropeType)
        {
            if (config == null) return;

            config.SpringStiffness = GetSpringStiffness(ropeType);
            config.RopeDamping = GetRopeDamping(ropeType);

            // إعدادات بصرية ديناميكية تختلف بحسب نوع الحبل لزيادة الواقعية
            switch (ropeType)
            {
                case RopeType.Rigid:
                    config.RopeColor = new Color(0.3f, 0.3f, 0.3f, 1f); // رمادي غامق معدني
                    config.StartWidth = 0.04f; config.EndWidth = 0.04f;
                    break;
                case RopeType.Nylon:
                    config.RopeColor = new Color(0.88f, 0.85f, 0.75f, 1f); // بيج خيطي كالنايلون
                    config.StartWidth = 0.03f; config.EndWidth = 0.03f;
                    break;
                case RopeType.Bungee:
                    config.RopeColor = new Color(0.85f, 0.3f, 0.1f, 1f); // برتقالي مطاطي واضح
                    config.StartWidth = 0.06f; config.EndWidth = 0.06f; // أكثر سمكاً
                    break;
            }
        }
    }
}