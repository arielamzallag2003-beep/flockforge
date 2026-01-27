using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Extensions;

namespace PalaceOfFantasy.FlockForge.Unity.Components
{
    public class BoidAgent : MonoBehaviour, IBoid
    {
        [SerializeField] private BoidProfile _profile;

        private IFlock _flock;
        private FVector3 _velocity;
        private FVector3 _accumulatedForce;
        private static int _nextId = 0;

        public int Id { get; private set; }
        public string Tag { get => tag; set => tag = value; }
        public bool IsActive
        {
            get => gameObject.activeInHierarchy;
            set => gameObject.SetActive(value);
        }

        public FVector3 Position
        {
            get => transform.position.ToFVector3();
            set => transform.position = value.ToUnityVector3();
        }

        public FVector3 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        public FVector3 Forward => transform.forward.ToFVector3();

        public IFlock Flock
        {
            get => _flock;
            set => _flock = value;
        }

        public IBoidSettings Settings => _profile;
        public FVector3 AccumulatedForce => _accumulatedForce;

        private void Awake()
        {
            Id = _nextId++;
        }

        public void AddForce(FVector3 force)
        {
            _accumulatedForce += force;
        }

        public void ApplyForces(float deltaTime)
        {
            var maxSpeed = Settings.MaxSpeed;
            var mass = Settings.Mass;
            var drag = Settings.Drag;

            // Apply accumulated force (integration)
            var acceleration = _accumulatedForce / mass;
            _velocity += acceleration * deltaTime;

            // Apply drag
            _velocity *= (1f - drag * deltaTime);

            // Clamp velocity
            _velocity = FVector3.ClampMagnitude(_velocity, maxSpeed);

            // Update position
            transform.position += _velocity.ToUnityVector3() * deltaTime;

            // Update rotation
            if (_velocity.SqrMagnitude > 0.01f)
            {
                transform.forward = _velocity.ToUnityVector3();
            }

            // Reset force
            _accumulatedForce = FVector3.Zero;
        }
    }
}
