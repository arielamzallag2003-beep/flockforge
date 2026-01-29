using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Components;
using PalaceOfFantasy.FlockForge.Unity.Extensions;

namespace PalaceOfFantasy.FlockForge.Unity.Debugging
{
    /// <summary>
    /// Comprehensive debugger for FlockForge state machines.
    /// Attach this to your FlockManager or any GameObject in the scene.
    /// </summary>
    public class FlockForgeDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private bool _logEveryFrame = false;
        [SerializeField] private float _logInterval = 1f;
        [SerializeField] private bool _drawDebugLines = true;
        [SerializeField] private int _maxBoidsToLog = 3;

        [Header("References")]
        [SerializeField] private FlockManager _flockManager;
        [SerializeField] private Transform _goalTransform;
        [SerializeField] private List<Transform> _enemyTransforms = new();

        private float _lastLogTime;
        private Dictionary<int, StateDebugInfo> _stateHistory = new();

        private void Start()
        {
            if (_flockManager == null)
                _flockManager = FindFirstObjectByType<FlockManager>();

            LogSetup();
        }

        private void Update()
        {
            if (!_enableLogging) return;

            bool shouldLog = _logEveryFrame || (Time.time - _lastLogTime >= _logInterval);

            if (shouldLog)
            {
                LogFlockState();
                _lastLogTime = Time.time;
            }

            if (_drawDebugLines)
            {
                DrawDebugVisualization();
            }
        }

        private void LogSetup()
        {
            UnityEngine.Debug.Log("=== FLOCKFORGE DEBUG SETUP ===");

            if (_flockManager == null)
            {
                UnityEngine.Debug.LogError("FlockManager is NULL! Cannot debug.");
                return;
            }

            UnityEngine.Debug.Log($"FlockManager found: {_flockManager.name}");
            UnityEngine.Debug.Log($"Flock ID: {_flockManager.Flock.Id}");
            UnityEngine.Debug.Log($"Total Boids: {_flockManager.Flock.Boids.Count}");
            UnityEngine.Debug.Log($"Active Boids: {_flockManager.Flock.ActiveCount}");

            // Check for target provider
            var targetProvider = _flockManager.GetComponent<ITargetProvider>();
            if (targetProvider == null)
            {
                UnityEngine.Debug.LogError("NO ITargetProvider found on FlockManager!");
            }
            else
            {
                UnityEngine.Debug.Log($"ITargetProvider found: {targetProvider.GetType().Name}");
            }

            // Log behaviors
            UnityEngine.Debug.Log($"Default Behaviours Count: {_flockManager.Flock.Settings.DefaultBehaviours.Count}");
            foreach (var behavior in _flockManager.Flock.Settings.DefaultBehaviours)
            {
                UnityEngine.Debug.Log($"  - {behavior.Name} (Weight: {behavior.Weight}, Enabled: {behavior.IsEnabled})");
            }

            UnityEngine.Debug.Log("==============================\n");
        }

        private void LogFlockState()
        {
            if (_flockManager == null || _flockManager.Flock == null) return;

            UnityEngine.Debug.Log($"\n=== FLOCK STATE at t={Time.time:F2} ===");

            var boids = _flockManager.Flock.Boids.Take(_maxBoidsToLog).ToList();

            foreach (var boid in boids)
            {
                if (!boid.IsActive) continue;
                LogBoidState(boid);
            }

            UnityEngine.Debug.Log("===================================\n");
        }

        private void LogBoidState(IBoid boid)
        {
            UnityEngine.Debug.Log($"\n--- Boid {boid.Id} ---");
            UnityEngine.Debug.Log($"Position: {boid.Position}");
            UnityEngine.Debug.Log($"Velocity: {boid.Velocity} (Magnitude: {boid.Velocity.Magnitude:F2})");
            UnityEngine.Debug.Log($"Forward: {boid.Forward}");

            // Get the state machine behaviour
            var stateMachineBehaviour = _flockManager.Flock.Settings.DefaultBehaviours
                .FirstOrDefault(b => b.Name == "StateMachine");

            if (stateMachineBehaviour == null)
            {
                UnityEngine.Debug.LogWarning($"Boid {boid.Id}: NO STATE MACHINE BEHAVIOUR FOUND!");
                return;
            }

            UnityEngine.Debug.Log($"State Machine Enabled: {stateMachineBehaviour.IsEnabled}");
            UnityEngine.Debug.Log($"State Machine Weight: {stateMachineBehaviour.Weight}");

            // Try to get current state through reflection (this is a hack for debugging)
            var smb = stateMachineBehaviour as FlockStateMachineBehaviour;
            if (smb != null)
            {
                // We need to access the private _machines dictionary
                var machinesField = typeof(FlockStateMachineBehaviour).GetField("_machines",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (machinesField != null)
                {
                    var machines = machinesField.GetValue(smb) as Dictionary<int, IStateMachine>;
                    if (machines != null && machines.TryGetValue(boid.Id, out var machine))
                    {
                        var currentState = machine.CurrentState;
                        if (currentState != null)
                        {
                            UnityEngine.Debug.Log($"CURRENT STATE: {currentState.Name}");
                            UnityEngine.Debug.Log($"  Behaviours in state:");
                            foreach (var b in currentState.Behaviours)
                            {
                                UnityEngine.Debug.Log($"    - {b.Name} (Weight: {b.Weight}, Enabled: {b.IsEnabled})");
                            }

                            // Track state changes
                            if (!_stateHistory.TryGetValue(boid.Id, out var history))
                            {
                                history = new StateDebugInfo();
                                _stateHistory[boid.Id] = history;
                            }

                            if (history.LastStateName != currentState.Name)
                            {
                                UnityEngine.Debug.LogWarning($">>> STATE CHANGED: {history.LastStateName ?? "NULL"} -> {currentState.Name} <<<");
                                history.LastStateName = currentState.Name;
                                history.StateChangeTime = Time.time;
                                history.StateChangeCount++;
                            }

                            UnityEngine.Debug.Log($"  Time in state: {Time.time - history.StateChangeTime:F2}s");
                            UnityEngine.Debug.Log($"  Total state changes: {history.StateChangeCount}");
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"Boid {boid.Id}: Machine exists but CurrentState is NULL!");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Boid {boid.Id}: No state machine instance found in dictionary!");
                    }
                }
            }

            // Log threat information
            LogThreatInfo(boid);

            // Log goal information
            LogGoalInfo(boid);

            UnityEngine.Debug.Log($"--- End Boid {boid.Id} ---\n");
        }

        private void LogThreatInfo(IBoid boid)
        {
            var targetProvider = _flockManager.GetComponent<ITargetProvider>();
            if (targetProvider == null)
            {
                UnityEngine.Debug.LogError("No ITargetProvider to get threats!");
                return;
            }

            var threats = targetProvider.GetThreats(boid);

            UnityEngine.Debug.Log($"THREATS:");
            if (threats == null || threats.Count == 0)
            {
                UnityEngine.Debug.LogWarning("  NO THREATS DETECTED!");
            }
            else
            {
                UnityEngine.Debug.Log($"  Threat count: {threats.Count}");
                for (int i = 0; i < threats.Count; i++)
                {
                    var threat = threats[i];
                    float distance = FVector3.Distance(boid.Position, threat);
                    UnityEngine.Debug.Log($"  Threat {i}: Position {threat}, Distance: {distance:F2}");

                    if (distance < 2f)
                        UnityEngine.Debug.LogWarning($"    >>> THREAT {i} IS WITHIN PANIC DISTANCE! <<<");
                    if (distance < 5f && distance >= 2f)
                        UnityEngine.Debug.Log($"    Threat {i} is nearby but outside panic distance");
                }
            }
        }

        private void LogGoalInfo(IBoid boid)
        {
            var targetProvider = _flockManager.GetComponent<ITargetProvider>();
            if (targetProvider == null) return;

            var seekTarget = targetProvider.GetSeekTarget(boid);

            UnityEngine.Debug.Log($"GOAL:");
            if (seekTarget == null)
            {
                UnityEngine.Debug.LogWarning("  NO SEEK TARGET!");
            }
            else
            {
                float distance = FVector3.Distance(boid.Position, seekTarget.Value);
                UnityEngine.Debug.Log($"  Position: {seekTarget.Value}");
                UnityEngine.Debug.Log($"  Distance: {distance:F2}");
            }
        }

        private void DrawDebugVisualization()
        {
            if (_flockManager == null || _flockManager.Flock == null) return;

            foreach (var boid in _flockManager.Flock.Boids)
            {
                if (!boid.IsActive) continue;

                var pos = boid.Position.ToUnityVector3();

                // Draw velocity
                UnityEngine.Debug.DrawRay(pos, boid.Velocity.ToUnityVector3(), Color.green);

                // Draw to goal
                if (_goalTransform != null)
                {
                    UnityEngine.Debug.DrawLine(pos, _goalTransform.position, Color.cyan);
                }

                // Draw to threats
                foreach (var enemy in _enemyTransforms)
                {
                    if (enemy != null)
                    {
                        float dist = Vector3.Distance(pos, enemy.position);
                        Color threatColor = dist < 2f ? Color.red : Color.yellow;
                        UnityEngine.Debug.DrawLine(pos, enemy.position, threatColor);
                    }
                }

                // Draw perception radius
                DrawCircle(pos, boid.Settings.PerceptionRadius, Color.blue);
            }

            // Draw panic radius around enemies
            foreach (var enemy in _enemyTransforms)
            {
                if (enemy != null)
                {
                    DrawCircle(enemy.position, 2f, Color.red, 32); // Assuming 10f panic distance
                    DrawCircle(enemy.position, 5f, Color.yellow, 32); // Assuming 15f safe distance
                }
            }
        }

        private void DrawCircle(Vector3 center, float radius, Color color, int segments = 24)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );
                UnityEngine.Debug.DrawLine(prevPoint, newPoint, color);
                prevPoint = newPoint;
            }
        }

        private class StateDebugInfo
        {
            public string LastStateName;
            public float StateChangeTime;
            public int StateChangeCount;
        }

        // Manual trigger for on-demand logging
        [ContextMenu("Log Current State")]
        public void ManualLog()
        {
            _enableLogging = true;
            LogFlockState();
        }

        [ContextMenu("Log Setup Info")]
        public void ManualLogSetup()
        {
            LogSetup();
        }
    }
}