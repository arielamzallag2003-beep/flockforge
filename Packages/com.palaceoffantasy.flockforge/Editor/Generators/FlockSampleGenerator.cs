using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Components;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Config.AI;
using PalaceOfFantasy.FlockForge.Unity.Config.AI.Transitions;
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
            var profile = CreateAsset<BoidProfile>(sampleFolder, "PreyProfile");
            SetProperty(profile, "_movementPlane", (int)MovementPlane.XZ);
            SetProperty(profile, "_maxSpeed", 6f);
            SetProperty(profile, "_maxForce", 15f);

            // Create Behaviors
            var wander = CreateAsset<WanderAsset>(behaviorFolder, "PreyWander");
            SetProperties(wander, new Dictionary<string, object> {
                { "_weight", 0.5f }, { "jitter", 8f }, { "radius", 3f }, { "distance", 5f }
            });

            var separation = CreateAsset<SeparationAsset>(behaviorFolder, "PreySeparation");
            SetProperties(separation, new Dictionary<string, object> {
                { "_weight", 2.0f }, { "radius", 2.5f }
            });

            var alignment = CreateAsset<AlignmentAsset>(behaviorFolder, "PreyAlignment");
            SetProperty(alignment, "_weight", 1.0f);

            var cohesion = CreateAsset<CohesionAsset>(behaviorFolder, "PreyCohesion");
            SetProperty(cohesion, "_weight", 1.0f);

            var seek = CreateAsset<SeekAsset>(behaviorFolder, "PreySeek");
            SetProperty(seek, "_weight", 2.0f);  // Moderate attraction to food

            var stay = CreateAsset<StayInRadiusAsset>(behaviorFolder, "PreyStayInRadius");
            SetProperties(stay, new Dictionary<string, object> {
                { "_weight", 1.0f }, { "radius", 25f }
            });

            var flee = CreateAsset<FleeAsset>(behaviorFolder, "PreyFlee");
            SetProperty(flee, "_weight", 3.0f);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 2b. Create States Folder
            string statesFolder = sampleFolder + "/States";
            if (!AssetDatabase.IsValidFolder(statesFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "States");
            }

            // 2c. Create Transitions
            string transitionsFolder = sampleFolder + "/Transitions";
            if (!AssetDatabase.IsValidFolder(transitionsFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Transitions");
            }

            var threatNearby = CreateAsset<ThreatNearbyTransitionAsset>(transitionsFolder, "ThreatNearby");
            SetProperty(threatNearby, "_panicDistance", 10f);

            var threatGone = CreateAsset<ThreatGoneTransitionAsset>(transitionsFolder, "ThreatGone");
            SetProperty(threatGone, "_safeDistance", 15f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 2d. Create States
            var forageState = CreateAsset<StateAsset>(statesFolder, "ForageState");
            var fleeState = CreateAsset<StateAsset>(statesFolder, "FleeState");

            // Configure ForageState behaviors: Wander, Separation, Alignment, Cohesion, StayInRadius, Seek
            var soForage = new SerializedObject(forageState);
            soForage.Update();
            var forageBehaviors = soForage.FindProperty("_behaviours");
            forageBehaviors.ClearArray();
            forageBehaviors.arraySize = 6;
            forageBehaviors.GetArrayElementAtIndex(0).objectReferenceValue = seek;       // Seek food target
            forageBehaviors.GetArrayElementAtIndex(1).objectReferenceValue = wander;
            forageBehaviors.GetArrayElementAtIndex(2).objectReferenceValue = separation;
            forageBehaviors.GetArrayElementAtIndex(3).objectReferenceValue = alignment;
            forageBehaviors.GetArrayElementAtIndex(4).objectReferenceValue = cohesion;
            forageBehaviors.GetArrayElementAtIndex(5).objectReferenceValue = stay;
            
            // Add transition: Forage -> Flee when ThreatNearby
            var forageTransitions = soForage.FindProperty("_transitions");
            forageTransitions.ClearArray();
            forageTransitions.arraySize = 1;
            var t0 = forageTransitions.GetArrayElementAtIndex(0);
            t0.FindPropertyRelative("Condition").objectReferenceValue = threatNearby;
            t0.FindPropertyRelative("TargetState").objectReferenceValue = fleeState;
            t0.FindPropertyRelative("Priority").intValue = 10;
            soForage.ApplyModifiedProperties();

            // Configure FleeState behaviors: Flee, Separation
            var soFlee = new SerializedObject(fleeState);
            soFlee.Update();
            var fleeBehaviors = soFlee.FindProperty("_behaviours");
            fleeBehaviors.ClearArray();
            fleeBehaviors.arraySize = 2;
            fleeBehaviors.GetArrayElementAtIndex(0).objectReferenceValue = flee;
            fleeBehaviors.GetArrayElementAtIndex(1).objectReferenceValue = separation;

            // Add transition: Flee -> Forage when ThreatGone
            var fleeTransitions = soFlee.FindProperty("_transitions");
            fleeTransitions.ClearArray();
            fleeTransitions.arraySize = 1;
            var t1 = fleeTransitions.GetArrayElementAtIndex(0); 
            t1.FindPropertyRelative("Condition").objectReferenceValue = threatGone;
            t1.FindPropertyRelative("TargetState").objectReferenceValue = forageState;
            t1.FindPropertyRelative("Priority").intValue = 5;
            soFlee.ApplyModifiedProperties();

            EditorUtility.SetDirty(forageState);
            EditorUtility.SetDirty(fleeState);

            // 2e. Create Prey State Machine
            var preyStateMachine = CreateAsset<StateMachineAsset>(sampleFolder, "PreyStateMachine");
            var soSM = new SerializedObject(preyStateMachine);
            soSM.Update();
            soSM.FindProperty("_initialState").objectReferenceValue = forageState;
            var allStates = soSM.FindProperty("_allStates");
            allStates.ClearArray();
            allStates.arraySize = 2;
            allStates.GetArrayElementAtIndex(0).objectReferenceValue = forageState;
            allStates.GetArrayElementAtIndex(1).objectReferenceValue = fleeState;
            soSM.FindProperty("_weight").floatValue = 1.0f;
            soSM.ApplyModifiedProperties();
            EditorUtility.SetDirty(preyStateMachine);

            // ============ PREDATOR ASSETS ============
            // 2f. Create Predator Profile (faster, more aggressive)
            var predatorProfile = CreateAsset<BoidProfile>(sampleFolder, "PredatorProfile");
            SetProperty(predatorProfile, "_movementPlane", (int)MovementPlane.XZ);
            SetProperty(predatorProfile, "_maxSpeed", 8f);  // Faster than prey
            SetProperty(predatorProfile, "_maxForce", 20f);

            // 2g. Create Predator-specific behaviors
            var predatorSeek = CreateAsset<SeekAsset>(behaviorFolder, "PredatorSeek");
            SetProperty(predatorSeek, "_weight", 4.0f);

            var predatorSeparation = CreateAsset<SeparationAsset>(behaviorFolder, "PredatorSeparation");
            SetProperties(predatorSeparation, new Dictionary<string, object> {
                { "_weight", 1.5f }, { "radius", 3f }
            });

            var predatorAlignment = CreateAsset<AlignmentAsset>(behaviorFolder, "PredatorAlignment");
            SetProperty(predatorAlignment, "_weight", 0.8f);

            var predatorCohesion = CreateAsset<CohesionAsset>(behaviorFolder, "PredatorCohesion");
            SetProperty(predatorCohesion, "_weight", 0.5f);

            var predatorWander = CreateAsset<WanderAsset>(behaviorFolder, "PredatorWander");
            SetProperties(predatorWander, new Dictionary<string, object> {
                { "_weight", 0.3f }, { "jitter", 5f }, { "radius", 2f }, { "distance", 4f }
            });

            var predatorStay = CreateAsset<StayInRadiusAsset>(behaviorFolder, "PredatorStayInRadius");
            SetProperties(predatorStay, new Dictionary<string, object> {
                { "_weight", 0.8f }, { "radius", 30f }
            });

            // 2h. Create Hunt State (Seek + Wander + pack flocking)
            var huntState = CreateAsset<StateAsset>(statesFolder, "HuntState");
            var soHunt = new SerializedObject(huntState);
            soHunt.Update();
            var huntBehaviors = soHunt.FindProperty("_behaviours");
            huntBehaviors.ClearArray();
            huntBehaviors.arraySize = 6;
            huntBehaviors.GetArrayElementAtIndex(0).objectReferenceValue = predatorSeek;
            huntBehaviors.GetArrayElementAtIndex(1).objectReferenceValue = predatorWander;  // Keep moving!
            huntBehaviors.GetArrayElementAtIndex(2).objectReferenceValue = predatorSeparation;
            huntBehaviors.GetArrayElementAtIndex(3).objectReferenceValue = predatorAlignment;
            huntBehaviors.GetArrayElementAtIndex(4).objectReferenceValue = predatorCohesion;
            huntBehaviors.GetArrayElementAtIndex(5).objectReferenceValue = predatorStay;
            // No transitions for now - predators always hunt
            soHunt.ApplyModifiedProperties();
            EditorUtility.SetDirty(huntState);

            // 2i. Create Predator State Machine
            var predatorStateMachine = CreateAsset<StateMachineAsset>(sampleFolder, "PredatorStateMachine");
            var soPSM = new SerializedObject(predatorStateMachine);
            soPSM.Update();
            soPSM.FindProperty("_initialState").objectReferenceValue = huntState;
            var predatorStates = soPSM.FindProperty("_allStates");
            predatorStates.ClearArray();
            predatorStates.arraySize = 1;
            predatorStates.GetArrayElementAtIndex(0).objectReferenceValue = huntState;
            soPSM.FindProperty("_weight").floatValue = 1.0f;
            soPSM.ApplyModifiedProperties();
            EditorUtility.SetDirty(predatorStateMachine);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3. Create Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 4. Setup Lighting
            var mainLight = GameObject.Find("Directional Light");
            if (mainLight != null)
            {
                mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                var light = mainLight.GetComponent<Light>();
                light.color = new Color(1f, 0.95f, 0.85f); // Warm sunlight
                light.intensity = 1.2f;
            }

            // Add ambient fill light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.25f, 0.35f); // Cool sky ambient

            // 5. Create Target (glowing green beacon)
            var target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target.name = "Target";
            target.transform.position = new Vector3(10, 2f, 10);
            target.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            var targetMat = CreateColorMaterial(new Color(0.2f, 1f, 0.4f), new Color(0.1f, 0.5f, 0.2f));
            target.GetComponent<Renderer>().material = targetMat;
            // Remove collider from target
            Object.DestroyImmediate(target.GetComponent<Collider>());

            // Add decorative boundary markers
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2 / 8;
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = $"Boundary_{i}";
                marker.transform.position = new Vector3(Mathf.Cos(angle) * 25, 0.5f, Mathf.Sin(angle) * 25);
                marker.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
                var markerMat = CreateColorMaterial(new Color(0.3f, 0.3f, 0.4f));
                marker.GetComponent<Renderer>().material = markerMat;
                Object.DestroyImmediate(marker.GetComponent<Collider>());
            }

            // 6. Create Materials Folder
            string materialsFolder = sampleFolder + "/Materials";
            if (!AssetDatabase.IsValidFolder(materialsFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Materials");
            }

            // Create and save PreyMaterial as asset
            var preyMat = CreateColorMaterial(new Color(0.2f, 0.6f, 1f), new Color(0.05f, 0.15f, 0.3f));
            AssetDatabase.CreateAsset(preyMat, materialsFolder + "/PreyMaterial.mat");

            // 7. Create Prey Boid Prefab
            var boidGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boidGO.name = "PreyBoid";
            boidGO.transform.rotation = Quaternion.Euler(90, 0, 0);
            boidGO.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f);
            boidGO.GetComponent<Renderer>().sharedMaterial = preyMat;
            Object.DestroyImmediate(boidGO.GetComponent<Collider>());
            boidGO.AddComponent<BoidAgent>();

            // Save as Prefab first (without profile)
            string prefabPath = sampleFolder + "/PreyBoid.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(boidGO, prefabPath);
            GameObject.DestroyImmediate(boidGO);

            // Now load the prefab asset and assign the profile to it
            var prefabAgent = prefab.GetComponent<BoidAgent>();
            if (prefabAgent != null)
            {
                SetProperty(prefabAgent, "_profile", profile);
                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssetIfDirty(prefab);
            }
            else
            {
                Debug.LogError("BoidAgent not found on saved prefab!");
            }

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
            
            // Assign Behaviors List - Use Prey StateMachine
            var behaviorListInfo = soManager.FindProperty("_defaultBehaviours");
            if (behaviorListInfo != null)
            {
                behaviorListInfo.ClearArray();
                behaviorListInfo.arraySize = 1;
                behaviorListInfo.GetArrayElementAtIndex(0).objectReferenceValue = preyStateMachine;
            }
            else
            {
                Debug.LogError("Could not find _defaultBehaviours property on FlockManager!");
            }
            
            soManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            // ============ PREDATOR FLOCK ============
            // Create and save PredatorMaterial as asset
            var predatorMat = CreateColorMaterial(new Color(1f, 0.2f, 0.2f), new Color(0.5f, 0.05f, 0.05f));
            AssetDatabase.CreateAsset(predatorMat, materialsFolder + "/PredatorMaterial.mat");

            // 9. Create Predator Boid Prefab (red, menacing)
            var predatorBoidGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            predatorBoidGO.name = "PredatorBoid";
            predatorBoidGO.transform.rotation = Quaternion.Euler(90, 0, 0);
            predatorBoidGO.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f); // Larger than prey
            predatorBoidGO.GetComponent<Renderer>().sharedMaterial = predatorMat;
            Object.DestroyImmediate(predatorBoidGO.GetComponent<Collider>());
            predatorBoidGO.AddComponent<BoidAgent>();

            // Save Predator Prefab
            string predatorPrefabPath = sampleFolder + "/PredatorBoid.prefab";
            var predatorPrefab = PrefabUtility.SaveAsPrefabAsset(predatorBoidGO, predatorPrefabPath);
            GameObject.DestroyImmediate(predatorBoidGO);

            // Assign predator profile to prefab
            var predatorPrefabAgent = predatorPrefab.GetComponent<BoidAgent>();
            if (predatorPrefabAgent != null)
            {
                SetProperty(predatorPrefabAgent, "_profile", predatorProfile);
                EditorUtility.SetDirty(predatorPrefab);
                AssetDatabase.SaveAssetIfDirty(predatorPrefab);
            }

            // 9. Create Predator FlockManager
            var predatorManagerGO = new GameObject("PredatorFlockManager");
            predatorManagerGO.transform.position = new Vector3(-20, 0, 0);
            var predatorManager = predatorManagerGO.AddComponent<FlockManager>();
            var predatorProvider = predatorManagerGO.AddComponent<TransformTargetProvider>();

            // Predators seek the prey flock center (use the prey manager as target)
            SetProperty(predatorProvider, "_seekTarget", managerGO.transform);

            // Setup Predator Manager Fields
            var soPredManager = new SerializedObject(predatorManager);
            soPredManager.Update();
            soPredManager.FindProperty("_boidPrefab").objectReferenceValue = predatorPrefab.GetComponent<BoidAgent>();
            soPredManager.FindProperty("_spawnCount").intValue = 5; // Small pack of predators
            soPredManager.FindProperty("_spawnRadius").floatValue = 8f;

            var predBehaviorList = soPredManager.FindProperty("_defaultBehaviours");
            if (predBehaviorList != null)
            {
                predBehaviorList.ClearArray();
                predBehaviorList.arraySize = 1;
                predBehaviorList.GetArrayElementAtIndex(0).objectReferenceValue = predatorStateMachine;
            }
            soPredManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(predatorManager);

            // 10. Wire Predators as Threats for Prey
            // We need to use the PredatorFlockManager's transform as the threat source
            var soProvider2 = new SerializedObject(provider);
            soProvider2.Update();
            var threatsList = soProvider2.FindProperty("_threats");
            if (threatsList != null)
            {
                threatsList.ClearArray();
                threatsList.arraySize = 1;
                threatsList.GetArrayElementAtIndex(0).objectReferenceValue = predatorManagerGO.transform;
            }
            soProvider2.ApplyModifiedProperties();
            EditorUtility.SetDirty(provider);

            // 11. Position Camera for cinematic view
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 35, -45);
                cam.transform.rotation = Quaternion.Euler(40, 0, 0);
                cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
                cam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 12. Save Scene
            string scenePath = sampleFolder + "/PreyPredatorSample.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.ImportAsset(scenePath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Created Prey/Predator Ecosystem Scene! Saved at: " + scenePath);
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

        /// <summary>
        /// Gets a lit shader that works across Built-in, URP, and HDRP render pipelines.
        /// </summary>
        private static Shader GetLitShader()
        {
            // Try URP Lit first
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null) return shader;

            // Try HDRP Lit
            shader = Shader.Find("HDRP/Lit");
            if (shader != null) return shader;

            // Try Built-in Standard
            shader = Shader.Find("Standard");
            if (shader != null) return shader;

            // Last resort: Unlit
            shader = Shader.Find("Unlit/Color");
            if (shader != null) return shader;

            Debug.LogError("Could not find any lit shader!");
            return null;
        }

        /// <summary>
        /// Creates a material with a color that works across render pipelines.
        /// </summary>
        private static Material CreateColorMaterial(Color baseColor, Color? emissionColor = null)
        {
            var shader = GetLitShader();
            var mat = new Material(shader);
            
            // Set base color - different property names for different pipelines
            mat.SetColor("_BaseColor", baseColor);  // URP/HDRP
            mat.SetColor("_Color", baseColor);      // Built-in
            mat.color = baseColor;                  // Universal fallback
            
            if (emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor.Value);
            }
            
            return mat;
        }
    }
}
