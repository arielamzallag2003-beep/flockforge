using UnityEngine;
using System.Collections.Generic;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace PalaceOfFantasy.FlockForge.Unity.Components
{
    public class FlockManager : MonoBehaviour, IFlockSettings
    {
        [SerializeField] private string _flockId = "Flock";
        [SerializeField] private bool _useFixedTimestep = false;
        [SerializeField] private float _fixedTimestep = 0.02f;
        [SerializeField] private BehaviourAsset[] _defaultBehaviours;

        private Flock _flock;

        public IFlock Flock => _flock;
        public bool UseFixedTimestep => _useFixedTimestep;
        public float FixedTimestep => _fixedTimestep;

        public IReadOnlyList<IBehaviour> DefaultBehaviours => _runtimeBehaviours;
        private List<IBehaviour> _runtimeBehaviours = new();

        private void Awake()
        {

            foreach (var asset in _defaultBehaviours)
            {
                if (asset != null)
                {
                    _runtimeBehaviours.Add(asset.CreateBehaviour());
                }
            }

            _flock = new Flock(_flockId, this);
            
            var provider = GetComponent<ITargetProvider>();
            if (provider != null)
            {
                _flock.TargetProvider = provider;
            }
        }

        private void OnEnable()
        {
            foreach (Transform child in transform)
            {
                var agent = child.GetComponent<BoidAgent>();
                if (agent != null)
                {
                    _flock.Register(agent);
                }
            }
        }

        private void Update()
        {
            if (!_useFixedTimestep)
            {
                _flock.Step(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_useFixedTimestep)
            {
                _flock.Step(Time.fixedDeltaTime); // Or use _fixedTimestep accumulator
            }
        }
    }
}
