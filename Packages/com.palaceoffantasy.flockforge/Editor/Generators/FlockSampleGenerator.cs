using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Components;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Behaviors;
using PalaceOfFantasy.FlockForge.Unity.Providers;

namespace PalaceOfFantasy.FlockForge.Editor.Generators
{
    public class FlockSampleGenerator
    {
        [MenuItem("FlockForge/Samples/Create Basic Scene")]
        public static void CreateBasicScene()
        {
            // 1. Setup Folders
            string sampleFolder = "Assets/FlockForge_Sample";
            if (!AssetDatabase.IsValidFolder(sampleFolder))
            {
                AssetDatabase.CreateFolder("Assets", "FlockForge_Sample");
            }
            string behaviorFolder = sampleFolder + "/Behaviors";
            if (!AssetDatabase.IsValidFolder(behaviorFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Behaviors");
            }

            // 2. Create Assets
            var profile = CreateAsset<BoidProfile>(sampleFolder, "SampleBoidProfile");
            SetProperty(profile, "_movementPlane", (int)MovementPlane.XZ);
            SetProperty(profile, "_maxSpeed", 6f);
            SetProperty(profile, "_maxForce", 15f);

            // Create Behaviors
            var wander = CreateAsset<WanderAsset>(behaviorFolder, "SampleWander");
            SetProperties(wander, new Dictionary<string, object> {
                { "_weight", 0.5f }, { "jitter", 8f }, { "radius", 3f }, { "distance", 5f }
            });

            var separation = CreateAsset<SeparationAsset>(behaviorFolder, "SampleSeparation");
            SetProperties(separation, new Dictionary<string, object> {
                { "_weight", 2.0f }, { "radius", 2.5f }
            });

            var alignment = CreateAsset<AlignmentAsset>(behaviorFolder, "SampleAlignment");
            SetProperty(alignment, "_weight", 1.0f);

            var cohesion = CreateAsset<CohesionAsset>(behaviorFolder, "SampleCohesion");
            SetProperty(cohesion, "_weight", 1.0f);

            var seek = CreateAsset<SeekAsset>(behaviorFolder, "SampleSeek");
            SetProperty(seek, "_weight", 5.0f);

            var stay = CreateAsset<StayInRadiusAsset>(behaviorFolder, "SampleStayInRadius");
            SetProperties(stay, new Dictionary<string, object> {
                { "_weight", 1.0f }, { "radius", 25f }
            });
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3. Create Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 4. Create Ground
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.localScale = new Vector3(5, 1, 5);
            plane.transform.position = Vector3.zero;

            // 5. Create Target
            var target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target.name = "Target";
            target.transform.position = new Vector3(10, 0.5f, 10);
            var targetMat = new Material(Shader.Find("Standard"));
            targetMat.color = Color.green;
            target.GetComponent<Renderer>().material = targetMat;

            // 6. Create Boid Prefab
            var boidGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boidGO.name = "Boid";
            boidGO.transform.rotation = Quaternion.Euler(90, 0, 0);
            boidGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            var agent = boidGO.AddComponent<BoidAgent>();
            SetProperty(agent, "_profile", profile);

            // Save as Prefab
            string prefabPath = sampleFolder + "/SampleBoid.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(boidGO, prefabPath);
            GameObject.DestroyImmediate(boidGO);

            // 7. Setup Manager
            var managerGO = new GameObject("FlockManager");
            var manager = managerGO.AddComponent<FlockManager>();
            var provider = managerGO.AddComponent<TransformTargetProvider>();

            // Setup Provider
            SetProperty(provider, "_seekTarget", target.transform);

            // Setup Manager Fields
            var soManager = new SerializedObject(manager);
            soManager.Update();
            soManager.FindProperty("_boidPrefab").objectReferenceValue = prefab.GetComponent<BoidAgent>();
            soManager.FindProperty("_spawnCount").intValue = 60;
            soManager.FindProperty("_spawnRadius").floatValue = 15f;
            
            // Assign Behaviors List
            var behaviorListInfo = soManager.FindProperty("_defaultBehaviours");
            if (behaviorListInfo != null)
            {
                behaviorListInfo.ClearArray();
                behaviorListInfo.arraySize = 6;
                behaviorListInfo.GetArrayElementAtIndex(0).objectReferenceValue = separation;
                behaviorListInfo.GetArrayElementAtIndex(1).objectReferenceValue = alignment;
                behaviorListInfo.GetArrayElementAtIndex(2).objectReferenceValue = cohesion;
                behaviorListInfo.GetArrayElementAtIndex(3).objectReferenceValue = wander;
                behaviorListInfo.GetArrayElementAtIndex(4).objectReferenceValue = seek;
                behaviorListInfo.GetArrayElementAtIndex(5).objectReferenceValue = stay;
            }
            else
            {
                Debug.LogError("Could not find _defaultBehaviours property on FlockManager!");
            }
            
            soManager.ApplyModifiedProperties();

            // 8. Ensure Persistence
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(provider);

            // Position Camera
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 40, -40);
                cam.transform.rotation = Quaternion.Euler(45, 0, 0);
            }

            // 9. Save Scene
            string scenePath = sampleFolder + "/BasicFlockSample.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.ImportAsset(scenePath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Created Basic Flock Sample Scene! Saved at: " + scenePath);
        }

        private static void SetProperty(Object target, string propertyName, object value)
        {
            var so = new SerializedObject(target);
            so.Update();
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                if (value is int i) prop.intValue = i;
                else if (value is float f) prop.floatValue = f;
                else if (value is bool b) prop.boolValue = b;
                else if (value is Object o) prop.objectReferenceValue = o;
                else if (value is string s) prop.stringValue = s;
                else Debug.LogWarning($"Unknown type for {propertyName}");
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"Property {propertyName} not found on {target.name}");
            }
        }

        private static void SetProperties(Object target, Dictionary<string, object> values)
        {
            var so = new SerializedObject(target);
            so.Update();
            foreach (var kvp in values)
            {
                var prop = so.FindProperty(kvp.Key);
                if (prop != null)
                {
                    var value = kvp.Value;
                    if (value is int i) prop.intValue = i;
                    else if (value is float f) prop.floatValue = f;
                    else if (value is bool b) prop.boolValue = b;
                    else if (value is Object o) prop.objectReferenceValue = o;
                    else if (value is string s) prop.stringValue = s;
                    else Debug.LogWarning($"Unknown type for {kvp.Key}");
                }
                else
                {
                    Debug.LogWarning($"Property {kvp.Key} not found on {target.name}");
                }
            }
            so.ApplyModifiedProperties();
        }

        private static T CreateAsset<T>(string folder, string name) where T : ScriptableObject
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }
    }
}
