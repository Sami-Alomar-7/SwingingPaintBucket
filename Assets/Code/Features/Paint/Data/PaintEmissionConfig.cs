using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Data
{
    /// <summary>
    /// Configuration for paint emission
    /// </summary>
    [System.Serializable]
    public class PaintEmissionConfig
    {
        [Tooltip("Base emission rate (particles per second) when not using dynamic emission")]
        public float baseSpawnRate = 10f;
        
        [Tooltip("Whether emission rate should be dynamically based on bucket velocity")]
        public bool useDynamicEmission = true;
        
        [Tooltip("Minimum emission rate (particles per second)")]
        public float minSpawnRate = 5f;
        
        [Tooltip("Maximum emission rate (particles per second)")]
        public float maxSpawnRate = 120f;
        
        [Tooltip("How strongly bucket speed affects emission rate")]
        public float velocityMultiplier = 8f;
        
        [Tooltip("Color of the paint particles")]
        public Color particleColor = Color.red;
        
        [Tooltip("Size of paint particles")]
        public float particleSize = 0.1f;
        
        [Tooltip("Lifetime of paint particles in seconds")]
        public float particleLife = 5f;
        
        [Tooltip("Range of random splash velocity added to particles")]
        public Vector3 splashRange = new Vector3(1f, 2f, 1f);

        [Tooltip("Bucket hole diameter in meters (affects emission rate: larger hole = more paint flow)")]
        public float holeDiameter = 0.01f; // 1cm default hole size
        
        [Tooltip("Gravity affecting paint particles")]
        public float gravity = 9.81f;

        [Tooltip("Mass of each paint particle in kilograms (realistic water drop ~0.00005 kg = 0.05g)")]
        public float particleMass = 0.00001f; // 0.01g per particle for fluid-like simulation (more particles)
    }
}