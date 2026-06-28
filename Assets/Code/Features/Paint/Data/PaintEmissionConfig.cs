using UnityEngine;

namespace SwingingPaintBucket.Features.Paint.Data
{
    [System.Serializable]
    public class PaintEmissionConfig
    {
        [Header("Standard Emission Settings")]
        public float baseSpawnRate = 120f; 
        public bool useDynamicEmission = true;
        public float minSpawnRate = 50f;
        public float maxSpawnRate = 350f;
        public float velocityMultiplier = 12f;
        public Color particleColor = Color.red;
        public float particleSize = 0.14f;
        public float particleLife = 6f;
        public Vector3 splashRange = Vector3.zero; 
        public float particleMass = 0.001f;
        public float gravity = 15f;

        [Tooltip("Hole diameter used by SceneBuilder initialization")]
        public float holeDiameter = 0.05f;

        [Header("SPH Fluid Physics Settings (Müller 2003)")]
        public float smoothingRadius = 0.1f; 
        public float restDensity = 100f;     
        public float pressureStiffness = 150f;
        public float viscosity = 0.1f;       
        public float surfaceTension = 0.05f;
    }
}