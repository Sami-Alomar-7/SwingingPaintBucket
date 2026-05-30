using UnityEngine;
using TMPro;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{
    /// <summary>
    /// Reads UI inputs and produces a PendulumConfig and PendulumState.
    /// </summary>
    public interface IPendulumInputHandler
    {
        /// <summary>
        /// Reads all five UI fields and returns a fresh PendulumConfig.
        /// </summary>
        PendulumConfig ReadPendulumConfig();

        /// <summary>
        /// Reads the angle and angular-velocity inputs and returns a PendulumState.
        /// </summary>
        PendulumState ReadPendulumState();
    }
}
