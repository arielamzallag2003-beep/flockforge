using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace PalaceOfFantasy.FlockForge.Samples.Epervier
{
    public class ArrivalBehaviour : IBehaviour
    {
        public string Name => "Arrival";
        public bool IsEnabled { get; set; } = true;
        public float Weight { get; set; } = 1f;
        public float SlowingRadius { get; set; } = 5f;

        public FVector3 CalculateForce(IBoidContext context)
        {
            if (context.SeekTarget == null) return FVector3.Zero;
            
            var targetOffset = context.SeekTarget.Value - context.Self.Position;
            var distance = targetOffset.Magnitude;
            var rampedSpeed = context.Settings.MaxSpeed * (distance / SlowingRadius);
            var clippedSpeed = Mathf.Min(rampedSpeed, context.Settings.MaxSpeed);
            
            var desired = (distance > 0) ? (targetOffset / distance) * clippedSpeed : FVector3.Zero;
            
            return desired - context.Self.Velocity;
        }
    }

    [CreateAssetMenu(menuName = "FlockForge/Behaviors/Arrival")]
    public class ArrivalAsset : BehaviourAsset
    {
        public float weight = 1f;
        public float slowingRadius = 5f;

        public override IBehaviour CreateBehaviour()
            => new ArrivalBehaviour 
            { 
                Weight = weight, 
                IsEnabled = _isEnabled,
                SlowingRadius = slowingRadius 
            };
    }
}
