using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace PalaceOfFantasy.FlockForge.Samples.Epervier
{
    public class SeparationBehaviour : IBehaviour
    {
        public string Name => "Separation";
        public bool IsEnabled { get; set; } = true;
        public float Weight { get; set; } = 1.5f;
        public float Radius { get; set; } = 2f;

        public FVector3 CalculateForce(IBoidContext context)
        {
            if (context.Neighbors.Count == 0) return FVector3.Zero;

            FVector3 force = FVector3.Zero;
            foreach (var n in context.Neighbors)
            {
                var diff = context.Self.Position - n.Position;
                var dist = diff.Magnitude;
                if (dist < Radius && dist > 0.001f)
                {
                    force += diff.Normalized / dist;
                }
            }
            return force.Normalized * context.Settings.MaxSpeed;
        }
    }

    [CreateAssetMenu(menuName = "FlockForge/Behaviors/Separation")]
    public class SeparationAsset : BehaviourAsset
    {
        public float weight = 1.5f;
        public float radius = 2f;

        public override IBehaviour CreateBehaviour()
            => new SeparationBehaviour { Weight = weight, Radius = radius, IsEnabled = _isEnabled };
    }
}
