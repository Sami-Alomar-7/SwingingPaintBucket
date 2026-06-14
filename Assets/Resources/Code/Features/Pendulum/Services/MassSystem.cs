using UnityEngine;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.Paint.Interfaces;

namespace SwingingPaintBucket.Features.Pendulum.Services
{
    /// <summary>
    /// Tracks the pendulum's base mass plus any paint accumulated in the bucket.
    /// Implements both IMassProvider (exposes total mass) and IMassLossNotifier
    /// (receives mass-loss callbacks from the Paint section).
    ///
    /// Also implements IPendulumView so a single instance can be injected as
    /// both the view and the mass system into PendulumController.
    /// </summary>
    public class MassSystem : IMassProvider, IMassLossNotifier
    {
        /// <summary>Base (dry-bucket) mass in kilograms.</summary>
        public float BaseMass { get; set; }

        /// <summary>Additional mass from paint currently in the bucket.</summary>
        public float PaintMass { get; private set; }

        /// <inheritdoc />
        public float GetTotalMass() => BaseMass + PaintMass;

        /// <inheritdoc />
        public event System.Action<float> OnMassChanged;

        public MassSystem(float baseMass = 1f) => BaseMass = baseMass;

        /// <summary>
        /// Adjusts the accumulated paint mass by the given delta (kg),
        /// then fires the OnMassChanged event.
        /// </summary>
        /// <param name="deltaMass">Positive = mass added; negative = mass lost.</param>
        public void AdjustPaintMass(float deltaMass)
        {
            if (Mathf.Approximately(deltaMass, 0f)) return;

            PaintMass = Mathf.Max(0f, PaintMass + deltaMass);
            OnMassChanged?.Invoke(deltaMass);
        }

        /// <inheritdoc />
        public void NotifyParticleEmitted(float particleMass)
        {
            // Subtract particle mass from paint mass, clamping to zero
            AdjustPaintMass(-particleMass);
        }
    }
}
