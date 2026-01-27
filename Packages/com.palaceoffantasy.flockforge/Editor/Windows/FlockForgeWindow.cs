using UnityEngine;
using UnityEditor;
using System.IO;

namespace PalaceOfFantasy.FlockForge.Editor.Windows
{
    public class FlockForgeWindow : EditorWindow
    {
        private enum Tab { Behavior, Transition, Event }
        private Tab _currentTab = Tab.Behavior;

        // Common
        private string _itemName = "NewItem";
        private string _namespace = "MyGame.AI";
        private string _savePath = "Assets/Scripts/AI";

        [MenuItem("FlockForge/Creator Tool")]
        public static void ShowWindow()
        {
            GetWindow<FlockForgeWindow>("FlockForge Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("FlockForge Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _currentTab = (Tab)GUILayout.Toolbar((int)_currentTab, new string[] { "Behavior", "Transition", "Event" });
            EditorGUILayout.Space();

            _itemName = EditorGUILayout.TextField("Item Name", _itemName);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            
            EditorGUILayout.BeginHorizontal();
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert absolute path to relative Asset path
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

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
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
                case Tab.Event:
                    GenerateEvent();
                    break;
            }

            AssetDatabase.Refresh();
        }

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

        public FVector3 CalculateForce(IBoidContext context)
        {{
            // TODO: Implement your custom steering logic here
            // Note: Use FVector3.Zero, FMath, etc. Avoid UnityEngine types here.
            return FVector3.Zero;
        }}
    }}
}}";
            
            // Note: Asset needs reference to the behavior we just defined in namespace
            string assetCode = $@"using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;
using {_namespace};

namespace {_namespace}.Assets
{{
    [CreateAssetMenu(menuName = ""FlockForge/Behaviors/{_itemName}"")]
    public class {assetName} : BehaviourAsset
    {{
        public float weight = 1f;

        public override IBehaviour CreateBehaviour()
            => new {behaviourName} {{ Weight = weight, IsEnabled = _isEnabled }};
    }}
}}";

            WriteFile(behaviourName, behaviourCode);
            WriteFile(assetName, assetCode);
            Debug.Log($"Generated Behavior: {behaviourName} and {assetName}");
        }

        private void GenerateTransition()
        {
            string assetName = _itemName + "TransitionAsset";

            string code = $@"using UnityEngine;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Config;

namespace {_namespace}
{{
    [CreateAssetMenu(menuName = ""FlockForge/Transitions/{_itemName}"")]
    public class {assetName} : TransitionAsset
    {{
        public override bool Check(IBoidContext context)
        {{
            // TODO: Implement your transition condition
            // Return true to trigger the transition
            return false;
        }}
    }}
}}";
            WriteFile(assetName, code);
            Debug.Log($"Generated Transition: {assetName}");
        }

        private void GenerateEvent()
        {
            string structName = _itemName + "Event";

            string code = $@"using PalaceOfFantasy.FlockForge.Core;

namespace {_namespace}
{{
    public struct {structName} : IFlockEvent
    {{
        // Add your event data here
        // public int Value;
    }}
}}";
            WriteFile(structName, code);
            Debug.Log($"Generated Event: {structName}");
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
    }
}
