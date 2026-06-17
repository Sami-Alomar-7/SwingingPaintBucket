namespace SwingingPaintBucket.Features.Rope.Data
{

    public static class RopeTypePresets
    {

        public static float GetSpringStiffness(RopeType ropeType)
        {
            return ropeType switch
            {
                RopeType.Rigid => 5000f,
                RopeType.Nylon => 800f,
                RopeType.Bungee => 150f,
                _ => 5000f
            };
        }

   
        public static float GetRopeDamping(RopeType ropeType)
        {
            return ropeType switch
            {
                RopeType.Rigid => 100f,
                RopeType.Nylon => 35f,
                RopeType.Bungee => 5f,
                _ => 100f
            };
        }

 
        public static void ApplyPreset(RopeConfig config, RopeType ropeType)
        {
            if (config == null) return;
            config.SpringStiffness = GetSpringStiffness(ropeType);
            config.RopeDamping = GetRopeDamping(ropeType);
        }
    }
}