namespace SwingingPaintBucket.Features.Rope.Data
{
    /// <summary>
    /// Preset configuration values for different rope types.
    /// Maps rope types to their physical properties (stiffness and damping).
    /// </summary>
    public static class RopeTypePresets
    {
        /// <summary>
        /// Gets the spring stiffness coefficient for a given rope type.
        /// </summary>
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

        /// <summary>
        /// Gets the rope damping coefficient for a given rope type.
        /// </summary>
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

        /// <summary>
        /// Applies the preset values to a rope configuration based on the current rope type.
        /// </summary>
        public static void ApplyPreset(RopeConfig config, RopeType ropeType)
        {
            if (config == null) return;
            config.SpringStiffness = GetSpringStiffness(ropeType);
            config.RopeDamping = GetRopeDamping(ropeType);
        }
    }
}
