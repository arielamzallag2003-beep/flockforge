using System;
using System.Collections.Generic;
using System.Linq;

namespace PalaceOfFantasy.FlockForge.Core
{
    public class Flock : IFlock
    {
        public string Id { get; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public IFlockSettings Settings { get; }

        private readonly List<IBoid> _boids = new();
        private readonly BoidContext _contextCache = new();
        private float _totalTime;

        public IReadOnlyList<IBoid> Boids => _boids;
        public int ActiveCount => _boids.Count(b => b.IsActive);

        public event Action<IBoid> OnBoidAdded;
        public event Action<IBoid> OnBoidRemoved;

        public ITargetProvider TargetProvider { get; set; }

        public Flock(string id, IFlockSettings settings)
        {
            Id = id;
            Name = id;
            Settings = settings;
        }

        public void Register(IBoid boid)
        {
            if (!_boids.Contains(boid))
            {
                _boids.Add(boid);
                boid.Flock = this;
                OnBoidAdded?.Invoke(boid);
            }
        }

        public void Unregister(IBoid boid)
        {
            if (_boids.Remove(boid))
            {
                boid.Flock = null;
                OnBoidRemoved?.Invoke(boid);
            }
        }

        public void Clear()
        {
            foreach (var boid in _boids.ToList())
                Unregister(boid);
        }

        public void Step(float deltaTime)
        {
            if (!IsActive) return;

            _totalTime += deltaTime;

            foreach (var boid in _boids)
            {
                if (!boid.IsActive) continue;

                var context = BuildContext(boid, deltaTime);

                foreach (var behaviour in Settings.DefaultBehaviours)
                {
                    if (!behaviour.IsEnabled) continue;
                    var force = behaviour.CalculateForce(context);
                    boid.AddForce(force * behaviour.Weight);
                }

                boid.ApplyForces(deltaTime);
            }
        }

        private IBoidContext BuildContext(IBoid boid, float deltaTime)
        {
            _contextCache.Self = boid;
            _contextCache.Flock = this;
            _contextCache.DeltaTime = deltaTime;
            _contextCache.TotalTime = _totalTime;
            _contextCache.Neighbors = GetNeighbors(boid);

            if (TargetProvider != null)
            {
                _contextCache.SeekTarget = TargetProvider.GetSeekTarget(boid);
                _contextCache.Threats = TargetProvider.GetThreats(boid);
                _contextCache.NearbyObstacles = TargetProvider.GetNearbyObstacles(boid);
            }
            else
            {
                _contextCache.SeekTarget = null;
            }

            return _contextCache;
        }

        public IReadOnlyList<IBoid> GetNeighbors(IBoid boid)
        {
            var radius = boid.Settings.PerceptionRadius;
            var maxNeighbors = boid.Settings.MaxNeighbors;

            return _boids
                .Where(b => b != boid && b.IsActive)
                .Where(b => FVector3.SqrDistance(b.Position, boid.Position) < radius * radius)
                .Take(maxNeighbors)
                .ToList();
        }

        public FVector3 GetCenterOfMass()
        {
            if (_boids.Count == 0) return FVector3.Zero;

            var sum = FVector3.Zero;
            int count = 0;
            foreach (var boid in _boids)
            {
                if (!boid.IsActive) continue;
                sum = sum + boid.Position;
                count++;
            }
            return count > 0 ? sum / count : FVector3.Zero;
        }

        public FVector3 GetAverageVelocity()
        {
            if (_boids.Count == 0) return FVector3.Zero;

            var sum = FVector3.Zero;
            int count = 0;
            foreach (var boid in _boids)
            {
                if (!boid.IsActive) continue;
                sum = sum + boid.Velocity;
                count++;
            }
            return count > 0 ? sum / count : FVector3.Zero;
        }
    }
}
