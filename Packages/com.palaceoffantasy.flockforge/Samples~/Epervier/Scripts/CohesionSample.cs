using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Extensions;

namespace PalaceOfFantasy.FlockForge.Samples.Epervier
{
    public class CohesionBehaviour : IBehaviour
    {
        public string Name => "Cohesion";
        public bool IsEnabled { get; set; } = true;
        public float Weight { get; set; } = 1f;

        public FVector3 CalculateForce(IBoidContext context)
        {
            if (context.Neighbors.Count == 0) return FVector3.Zero;

            var center = FVector3.Zero;
            foreach (var n in context.Neighbors) center += n.Position;
            center /= context.Neighbors.Count;

            var direction = center - context.Self.Position;
            return direction.Normalized * context.Settings.MaxSpeed;
        }
    }

    [CreateAssetMenu(menuName = "FlockForge Samples/Behaviors/Cohesion")]
    public class CohesionAsset : BehaviourAsset
    {
        public override IBehaviour CreateBehaviour()
        {
            return new CohesionBehaviour 
            { 
                Weight = _weight, 
                IsEnabled = _isEnabled 
            };
        }
    }
}
