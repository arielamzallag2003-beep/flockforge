using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace PalaceOfFantasy.FlockForge.Samples.Epervier
{
    public class SeekBehaviour : IBehaviour
    {
        public string Name => "Seek";
        public bool IsEnabled { get; set; } = true;
        public float Weight { get; set; } = 1f;

        public FVector3 CalculateForce(IBoidContext context)
        {
            if (context.SeekTarget == null) return FVector3.Zero;
            var desired = (context.SeekTarget.Value - context.Self.Position).Normalized * context.Settings.MaxSpeed;
            return desired - context.Self.Velocity;
        }
    }

    [CreateAssetMenu(menuName = "FlockForge/Behaviors/Seek")]
    public class SeekAsset : BehaviourAsset
    {
        public float weight = 1f;

        public override IBehaviour CreateBehaviour()
            => new SeekBehaviour { Weight = weight, IsEnabled = _isEnabled };
    }
}
