using UnityEngine;

namespace SwingingPaintBucket.Features.Rope.Data
{
    public static class RopeTypePresets
    {
        public static float GetSpringStiffness(RopeType ropeType)
        {
            return ropeType switch
            {
                RopeType.Rigid => 12000f,
                RopeType.Nylon => 500f,
                RopeType.Bungee => 150f,
                _ => 5000f
            };
        }

        public static float GetRopeDamping(RopeType ropeType)
        {
            return ropeType switch
            {
                RopeType.Rigid => 120f,
                RopeType.Nylon => 15f,
                RopeType.Bungee => 5f,
                _ => 100f
            };
        }

        public static void ApplyPreset(RopeConfig config, RopeType ropeType)
        {
            if (config == null) return;

            config.SpringStiffness = GetSpringStiffness(ropeType);
            config.RopeDamping = GetRopeDamping(ropeType);

            switch (ropeType)
            {
                case RopeType.Rigid:
                    config.AllowStretch = false;
                    config.RopeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                    config.StartWidth = 0.04f; config.EndWidth = 0.04f;
                    break;
                case RopeType.Nylon:
                    config.AllowStretch = true;
                    config.RopeColor = new Color(0.88f, 0.85f, 0.75f, 1f);
                    config.StartWidth = 0.03f; config.EndWidth = 0.03f;
                    break;
                case RopeType.Bungee:
                    config.AllowStretch = true;
                    config.RopeColor = new Color(0.85f, 0.3f, 0.1f, 1f);
                    config.StartWidth = 0.06f; config.EndWidth = 0.06f;
                    break;
            }
        }
    }
}