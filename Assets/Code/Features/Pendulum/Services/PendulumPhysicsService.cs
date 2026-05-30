using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;

namespace SwingingPaintBucket.Features.Pendulum.Services
{
    /// <summary>
    /// Implements vector-based pendulum mechanics. Aggregates all registered
    /// force providers and resolves them within the vertical X-Y swing plane.
    /// </summary>
    public class PendulumPhysicsService : IPendulumPhysics
    {
        public float ComputeAngularAcceleration(
            PendulumState state, 
            PendulumConfig config, 
            float totalMass, 
            IList<IForceProvider> forceProviders)
        {
            if (totalMass <= 0.001f) totalMass = 0.001f;
            float length = config.RopeLength;

            // 1. Establish the vertical X-Y tangential axis vector
            // Position = (L*sinθ, -L*cosθ, 0). Derivative w.r.t θ = (L*cosθ, L*sinθ, 0)
            Vector3 tangentialDirection = new Vector3(Mathf.Cos(state.Theta), Mathf.Sin(state.Theta), 0f);
            tangentialDirection.Normalize();

            // 2. Aggregate all active forces acting on the rigid body
            Vector3 netForceWorld = Vector3.zero;

            if (forceProviders != null && forceProviders.Count > 0)
            {
                for (int i = 0; i < forceProviders.Count; i++)
                {
                    if (forceProviders[i] != null)
                    {
                        netForceWorld += forceProviders[i].GetForce(state, config);
                    }
                }
            }
            else
            {
                // Fallback calculations if no external providers are registered
                float linearVelocityMag = state.Omega * length;
                Vector3 linearVelocity = tangentialDirection * linearVelocityMag;
                
                Vector3 gravityForce = totalMass * config.Gravity * Vector3.down;
                Vector3 dragForce = -config.DampingCoefficient * linearVelocity * linearVelocity.magnitude;
                
                netForceWorld = gravityForce + dragForce;
            }

            // 3. Project the net 3D vector force onto our localized tangential axis
            float netTangentialForce = Vector3.Dot(netForceWorld, tangentialDirection);

            // 4. Convert linear force back to angular acceleration (α = F / (m * L))
            return netTangentialForce / (totalMass * length);
        }
    }
}