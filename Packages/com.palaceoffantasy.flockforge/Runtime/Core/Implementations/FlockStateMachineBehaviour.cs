using System;
using System.Collections.Generic;

namespace PalaceOfFantasy.FlockForge.Core
{
    public class FlockStateMachineBehaviour : IBehaviour
    {
        private readonly Func<IStateMachine> _factory;
        private readonly Dictionary<int, IStateMachine> _machines = new();
        
        public string Name { get; } = "StateMachine";
        public float Weight { get; set; }
        public bool IsEnabled { get; set; }

        public FlockStateMachineBehaviour(Func<IStateMachine> factory, float weight = 1f, bool isEnabled = true)
        {
            _factory = factory;
            Weight = weight;
            IsEnabled = isEnabled;
        }

        public FVector3 CalculateForce(IBoidContext context)
        {
            if (!IsEnabled) return FVector3.Zero;

            if (!_machines.TryGetValue(context.Self.Id, out var machine))
            {
                machine = _factory();
                _machines[context.Self.Id] = machine;
            }

            machine.Update(context.Self, context);

            if (machine.CurrentState == null)
            {
                if (context.Self.Id == 1) UnityEngine.Debug.LogWarning($"[Boid 1] No Current State!");
                return FVector3.Zero;
            }

            FVector3 totalForce = FVector3.Zero;
            foreach (var behaviour in machine.CurrentState.Behaviours)
            {
                if (behaviour.IsEnabled)
                {
                    var f = behaviour.CalculateForce(context);
                    totalForce += f * behaviour.Weight;
                }
            }
            
            return totalForce;
        }
    }
}
