using UnityEngine;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.Paint.Interfaces;

namespace SwingingPaintBucket.Features.Pendulum.Services
{
    public class MassSystem : IMassProvider, IMassLossNotifier
    {
        public float BaseMass { get; set; }
        public float PaintMass { get; private set; }

        public float GetTotalMass() => BaseMass + PaintMass;
        public event System.Action<float> OnMassChanged;

        public MassSystem(float baseMass = 1f) => BaseMass = baseMass;

        public void AdjustPaintMass(float deltaMass)
        {
            if (Mathf.Approximately(deltaMass, 0f)) return;

            PaintMass = Mathf.Max(0f, PaintMass + deltaMass);
            OnMassChanged?.Invoke(deltaMass);
        }

        public void NotifyParticleEmitted(float particleMass)
        {
            AdjustPaintMass(-particleMass);
        }
    }
}