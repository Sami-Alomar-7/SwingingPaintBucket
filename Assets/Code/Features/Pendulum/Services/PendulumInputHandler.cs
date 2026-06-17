using UnityEngine;
using TMPro;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.Pendulum.Services
{
    [System.Serializable]
    public class PendulumInputHandler : IPendulumInputHandler
    {
        public TMP_InputField lengthInput;
        public TMP_InputField gravityInput;
        public TMP_InputField dampingInput;
        public TMP_InputField angleInput;
        public TMP_InputField angularVelocityInput;
        public TMP_InputField baseMassInput;
        public TMP_InputField initialPaintMassInput;
        public TMP_InputField ropeTypeInput;

        private const float DefaultLength = 3f;
        private const float DefaultGravity = 9.81f;
        private const float DefaultDamping = 0.05f;
        private const float DefaultAngleDeg = 45f;
        private const float DefaultOmega = 0f;
        private const float DefaultBaseMass = 1f;
        private const float DefaultPaintMass = 0.5f;
        private const RopeType DefaultRopeType = RopeType.Rigid;

        public PendulumInputHandler() { }

        public PendulumInputHandler(TMP_InputField length, TMP_InputField gravity, TMP_InputField damping, TMP_InputField angle, TMP_InputField angularVelocity)
        {
            lengthInput = length; gravityInput = gravity; dampingInput = damping; angleInput = angle; angularVelocityInput = angularVelocity;
        }

        public PendulumInputHandler(TMP_InputField length, TMP_InputField gravity, TMP_InputField damping, TMP_InputField angle, TMP_InputField angularVelocity, TMP_InputField baseMass, TMP_InputField initialPaintMass)
        {
            lengthInput = length; gravityInput = gravity; dampingInput = damping; angleInput = angle; angularVelocityInput = angularVelocity; baseMassInput = baseMass; initialPaintMassInput = initialPaintMass;
        }

        public PendulumConfig ReadPendulumConfig()
        {
            var config = new PendulumConfig();
            config.RopeLength = ParsePositive(lengthInput?.text, DefaultLength);
            config.Gravity = ParseNonNegative(gravityInput?.text, DefaultGravity);
            config.DampingCoefficient = ParseNonNegative(dampingInput?.text, DefaultDamping);
            config.BaseMass = ParsePositive(baseMassInput?.text, DefaultBaseMass);
            config.InitialPaintMass = ParseNonNegative(initialPaintMassInput?.text, DefaultPaintMass);

            if (ropeTypeInput != null && !string.IsNullOrEmpty(ropeTypeInput.text))
            {
                string ropeTypeText = ropeTypeInput.text.Trim();
                if (System.Enum.TryParse<RopeType>(ropeTypeText, true, out RopeType parsedType))
                    config.CurrentRopeType = parsedType;
                else
                    config.CurrentRopeType = DefaultRopeType;
            }
            else
            {
                config.CurrentRopeType = DefaultRopeType;
            }

            if (config.CurrentRopeType != RopeType.Rigid)
            {
                RopeTypePresets.ApplyPreset(config.RopeConfig, config.CurrentRopeType);
            }

            return config;
        }

        public PendulumState ReadPendulumState()
        {
            float angleDeg = ParseFloat(angleInput?.text, DefaultAngleDeg);
            float omega = ParseFloat(angularVelocityInput?.text, DefaultOmega);

            return new PendulumState
            {
                Theta = angleDeg * Mathf.Deg2Rad,
                Omega = omega
            };
        }

        private static float ParseFloat(string text, float fallback)
            => float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : fallback;

        private static float ParsePositive(string text, float fallback)
        {
            float v = ParseFloat(text, fallback);
            return v > 0f ? v : fallback;
        }

        private static float ParseNonNegative(string text, float fallback)
        {
            float v = ParseFloat(text, fallback);
            return v >= 0f ? v : fallback;
        }
    }
}