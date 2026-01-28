using UnityEngine;
using UnityEditor;
using System.IO;

namespace PalaceOfFantasy.FlockForge.Editor.Windows
{
    public class FlockForgeWindow : EditorWindow
    {
        private enum Tab { Behavior, Transition, State, StateMachine, Profile, Event }
        private Tab _currentTab = Tab.Behavior;

        // Common
        private string _itemName = "NewItem";
        private string _namespace = "MyGame.AI";
        private string _savePath = "Assets/Scripts/AI";
        private Vector2 _scrollPos;
        private bool _showPreview = true;

        [MenuItem("FlockForge/Creator Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<FlockForgeWindow>("FlockForge Creator");
            window.minSize = new Vector2(450, 500);
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            
            _currentTab = (Tab)GUILayout.Toolbar((int)_currentTab, new string[] { 
                "Behavior", "Transition", "State", "StateMachine", "Profile", "Event" 
            });
            EditorGUILayout.Space(10);

            DrawCommonFields();
            EditorGUILayout.Space(10);
            
            // Force repaint when any value changes so preview updates
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }

            DrawTabSpecificHelp();
            EditorGUILayout.Space(10);

            _showPreview = EditorGUILayout.Foldout(_showPreview, "Code Preview", true);
            if (_showPreview)
            {
                DrawCodePreview();
            }

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Generate Code", GUILayout.Height(30)))
            {
                Generate();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("FlockForge Creator", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                EditorUtility.DisplayDialog("FlockForge Creator",
                    "Generate custom AI code for your flocking simulation.\n\n" +
                    "• Behavior: Custom steering behaviors\n" +
                    "• Transition: State change conditions\n" +
                    "• State: AI state with behaviors\n" +
                    "• StateMachine: Full state machine behavior\n" +
                    "• Profile: Boid settings (speed, force, etc.)\n" +
                    "• Event: Custom flock events",
                    "OK");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCommonFields()
        {
            _itemName = EditorGUILayout.TextField("Item Name", _itemName);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            
            EditorGUILayout.BeginHorizontal();
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        _savePath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Path", "Please select a folder inside the Assets directory.", "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabSpecificHelp()
        {
            string helpText = _currentTab switch
            {
                Tab.Behavior => "Creates a custom steering behavior. Implement CalculateForce() to return a steering force (FVector3).",
                Tab.Transition => "Creates a state transition condition. Implement Evaluate() to return true when the transition should trigger.",
                Tab.State => "Creates a custom state asset. States hold behaviors and transitions. Configure in the Unity Inspector.",
                Tab.StateMachine => "Creates a state machine behavior. Can be used as a behavior itself, containing multiple states.",
                Tab.Profile => "Creates a custom BoidProfile subclass. Add your own settings fields for specialized boid types.",
                Tab.Event => "Creates a custom flock event struct. Use with IEventManager for boid-to-boid or boid-to-world communication.",
                _ => ""
            };
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
        }

        private void DrawCodePreview()
        {
            string preview = GetGeneratedCode();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
            EditorGUILayout.TextArea(preview, EditorStyles.textArea, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private string GetGeneratedCode()
        {
            return _currentTab switch
            {
                Tab.Behavior => GetBehaviorCode(),
                Tab.Transition => GetTransitionCode(),
                Tab.State => GetStateCode(),
                Tab.StateMachine => GetStateMachineCode(),
                Tab.Profile => GetProfileCode(),
                Tab.Event => GetEventCode(),
                _ => ""
            };
        }

        private void Generate()
        {
            if (string.IsNullOrEmpty(_itemName))
            {
                EditorUtility.DisplayDialog("Error", "Item Name cannot be empty.", "OK");
                return;
            }

            switch (_currentTab)
            {
                case Tab.Behavior:
                    GenerateBehavior();
                    break;
                case Tab.Transition:
                    GenerateTransition();
                    break;
                case Tab.State:
                    GenerateState();
                    break;
                case Tab.StateMachine:
                    GenerateStateMachine();
                    break;
                case Tab.Profile:
                    GenerateProfile();
                    break;
                case Tab.Event:
                    GenerateEvent();
                    break;
            }

            AssetDatabase.Refresh();
        }

        #region Code Generation

        private string GetBehaviorCode()
        {
            string behaviourName = _itemName + "Behaviour";
            string assetName = _itemName + "Asset";
            return $@"// ===== {behaviourName}.cs =====
using PalaceOfFantasy.FlockForge.Core;

namespace {_namespace}
{{
    public class {behaviourName} : IBehaviour
    {{
        public string Name => ""{_itemName}"";
        public bool IsEnabled {{ get; set; }} = true;
        public float Weight {{ get; set; }} = 1f;

        // Add your custom fields here
        public float MyParameter = 1f;

        public FVector3 CalculateForce(IBoidContext context)
        {{
            // TODO: Implement your custom steering logic
            // Access context.Position, context.Velocity, context.Neighbors, etc.
            // Return a steering FORCE (not velocity), e.g.:
            // return (targetPos - context.Position).Normalized * context.Settings.MaxForce;
            
            return FVector3.Zero;
        }}
    }}
}}

// ===== {assetName}.cs =====
using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace {_namespace}
{{
    [CreateAssetMenu(menuName = ""FlockForge/Behaviors/{_itemName}"")]
    public class {assetName} : BehaviourAsset
    {{
        [Header(""{_itemName} Settings"")]
        public float myParameter = 1f;

        public override IBehaviour CreateBehaviour()
            => new {behaviourName} 
            {{ 
                Weight = _weight, 
                IsEnabled = _isEnabled,
                MyParameter = myParameter
            }};
    }}
}}";
        }

        private string GetTransitionCode()
        {
            string assetName = _itemName + "TransitionAsset";
            return $@"using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config.AI;

namespace {_namespace}
{{
    [CreateAssetMenu(menuName = ""FlockForge/Transitions/{_itemName}"")]
    public class {assetName} : TransitionAsset
    {{
        [Header(""{_itemName} Settings"")]
        [SerializeField] private float _threshold = 5f;

        public override bool Evaluate(IBoid boid, IBoidContext context)
        {{
            // TODO: Implement your transition condition
            // Return true to trigger the transition
            // Examples:
            // - Distance check: FVector3.Distance(boid.Position, target) < _threshold
            // - Health check: boid has custom health component
            // - Timer: time since state entered > _threshold
            
            return false;
        }}
    }}
}}";
        }

        private string GetStateCode()
        {
            string assetName = _itemName + "StateAsset";
            return $@"using UnityEngine;
using System.Collections.Generic;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Config.AI;

namespace {_namespace}
{{
    /// <summary>
    /// Custom state asset for {_itemName}.
    /// Add custom logic in OnEnter/OnExit if needed.
    /// </summary>
    [CreateAssetMenu(menuName = ""FlockForge/AI/States/{_itemName}"")]
    public class {assetName} : StateAsset
    {{
        [Header(""{_itemName} Custom Settings"")]
        [SerializeField] private float _customValue = 1f;

        // Override CreateState if you need custom state logic
        // public override IState CreateState() {{ ... }}
    }}
}}";
        }

        private string GetStateMachineCode()
        {
            string behaviourName = _itemName + "StateMachineBehaviour";
            string assetName = _itemName + "StateMachineAsset";
            return $@"using UnityEngine;
using System.Collections.Generic;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Config.AI;

namespace {_namespace}
{{
    /// <summary>
    /// Custom state machine for {_itemName}.
    /// Extend StateMachineAsset for custom initialization logic.
    /// </summary>
    [CreateAssetMenu(menuName = ""FlockForge/AI/StateMachines/{_itemName}"")]
    public class {assetName} : StateMachineAsset
    {{
        [Header(""{_itemName} Custom Settings"")]
        [SerializeField] private bool _debugMode = false;

        // Override CreateBehaviour if you need custom state machine logic
        // public override IBehaviour CreateBehaviour() {{ ... }}
    }}
}}";
        }

        private string GetProfileCode()
        {
            string profileName = _itemName + "Profile";
            return $@"using UnityEngine;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace {_namespace}
{{
    /// <summary>
    /// Custom boid profile for {_itemName}.
    /// Add specialized settings for this boid type.
    /// </summary>
    [CreateAssetMenu(menuName = ""FlockForge/Profiles/{_itemName}"")]
    public class {profileName} : BoidProfile
    {{
        [Header(""{_itemName} Specific Settings"")]
        [SerializeField] private float _customSpeed = 10f;
        [SerializeField] private float _customForce = 20f;
        [SerializeField] private bool _canFly = false;

        // Expose custom settings as properties
        public float CustomSpeed => _customSpeed;
        public float CustomForce => _customForce;
        public bool CanFly => _canFly;

        // Override base properties if needed
        // public override float MaxSpeed => _customSpeed;
    }}
}}";
        }

        private string GetEventCode()
        {
            string eventName = _itemName + "Event";
            return $@"using PalaceOfFantasy.FlockForge.Core;

namespace {_namespace}
{{
    /// <summary>
    /// Custom flock event for {_itemName}.
    /// Fire with: eventManager.Fire(new {eventName} {{ ... }});
    /// Subscribe with: eventManager.Subscribe<{eventName}>(OnEvent);
    /// </summary>
    public struct {eventName} : IFlockEvent
    {{
        // Add your event data here
        public int SenderId;
        public float Value;
        
        // Example: public FVector3 Position;
    }}
}}";
        }

        #endregion

        #region File Writers

        private void GenerateBehavior()
        {
            string behaviourName = _itemName + "Behaviour";
            string assetName = _itemName + "Asset";

            string behaviourCode = $@"using PalaceOfFantasy.FlockForge.Core;

namespace {_namespace}
{{
    public class {behaviourName} : IBehaviour
    {{
        public string Name => ""{_itemName}"";
        public bool IsEnabled {{ get; set; }} = true;
        public float Weight {{ get; set; }} = 1f;

        public float MyParameter = 1f;

        public FVector3 CalculateForce(IBoidContext context)
        {{
            // TODO: Implement your custom steering logic
            return FVector3.Zero;
        }}
    }}
}}";
            
            string assetCode = $@"using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace {_namespace}
{{
    [CreateAssetMenu(menuName = ""FlockForge/Behaviors/{_itemName}"")]
    public class {assetName} : BehaviourAsset
    {{
        [Header(""{_itemName} Settings"")]
        public float myParameter = 1f;

        public override IBehaviour CreateBehaviour()
            => new {behaviourName} 
            {{ 
                Weight = _weight, 
                IsEnabled = _isEnabled,
                MyParameter = myParameter
            }};
    }}
}}";

            WriteFile(behaviourName, behaviourCode);
            WriteFile(assetName, assetCode);
            Debug.Log($"Generated Behavior: {behaviourName} and {assetName}");
        }

        private void GenerateTransition()
        {
            string assetName = _itemName + "TransitionAsset";
            string code = GetTransitionCode();
            WriteFile(assetName, code);
            Debug.Log($"Generated Transition: {assetName}");
        }

        private void GenerateState()
        {
            string assetName = _itemName + "StateAsset";
            string code = GetStateCode();
            WriteFile(assetName, code);
            Debug.Log($"Generated State: {assetName}");
        }

        private void GenerateStateMachine()
        {
            string assetName = _itemName + "StateMachineAsset";
            string code = GetStateMachineCode();
            WriteFile(assetName, code);
            Debug.Log($"Generated StateMachine: {assetName}");
        }

        private void GenerateProfile()
        {
            string profileName = _itemName + "Profile";
            string code = GetProfileCode();
            WriteFile(profileName, code);
            Debug.Log($"Generated Profile: {profileName}");
        }

        private void GenerateEvent()
        {
            string eventName = _itemName + "Event";
            string code = GetEventCode();
            WriteFile(eventName, code);
            Debug.Log($"Generated Event: {eventName}");
        }

        private void WriteFile(string fileName, string content)
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            string fullPath = Path.Combine(_savePath, fileName + ".cs");
            File.WriteAllText(fullPath, content);
        }

        #endregion
    }
}
