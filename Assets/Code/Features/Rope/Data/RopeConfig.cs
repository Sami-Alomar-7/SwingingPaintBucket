using UnityEngine;

namespace SwingingPaintBucket.Features.Rope.Data
{
    /// <summary>
    /// Rope configuration parameters shared between rope implementations.
    /// </summary>
    public class RopeConfig
    {
        /// <summary>Rest length of the rope when no load is applied (metres).</summary>
        public float RestLength = 3f;

        /// <summary>Linear drag applied to rope oscillation (1/s).</summary>
        public float RopeDamping = 0f;

        /// <summary>Spring stiffness coefficient (N/m) for elastic ropes.</summary>
        public float SpringStiffness = 5000f;

        /// <summary>Whether the rope can stretch beyond RestLength.</summary>
        public bool  AllowStretch = false;

        /// <summary>Colour of the rendered rope.</summary>
        public Color RopeColor = Color.gray;

        /// <summary>Width of the rope at the pivot end.</summary>
        public float StartWidth = 0.05f;

        /// <summary>Width of the rope at the bucket end.</summary>
        public float EndWidth = 0.05f;
    }
}
