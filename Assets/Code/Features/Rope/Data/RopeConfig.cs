using UnityEngine;

namespace SwingingPaintBucket.Features.Rope.Data
{

    public class RopeConfig
    {
        public float RestLength = 3f;

        public float RopeDamping = 0f;

        public float SpringStiffness = 5000f;

        public bool AllowStretch = false;

        public Color RopeColor = Color.gray;

        public float StartWidth = 0.05f;

        public float EndWidth = 0.05f;
    }
}