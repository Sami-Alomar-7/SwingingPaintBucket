using System.Collections.Generic;
using UnityEngine;
using SwingingPaintBucket.Features.Paint.Data;

namespace SwingingPaintBucket.Features.Paint.Interfaces
{
    public interface IPaintEmissionService
    {
        float CalculateEmissionRate(Vector3 bucketVelocity, PaintEmissionConfig config);
        void EmitParticle(PaintEmissionConfig config, Vector3 spawnPosition, Vector3 bucketVelocity, List<ParticleData> particles);
    }
}