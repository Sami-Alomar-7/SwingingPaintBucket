using UnityEngine;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;

namespace SwingingPaintBucket.Features.Paint.Services
{
    /// <summary>
    /// Emits paint particles that drip downward from the bucket.
    ///
    /// Physics model:
    ///   - Particles inherit the bucket's horizontal velocity (they move with the swing).
    ///   - A small random horizontal spread simulates the hole in the bucket.
    ///   - Initial vertical velocity is zero or slightly downward — gravity does the rest.
    ///   - Emission rate scales with bucket speed (faster swing = more paint spills).
    /// </summary>
    public class DynamicPaintEmissionService : IPaintEmissionService
    {
        /// <inheritdoc />
        public float CalculateEmissionRate(Vector3 bucketVelocity, PaintEmissionConfig config)
        {
            if (!config.useDynamicEmission)
                return config.baseSpawnRate;

            float speed = bucketVelocity.magnitude;

            // Hole size affects emission rate: larger hole = more paint flow
            // Base calculation: hole area (π * r²) scales emission
            float holeRadius = config.holeDiameter / 2f;
            float holeArea = Mathf.PI * holeRadius * holeRadius;
            float holeMultiplier = holeArea * 10000f; // Scale factor for reasonable emission rates

            float dynamicRate = speed * config.velocityMultiplier * holeMultiplier;
            return Mathf.Clamp(dynamicRate, config.minSpawnRate, config.maxSpawnRate);
        }

        /// <inheritdoc />
        public void EmitParticle(
            PaintEmissionConfig config,
            Vector3 spawnPosition,
            Vector3 bucketVelocity,
            System.Collections.Generic.List<ParticleData> particles)
        {
            // ── Velocity model ────────────────────────────────────────────────────
            // Paint droplets inherit only a FRACTION of the bucket's horizontal velocity.
            // Real physics: paint drips from a small hole, velocity transfer is inefficient.
            // The hole size (splashRange.x) controls horizontal spread.
            // Vertical component starts near zero — gravity does the rest.

            float holeSpread = config.splashRange.x;   // horizontal spread = hole size
            float velocityTransferRatio = 0.15f;        // Only 15% of bucket velocity transfers to droplets

            Vector3 initialVelocity = new Vector3(
                bucketVelocity.x * velocityTransferRatio + Random.Range(-holeSpread, holeSpread),
                Random.Range(-0.5f, 0.2f),              // slight downward bias
                0f                                      // Z = 0 (pendulum moves in X-Y plane)
            );

            // ── Color ─────────────────────────────────────────────────────────────
            // Use the configured paint color with a tiny brightness variation
            // so the splatter looks natural rather than flat.
            Color baseColor = config.particleColor;
            float variation = Random.Range(-0.08f, 0.08f);
            Color particleColor = new Color(
                Mathf.Clamp01(baseColor.r + variation),
                Mathf.Clamp01(baseColor.g + variation),
                Mathf.Clamp01(baseColor.b + variation),
                1f
            );

            // ── Size variation ────────────────────────────────────────────────────
            float sizeVariation = Random.Range(0.7f, 1.3f);
            float particleSize  = config.particleSize * sizeVariation;

            particles.Add(new ParticleData(
                spawnPosition,
                initialVelocity,
                config.particleLife,
                particleColor,
                particleSize
            ));
        }
    }
}
