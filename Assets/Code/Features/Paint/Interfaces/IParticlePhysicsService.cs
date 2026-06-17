using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;

namespace SwingingPaintBucket.Features.Paint.Interfaces
{
    public interface IParticlePhysicsService
    {
        List<int> UpdateParticles(List<ParticleData> particles, float deltaTime, float surfaceY, PaintEmissionConfig config);
    }
}