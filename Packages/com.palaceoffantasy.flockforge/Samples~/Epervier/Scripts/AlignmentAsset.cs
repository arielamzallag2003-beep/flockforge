using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace PalaceOfFantasy.FlockForge.Samples.Epervier
{
    public class AlignmentBehaviour : IBehaviour
    {
        public string Name => "Alignment";
        public bool IsEnabled { get; set; } = true;
        public float Weight { get; set; } = 1f;

        public FVector3 CalculateForce(IBoidContext context)
        {
            if (context.Neighbors.Count == 0) return FVector3.Zero;

            FVector3 avgVel = FVector3.Zero;
            foreach (var n in context.Neighbors) avgVel += n.Velocity;
            avgVel /= context.Neighbors.Count;

            return avgVel.Normalized * context.Settings.MaxSpeed;
        }
    }

    [CreateAssetMenu(menuName = "FlockForge/Behaviors/Alignment")]
    public class AlignmentAsset : BehaviourAsset
    {
        public float weight = 1f;

        public override IBehaviour CreateBehaviour()
            => new AlignmentBehaviour { Weight = weight, IsEnabled = _isEnabled };
    }
}
