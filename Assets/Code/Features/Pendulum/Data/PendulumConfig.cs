using UnityEngine;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.Pendulum.Data
{
    /// <summary>
    /// Mutable configuration data for the pendulum system.
    /// All fields can be updated in-place via SetField / UpdateField helpers.
    /// </summary>
    public class PendulumConfig
    {
        /// <summary>Rope length in metres (used as initial length for rigid, rest length for elastic).</summary>
        public float RopeLength = 3f;

        /// <summary>Gravitational acceleration (m/s²).</summary>
        public float Gravity = 9.81f;

        /// <summary>Rotational damping coefficient (1/s).</summary>
        public float DampingCoefficient = 0.1f;

        /// <summary>Pivot world position.</summary>
        public Vector3 PivotPosition;

        /// <summary>Base mass of the bucket in kilograms.</summary>
        public float BaseMass = 1f;

        /// <summary>Initial paint mass in the bucket in kilograms.</summary>
        public float InitialPaintMass = 0.5f;

        /// <summary>Type of rope elasticity model (Rigid, Nylon, Bungee).</summary>
        public RopeType CurrentRopeType = RopeType.Rigid;

        /// <summary>Rope configuration (contains elasticity parameters for non-rigid ropes).</summary>
        public RopeConfig RopeConfig = new RopeConfig();

        /// <summary>
        /// Updates one editable field by index: 0 = RopeLength, 1 = Gravity, 2 = Damping.
        /// </summary>
        public void SetField(int fieldId, string textValue)
        {
            if (!float.TryParse(textValue, out float value)) return;

            switch (fieldId)
            {
                case 0: RopeLength = Mathf.Max(value, 0.01f); break;
                case 1: Gravity = Mathf.Max(value, 0f); break;
                case 2: DampingCoefficient = Mathf.Max(value, 0f); break;
                case 3: BaseMass = Mathf.Max(value, 0.01f); break;
                case 4: InitialPaintMass = Mathf.Max(value, 0f); break;
            }
        }

        /// <summary>
        /// Copies all editable values from another config (used when UI values change).
        /// </summary>
        public void UpdateField(PendulumConfig other)
        {
            if (other == null) return;
            RopeLength = other.RopeLength;
            Gravity = other.Gravity;
            DampingCoefficient = other.DampingCoefficient;
            BaseMass = other.BaseMass;
            InitialPaintMass = other.InitialPaintMass;
            CurrentRopeType = other.CurrentRopeType;
            RopeConfig.RestLength = other.RopeConfig.RestLength;
            RopeConfig.RopeDamping = other.RopeConfig.RopeDamping;
            RopeConfig.SpringStiffness = other.RopeConfig.SpringStiffness;
        }
    }
}
