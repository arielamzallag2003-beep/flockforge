using UnityEngine;
using System.Collections.Generic;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Extensions;

namespace PalaceOfFantasy.FlockForge.Unity.Components
{
    public class FlockManager : MonoBehaviour, IFlockSettings
    {
        [SerializeField] private string _flockId = "Flock";
        [SerializeField] private bool _useFixedTimestep = false;
        [SerializeField] private float _fixedTimestep = 0.02f;
        [SerializeField] private BoidAgent _boidPrefab;
        [SerializeField] private int _spawnCount = 50;
        [SerializeField] private float _spawnRadius = 10f;
        [SerializeField] private BehaviourAsset[] _defaultBehaviours;

        private Flock _flock;

        public IFlock Flock => _flock;
        public bool UseFixedTimestep => _useFixedTimestep;
        public float FixedTimestep => _fixedTimestep;
        public FVector3 AnchorPosition => transform.position.ToFVector3();

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
            
            // Spawning logic
            if (_boidPrefab != null)
            {
                // Determine spawn function based on movement plane
                var plane = MovementPlane.Free3D;
                if (_boidPrefab.Settings != null)
                {
                    plane = _boidPrefab.Settings.MovementPlane;
                }

                for (int i = 0; i < _spawnCount; i++) 
                {
                    Vector3 randomPos = Vector3.zero;
                    
                    switch (plane)
                    {
                        case MovementPlane.XY:
                            randomPos = (Vector3)UnityEngine.Random.insideUnitCircle * _spawnRadius;
                            break;
                            
                        case MovementPlane.XZ:
                            var circle = UnityEngine.Random.insideUnitCircle * _spawnRadius;
                            randomPos = new Vector3(circle.x, 0, circle.y);
                            break;
                            
                        case MovementPlane.Free3D:
                        default:
                            randomPos = UnityEngine.Random.insideUnitSphere * _spawnRadius;
                            break;
                    }

                    var pos = transform.position + randomPos;
                    var go = Instantiate(_boidPrefab, pos, Quaternion.identity, transform);
                    var agent = go.GetComponent<BoidAgent>();
                    if (agent != null)
                    {
                        // Ensure they start with the correct rotation if needed (facing random direction)
                        if (plane == MovementPlane.XZ)
                            go.transform.forward = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                        else
                            go.transform.rotation = Random.rotation;

                        _flock.Register(agent);
                    }
                }
            }

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
