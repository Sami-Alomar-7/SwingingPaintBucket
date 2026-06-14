using UnityEngine;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{
    /// <summary>
    /// Exposes the current total mass of the pendulum system and notifies subscribers
    /// when the mass changes (e.g., due to paint loss).
    /// </summary>
    public interface IMassProvider
    {
        /// <summary>Current total mass in kilograms.</summary>
        float GetTotalMass();

        /// <summary>Raised on the Unity main thread whenever the total mass changes.</summary>
        event System.Action<float> OnMassChanged; // float = delta mass
    }
}
