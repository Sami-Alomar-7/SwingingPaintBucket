using UnityEngine;
using TMPro;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{

    public interface IPendulumInputHandler
    {
        PendulumConfig ReadPendulumConfig();
        PendulumState ReadPendulumState();
    }
}
