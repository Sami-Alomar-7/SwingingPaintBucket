using UnityEngine;

namespace SwingingPaintBucket.Features.Pendulum.Interfaces
{
    public interface IMassProvider
    {
        float GetTotalMass();
        event System.Action<float> OnMassChanged;
    }
}
