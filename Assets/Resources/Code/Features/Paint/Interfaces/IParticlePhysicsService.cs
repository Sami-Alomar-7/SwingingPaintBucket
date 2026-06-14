using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;

namespace SwingingPaintBucket.Features.Paint.Interfaces
{
    public interface IParticlePhysicsService
    {
        /// <summary>
        /// تحديث فيزياء الجسيمات بناءً على خوارزمية SPH المتوافقة مع Müller 2003
        /// </summary>
        List<int> UpdateParticles(List<ParticleData> particles, float deltaTime, float surfaceY, PaintEmissionConfig config);
    }
}