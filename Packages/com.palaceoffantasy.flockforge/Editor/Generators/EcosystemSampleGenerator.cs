using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using PalaceOfFantasy.FlockForge.Core;
using PalaceOfFantasy.FlockForge.Unity.Components;
using PalaceOfFantasy.FlockForge.Unity.Config;
using PalaceOfFantasy.FlockForge.Unity.Config.AI;
using PalaceOfFantasy.FlockForge.Unity.Config.AI.Transitions;
using PalaceOfFantasy.FlockForge.Unity.Behaviors; 
using PalaceOfFantasy.FlockForge.Unity.Providers;
using PalaceOfFantasy.FlockForge.Unity.Ecosystem;
using System.Collections.Generic;

namespace PalaceOfFantasy.FlockForge.Editor.Generators
{
    public class EcosystemSampleGenerator
    {
        [MenuItem("FlockForge/Samples/Create Ecosystem Scene")]
        public static void CreateEcosystemScene()
        {
            Debug.Log("[EcosystemGenerator] Starting ecosystem scene generation...");
            
            try
            {
                // 1. Create folder structure
                string sampleFolder = "Assets/FlockForge_Ecosystem";
                if (!AssetDatabase.IsValidFolder(sampleFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "FlockForge_Ecosystem");
                    Debug.Log("[EcosystemGenerator] Created folder: " + sampleFolder);
                }
            
            string prefabsFolder = sampleFolder + "/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabsFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Prefabs");
            }

            string materialsFolder = sampleFolder + "/Materials";
            if (!AssetDatabase.IsValidFolder(materialsFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Materials");
            }

            // 2. Create Materials with vibrant colors
            var preyMat = CreateColorMaterial(
                new Color(0.1f, 0.7f, 1f),      // Bright cyan
                new Color(0.0f, 0.4f, 0.8f)     // Cyan emission
            );
            AssetDatabase.CreateAsset(preyMat, materialsFolder + "/PreyMaterial.mat");

            var predatorMat = CreateColorMaterial(
                new Color(1f, 0.15f, 0.1f),     // Bright red
                new Color(0.8f, 0.1f, 0.0f)     // Red emission
            );
            AssetDatabase.CreateAsset(predatorMat, materialsFolder + "/PredatorMaterial.mat");

            var foodMat = CreateColorMaterial(
                new Color(0.3f, 1f, 0.4f),      // Bright green
                new Color(0.2f, 0.8f, 0.3f)     // Green emission
            );
            AssetDatabase.CreateAsset(foodMat, materialsFolder + "/FoodMaterial.mat");

            var groundMat = CreateColorMaterial(
                new Color(0.08f, 0.08f, 0.12f), // Dark blue-gray
                null
            );
            AssetDatabase.CreateAsset(groundMat, materialsFolder + "/GroundMaterial.mat");

            var ringMat = CreateColorMaterial(
                new Color(0.2f, 0.3f, 0.5f),    // Blue-gray
                new Color(0.1f, 0.15f, 0.3f)    // Subtle glow
            );
            AssetDatabase.CreateAsset(ringMat, materialsFolder + "/RingMaterial.mat");

            // Trail Materials
            var spriteShader = Shader.Find("Sprites/Default") ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended") ?? Shader.Find("Standard");
            
            var preyTrailMat = new Material(spriteShader);
            preyTrailMat.color = new Color(0.1f, 0.7f, 1f, 0.5f);
            AssetDatabase.CreateAsset(preyTrailMat, materialsFolder + "/PreyTrailMaterial.mat");

            var predTrailMat = new Material(spriteShader);
            predTrailMat.color = new Color(1f, 0.2f, 0.1f, 0.6f);
            AssetDatabase.CreateAsset(predTrailMat, materialsFolder + "/PredatorTrailMaterial.mat");

            var additiveShader = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
            var foodGlowMat = new Material(additiveShader);
            foodGlowMat.color = new Color(0.3f, 1f, 0.4f, 0.3f);
            AssetDatabase.CreateAsset(foodGlowMat, materialsFolder + "/FoodGlowMaterial.mat");

            // 3. Create Profiles (Full 3D movement!)
            var preyProfile = ScriptableObject.CreateInstance<BoidProfile>();
            SetProperty(preyProfile, "_movementPlane", (int)MovementPlane.Free3D);
            SetProperty(preyProfile, "_maxSpeed", 8f);  // Fast prey!
            SetProperty(preyProfile, "_maxForce", 20f);
            SetProperty(preyProfile, "_perceptionRadius", 8f);
            AssetDatabase.CreateAsset(preyProfile, sampleFolder + "/PreyProfile.asset");

            var predatorProfile = ScriptableObject.CreateInstance<BoidProfile>();
            SetProperty(predatorProfile, "_movementPlane", (int)MovementPlane.Free3D);
            SetProperty(predatorProfile, "_maxSpeed", 9.5f);  // Faster than prey (8.0)
            SetProperty(predatorProfile, "_maxForce", 18f);
            SetProperty(predatorProfile, "_perceptionRadius", 12f); // Predators see further
            AssetDatabase.CreateAsset(predatorProfile, sampleFolder + "/PredatorProfile.asset");

            // 4. Create simple behaviors
            string behaviorFolder = sampleFolder + "/Behaviors";
            if (!AssetDatabase.IsValidFolder(behaviorFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Behaviors");
            }

            var wander = ScriptableObject.CreateInstance<WanderAsset>();
            SetProperties(wander, new Dictionary<string, object> {
                { "_weight", 1.0f }, { "jitter", 10f }, { "radius", 4f }, { "distance", 6f }
            });
            AssetDatabase.CreateAsset(wander, behaviorFolder + "/Wander.asset");

            var separation = ScriptableObject.CreateInstance<SeparationAsset>();
            SetProperties(separation, new Dictionary<string, object> {
                { "_weight", 2.0f }, { "radius", 2.5f }
            });
            AssetDatabase.CreateAsset(separation, behaviorFolder + "/Separation.asset");

            var alignment = ScriptableObject.CreateInstance<AlignmentAsset>();
            SetProperty(alignment, "_weight", 1.0f);
            AssetDatabase.CreateAsset(alignment, behaviorFolder + "/Alignment.asset");

            var cohesion = ScriptableObject.CreateInstance<CohesionAsset>();
            SetProperty(cohesion, "_weight", 1.0f);
            AssetDatabase.CreateAsset(cohesion, behaviorFolder + "/Cohesion.asset");

            var seek = ScriptableObject.CreateInstance<SeekAsset>();
            SetProperty(seek, "_weight", 2.0f);
            AssetDatabase.CreateAsset(seek, behaviorFolder + "/Seek.asset");

            var pursuit = ScriptableObject.CreateInstance<PursuitAsset>();
            SetProperty(pursuit, "_weight", 3.0f);
            AssetDatabase.CreateAsset(pursuit, behaviorFolder + "/Pursuit.asset");

            var flee = ScriptableObject.CreateInstance<FleeAsset>();
            SetProperty(flee, "_weight", 3.0f);
            AssetDatabase.CreateAsset(flee, behaviorFolder + "/Flee.asset");

            var stay = ScriptableObject.CreateInstance<StayInRadiusAsset>();
            SetProperties(stay, new Dictionary<string, object> {
                { "_weight", 1.0f }, { "radius", 30f }
            });
            var obstacleAvoidance = ScriptableObject.CreateInstance<ObstacleAvoidanceAsset>();
            SetProperties(obstacleAvoidance, new Dictionary<string, object> {
                { "_weight", 4.0f }, { "_avoidanceRadius", 3.0f }, { "_lookAheadDistance", 7.0f }
            });
            AssetDatabase.CreateAsset(obstacleAvoidance, behaviorFolder + "/ObstacleAvoidance.asset");

            // Create Formation Assets
            var circleFormation = ScriptableObject.CreateInstance<FormationAsset>();
            SetProperties(circleFormation, new Dictionary<string, object> { { "_type", 0 }, { "_spacing", 2f } });
            AssetDatabase.CreateAsset(circleFormation, behaviorFolder + "/CircleFormation.asset");

            var wedgeFormation = ScriptableObject.CreateInstance<FormationAsset>();
            SetProperties(wedgeFormation, new Dictionary<string, object> { { "_type", 1 }, { "_spacing", 2f } });
            AssetDatabase.CreateAsset(wedgeFormation, behaviorFolder + "/WedgeFormation.asset");

            var formationBehaviourAsset = ScriptableObject.CreateInstance<FormationBehaviourAsset>();
            SetProperties(formationBehaviourAsset, new Dictionary<string, object> { { "_weight", 15.0f }, { "_arrivalRadius", 3.0f } });
            AssetDatabase.CreateAsset(formationBehaviourAsset, behaviorFolder + "/FormationBehaviour.asset");
            
            AssetDatabase.SaveAssets();

            // 5. Create States and Transitions folders
            string statesFolder = sampleFolder + "/States";
            if (!AssetDatabase.IsValidFolder(statesFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "States");
            }

            string transitionsFolder = sampleFolder + "/Transitions";
            if (!AssetDatabase.IsValidFolder(transitionsFolder))
            {
                AssetDatabase.CreateFolder(sampleFolder, "Transitions");
            }

            // 6. Create Transitions
            var threatNearby = ScriptableObject.CreateInstance<ThreatNearbyTransitionAsset>();
            SetProperty(threatNearby, "_panicDistance", 18f); // Larger detection radius so prey notice threats while eating
            AssetDatabase.CreateAsset(threatNearby, transitionsFolder + "/ThreatNearby.asset");

            var threatGone = ScriptableObject.CreateInstance<ThreatGoneTransitionAsset>();
            SetProperty(threatGone, "_safeDistance", 15f);
            AssetDatabase.CreateAsset(threatGone, transitionsFolder + "/ThreatGone.asset");

            // 7. Create States
            var forageState = ScriptableObject.CreateInstance<StateAsset>();
            var fleeState = ScriptableObject.CreateInstance<StateAsset>();
            var huntState = ScriptableObject.CreateInstance<StateAsset>();

            // Configure ForageState: Seek, Wander, Separation, Alignment, Cohesion, StayInRadius
            var soForage = new SerializedObject(forageState);
            soForage.Update();
            var forageBehaviors = soForage.FindProperty("_behaviours");
            forageBehaviors.ClearArray();
            forageBehaviors.arraySize = 8;
            forageBehaviors.GetArrayElementAtIndex(0).objectReferenceValue = seek;
            forageBehaviors.GetArrayElementAtIndex(1).objectReferenceValue = wander;
            forageBehaviors.GetArrayElementAtIndex(2).objectReferenceValue = separation;
            forageBehaviors.GetArrayElementAtIndex(3).objectReferenceValue = alignment;
            forageBehaviors.GetArrayElementAtIndex(4).objectReferenceValue = cohesion;
            forageBehaviors.GetArrayElementAtIndex(5).objectReferenceValue = stay;
            forageBehaviors.GetArrayElementAtIndex(6).objectReferenceValue = obstacleAvoidance;
            forageBehaviors.GetArrayElementAtIndex(7).objectReferenceValue = formationBehaviourAsset; // Now correctly using BehaviourAsset
            var forageTransitions = soForage.FindProperty("_transitions");
            forageTransitions.ClearArray();
            forageTransitions.arraySize = 1;
            var t0 = forageTransitions.GetArrayElementAtIndex(0);
            t0.FindPropertyRelative("Condition").objectReferenceValue = threatNearby;
            t0.FindPropertyRelative("TargetState").objectReferenceValue = fleeState;
            t0.FindPropertyRelative("Priority").intValue = 10;
            soForage.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(forageState, statesFolder + "/ForageState.asset");

            // Configure FleeState: Flee, Separation
            var soFlee = new SerializedObject(fleeState);
            soFlee.Update();
            var fleeBehaviors = soFlee.FindProperty("_behaviours");
            fleeBehaviors.ClearArray();
            fleeBehaviors.arraySize = 2;
            fleeBehaviors.GetArrayElementAtIndex(0).objectReferenceValue = flee;
            fleeBehaviors.GetArrayElementAtIndex(1).objectReferenceValue = separation;
            var fleeTransitions = soFlee.FindProperty("_transitions");
            fleeTransitions.ClearArray();
            fleeTransitions.arraySize = 1;
            var t1 = fleeTransitions.GetArrayElementAtIndex(0);
            t1.FindPropertyRelative("Condition").objectReferenceValue = threatGone;
            t1.FindPropertyRelative("TargetState").objectReferenceValue = forageState;
            t1.FindPropertyRelative("Priority").intValue = 5;
            soFlee.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(fleeState, statesFolder + "/FleeState.asset");

            // Configure HuntState: Pursuit (Smarter!), Wander, Separation, Alignment, Cohesion, StayInRadius
            var soHunt = new SerializedObject(huntState);
            soHunt.Update();
            var huntBehaviors = soHunt.FindProperty("_behaviours");
            huntBehaviors.ClearArray();
            huntBehaviors.arraySize = 8;
            huntBehaviors.GetArrayElementAtIndex(0).objectReferenceValue = pursuit; // Use Pursuit instead of Seek
            huntBehaviors.GetArrayElementAtIndex(1).objectReferenceValue = wander;
            huntBehaviors.GetArrayElementAtIndex(2).objectReferenceValue = separation;
            huntBehaviors.GetArrayElementAtIndex(3).objectReferenceValue = alignment;
            huntBehaviors.GetArrayElementAtIndex(4).objectReferenceValue = cohesion;
            huntBehaviors.GetArrayElementAtIndex(5).objectReferenceValue = stay;
            huntBehaviors.GetArrayElementAtIndex(6).objectReferenceValue = obstacleAvoidance;
            huntBehaviors.GetArrayElementAtIndex(7).objectReferenceValue = formationBehaviourAsset;
            soHunt.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(huntState, statesFolder + "/HuntState.asset");

            // 8. Create State Machines
            var preyStateMachine = ScriptableObject.CreateInstance<StateMachineAsset>();
            var soSM = new SerializedObject(preyStateMachine);
            soSM.Update();
            soSM.FindProperty("_initialState").objectReferenceValue = forageState;
            var preyStates = soSM.FindProperty("_allStates");
            preyStates.ClearArray();
            preyStates.arraySize = 2;
            preyStates.GetArrayElementAtIndex(0).objectReferenceValue = forageState;
            preyStates.GetArrayElementAtIndex(1).objectReferenceValue = fleeState;
            soSM.FindProperty("_weight").floatValue = 1.0f;
            soSM.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(preyStateMachine, sampleFolder + "/PreyStateMachine.asset");

            var predatorStateMachine = ScriptableObject.CreateInstance<StateMachineAsset>();
            var soPSM = new SerializedObject(predatorStateMachine);
            soPSM.Update();
            soPSM.FindProperty("_initialState").objectReferenceValue = huntState;
            var predStates = soPSM.FindProperty("_allStates");
            predStates.ClearArray();
            predStates.arraySize = 1;
            predStates.GetArrayElementAtIndex(0).objectReferenceValue = huntState;
            soPSM.FindProperty("_weight").floatValue = 1.0f;
            soPSM.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(predatorStateMachine, sampleFolder + "/PredatorStateMachine.asset");

            EditorUtility.SetDirty(forageState);
            EditorUtility.SetDirty(fleeState);
            EditorUtility.SetDirty(huntState);
            EditorUtility.SetDirty(preyStateMachine);
            EditorUtility.SetDirty(predatorStateMachine);
            AssetDatabase.SaveAssets();

            // 9. Create Prey Prefab - Using Floreswa low-poly fish model
            var preyGO = new GameObject("PreyBoid");
            
            // Try to load Floreswa fish model
            var preyFishPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Floreswa/Prefabs/fish01_shade.prefab");
            if (preyFishPrefab != null)
            {
                var fishModel = (GameObject)PrefabUtility.InstantiatePrefab(preyFishPrefab);
                fishModel.name = "Model";
                fishModel.transform.SetParent(preyGO.transform);
                fishModel.transform.localPosition = Vector3.zero;
                fishModel.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face forward
                fishModel.transform.localScale = Vector3.one * 0.3f; // Scale to appropriate size
                Debug.Log("[EcosystemGenerator] 🐟 Using Floreswa fish01 model for Prey!");
            }
            else
            {
                // Fallback to primitive shapes
                var preyBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                preyBody.name = "Body";
                preyBody.transform.SetParent(preyGO.transform);
                preyBody.transform.localPosition = Vector3.zero;
                preyBody.transform.localRotation = Quaternion.Euler(90, 0, 0);
                preyBody.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
                preyBody.GetComponent<Renderer>().sharedMaterial = preyMat;
                Object.DestroyImmediate(preyBody.GetComponent<Collider>());
                
                var preyTail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                preyTail.name = "Tail";
                preyTail.transform.SetParent(preyGO.transform);
                preyTail.transform.localPosition = new Vector3(0, 0, -0.3f);
                preyTail.transform.localRotation = Quaternion.Euler(0, 0, 45);
                preyTail.transform.localScale = new Vector3(0.15f, 0.15f, 0.02f);
                preyTail.GetComponent<Renderer>().sharedMaterial = preyMat;
                Object.DestroyImmediate(preyTail.GetComponent<Collider>());
                Debug.Log("[EcosystemGenerator] Floreswa asset not found, using primitive shapes for Prey");
            }
            
            // Add trail renderer
            var preyTrail = preyGO.AddComponent<TrailRenderer>();
            preyTrail.time = 0.3f;
            preyTrail.startWidth = 0.1f;
            preyTrail.endWidth = 0f;
            preyTrail.material = preyTrailMat;
            preyTrail.startColor = new Color(0.1f, 0.7f, 1f, 0.5f);
            preyTrail.endColor = new Color(0.1f, 0.7f, 1f, 0f);
            
            var preyCol = preyGO.AddComponent<SphereCollider>();
            preyCol.isTrigger = true;
            preyCol.radius = 0.3f;
            
            // Add procedural swimming animation
            var swim = preyGO.AddComponent<ProceduralSwim>();
            swim.SetFrequency(12f);
            swim.SetAmplitude(15f);
            
            var preyAgent = preyGO.AddComponent<BoidAgent>();
            preyGO.AddComponent<PreyCatchable>();
            preyGO.AddComponent<NearestFoodTargetProvider>();
            preyGO.AddComponent<BoidBrain>(); // Evolution genes
            var preyGeneApplicator = preyGO.AddComponent<BoidGeneApplicator>();
            SetProperty(preyGeneApplicator, "_isPrey", true);
            
            // Add Selection/Command support
            preyGO.AddComponent<BoidCommandAgent>();

            SetProperty(preyAgent, "_profile", preyProfile);
            EditorUtility.SetDirty(preyAgent); // Ensure profile reference is serialized
            preyGO.layer = 6; // Boids layer

            var preyPrefab = PrefabUtility.SaveAsPrefabAsset(preyGO, prefabsFolder + "/PreyBoid.prefab");
            GameObject.DestroyImmediate(preyGO);

            // 10. Create Predator Prefab - Using Floreswa low-poly fish model (larger)
            var predatorGO = new GameObject("PredatorBoid");
            
            // Try to load Floreswa fish model (fish03 for predator - different species)
            var predFishPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Floreswa/Prefabs/fish03_shade.prefab");
            if (predFishPrefab != null)
            {
                var fishModel = (GameObject)PrefabUtility.InstantiatePrefab(predFishPrefab);
                fishModel.name = "Model";
                fishModel.transform.SetParent(predatorGO.transform);
                fishModel.transform.localPosition = Vector3.zero;
                fishModel.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face forward
                fishModel.transform.localScale = Vector3.one * 0.6f; // Larger than prey
                Debug.Log("[EcosystemGenerator] 🦈 Using Floreswa fish03 model for Predator!");
            }
            else
            {
                // Fallback to primitive shapes 
                var predBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                predBody.name = "Body";
                predBody.transform.SetParent(predatorGO.transform);
                predBody.transform.localPosition = Vector3.zero;
                predBody.transform.localRotation = Quaternion.Euler(90, 0, 0);
                predBody.transform.localScale = new Vector3(0.35f, 0.7f, 0.35f);
                predBody.GetComponent<Renderer>().sharedMaterial = predatorMat;
                Object.DestroyImmediate(predBody.GetComponent<Collider>());
                
                var dorsalFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dorsalFin.name = "DorsalFin";
                dorsalFin.transform.SetParent(predatorGO.transform);
                dorsalFin.transform.localPosition = new Vector3(0, 0.25f, 0);
                dorsalFin.transform.localRotation = Quaternion.Euler(0, 0, 0);
                dorsalFin.transform.localScale = new Vector3(0.05f, 0.3f, 0.15f);
                dorsalFin.GetComponent<Renderer>().sharedMaterial = predatorMat;
                Object.DestroyImmediate(dorsalFin.GetComponent<Collider>());
                
                var predTail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                predTail.name = "Tail";
                predTail.transform.SetParent(predatorGO.transform);
                predTail.transform.localPosition = new Vector3(0, 0, -0.5f);
                predTail.transform.localRotation = Quaternion.Euler(0, 0, 45);
                predTail.transform.localScale = new Vector3(0.25f, 0.25f, 0.03f);
                predTail.GetComponent<Renderer>().sharedMaterial = predatorMat;
                Object.DestroyImmediate(predTail.GetComponent<Collider>());
                Debug.Log("[EcosystemGenerator] Floreswa asset not found, using primitive shapes for Predator");
            }
            
            // Aggressive trail
            var predTrail = predatorGO.AddComponent<TrailRenderer>();
            predTrail.time = 0.4f;
            predTrail.startWidth = 0.2f;
            predTrail.endWidth = 0f;
            predTrail.material = predTrailMat;
            predTrail.startColor = new Color(1f, 0.2f, 0.1f, 0.6f);
            predTrail.endColor = new Color(1f, 0.1f, 0f, 0f);
            
            var predCol = predatorGO.AddComponent<SphereCollider>();
            predCol.isTrigger = true;
            predCol.radius = 0.5f;
            
            // Add procedural swimming animation (slower but more powerful)
            var predSwim = predatorGO.AddComponent<ProceduralSwim>();
            predSwim.SetFrequency(8f);
            predSwim.SetAmplitude(20f);
            
            var predatorAgent = predatorGO.AddComponent<BoidAgent>();
            predatorGO.AddComponent<PredatorLifespan>();
            predatorGO.AddComponent<NearestPreyTargetProvider>();
            predatorGO.AddComponent<BoidBrain>(); // Evolution genes
            var predGeneApplicator = predatorGO.AddComponent<BoidGeneApplicator>();
            SetProperty(predGeneApplicator, "_isPrey", false);
            
            // Add Selection/Command support
            predatorGO.AddComponent<BoidCommandAgent>();

            SetProperty(predatorAgent, "_profile", predatorProfile);
            EditorUtility.SetDirty(predatorAgent); // Ensure profile reference is serialized
            predatorGO.layer = 6; // Boids layer

            var predatorPrefab = PrefabUtility.SaveAsPrefabAsset(predatorGO, prefabsFolder + "/PredatorBoid.prefab");
            GameObject.DestroyImmediate(predatorGO);

            // 11. Create Food Prefab - Glowing orb with particles
            var foodGO = new GameObject("Food");
            
            // Core sphere
            var foodCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foodCore.name = "Core";
            foodCore.transform.SetParent(foodGO.transform);
            foodCore.transform.localPosition = Vector3.zero;
            foodCore.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            foodCore.GetComponent<Renderer>().sharedMaterial = foodMat;
            Object.DestroyImmediate(foodCore.GetComponent<Collider>());
            
            // Outer glow sphere
            var foodGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foodGlow.name = "Glow";
            foodGlow.transform.SetParent(foodGO.transform);
            foodGlow.transform.localPosition = Vector3.zero;
            foodGlow.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            var glowRenderer = foodGlow.GetComponent<Renderer>();
            glowRenderer.sharedMaterial = foodGlowMat;
            Object.DestroyImmediate(foodGlow.GetComponent<Collider>());
            
            var foodCol = foodGO.AddComponent<SphereCollider>();
            foodCol.isTrigger = true;
            foodCol.radius = 0.5f;
            
            foodGO.AddComponent<FoodTarget>();
            
            // Add spinning animation via simple script or rotation
            var foodRotator = foodGO.AddComponent<SpinObject>();

            var foodPrefab = PrefabUtility.SaveAsPrefabAsset(foodGO, prefabsFolder + "/Food.prefab");
            GameObject.DestroyImmediate(foodGO);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 8. Create Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 9. Setup Lighting - Underwater atmosphere with caustic-style lighting
            var mainLight = GameObject.Find("Directional Light");
            if (mainLight != null)
            {
                mainLight.transform.rotation = Quaternion.Euler(45, -30, 0);
                var light = mainLight.GetComponent<Light>();
                light.color = new Color(0.3f, 0.6f, 0.9f); // Deep ocean blue
                light.intensity = 1.2f; // Brighter for underwater caustic feel
                light.shadows = LightShadows.Soft;
            }
            
            // Add volumetric fog for underwater depth
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.015f;
            RenderSettings.fogColor = new Color(0.01f, 0.08f, 0.15f); // Slightly darker for depth
            
            // 9b. Setup Skybox
            var skyboxTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Floreswa/Materials/OceanSkybox.png");
            if (skyboxTex != null)
            {
                // Ensure texture is set to Clamp to avoid seams (though we can't change import settings easily via code without TextureImporter, we assume user or auto-init)
                var skyboxMat = new Material(Shader.Find("Skybox/Panoramic"));
                skyboxMat.SetTexture("_MainTex", skyboxTex);
                AssetDatabase.CreateAsset(skyboxMat, materialsFolder + "/OceanSkybox.mat");
                RenderSettings.skybox = skyboxMat;
                Debug.Log("[EcosystemGenerator] 🌌 Ocean Skybox applied!");
            }
            
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.15f, 0.25f, 0.4f); // Blue sky light from above
            RenderSettings.ambientEquatorColor = new Color(0.1f, 0.2f, 0.3f); // Mid-depth blue
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.05f, 0.1f); // Dark ocean floor
            
            // 9b. Create Ground Plane - Much larger for more space
            var groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundGO.name = "Ground";
            groundGO.transform.position = new Vector3(0, -0.5f, 0);
            groundGO.transform.localScale = new Vector3(15, 1, 15); // 150x150 units
            groundGO.GetComponent<Renderer>().sharedMaterial = groundMat;
            groundGO.layer = 7; // Ground layer
            Object.DestroyImmediate(groundGO.GetComponent<Collider>());
            groundGO.AddComponent<MeshCollider>(); // Real collision for raycasting
            
            // 9c. Create simple corner markers (cleaner than rings)
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 2 / 4 + Mathf.PI / 4;
                float radius = 70f;
                
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = $"Marker_{i}";
                marker.transform.position = new Vector3(Mathf.Cos(angle) * radius, 0.5f, Mathf.Sin(angle) * radius);
                marker.transform.localScale = new Vector3(2f, 2f, 2f);
                marker.GetComponent<Renderer>().sharedMaterial = ringMat;
                Object.DestroyImmediate(marker.GetComponent<Collider>());
            }

            // 9d. Create Obstacles (Environment)
            var obstaclesGO = new GameObject("Environment_Obstacles");
            var obstacleMat = CreateColorMaterial(new Color(0.4f, 0.25f, 0.1f)); // Rock brown
            for (int i = 0; i < 5; i++)
            {
                var obs = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obs.name = $"Rock_{i}";
                obs.transform.SetParent(obstaclesGO.transform);
                float angle = i * Mathf.PI * 2 / 5;
                obs.transform.position = new Vector3(Mathf.Cos(angle) * 30f, 15f + i * 5f, Mathf.Sin(angle) * 30f);
                obs.transform.localScale = Vector3.one * (5f + i);
                obs.GetComponent<Renderer>().sharedMaterial = obstacleMat;
                
                // Add obstacle component for boids
                var flockObs = obs.AddComponent<FlockObstacle>();
                SetProperty(flockObs, "_radius", (5f + i) * 0.5f);
            }

            // 10. Create Ecosystem Manager
            var ecosystemGO = new GameObject("EcosystemManager");
            var ecosystem = ecosystemGO.AddComponent<EcosystemManager>();
            ecosystemGO.AddComponent<EcosystemUI>();
            
            // 10b. Create Evolution Manager for machine learning
            var evolutionGO = new GameObject("EvolutionManager");
            var evolution = evolutionGO.AddComponent<EvolutionManager>();
            EditorUtility.SetDirty(evolution);

            // 10c. Create Selection Manager for RTS Control
            var selectionGO = new GameObject("SelectionManager");
            var selectionManager = selectionGO.AddComponent<SelectionManager>();
            
            // Setup selection box visual (UI)
            var canvasGO = new GameObject("SelectionCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            var boxGO = new GameObject("SelectionBox", typeof(RectTransform));
            boxGO.transform.SetParent(canvasGO.transform);
            var boxRect = boxGO.GetComponent<RectTransform>();
            boxRect.anchorMin = Vector2.zero;
            boxRect.anchorMax = Vector2.zero;
            boxRect.pivot = Vector2.one * 0.5f;
            var boxImg = boxGO.AddComponent<UnityEngine.UI.Image>();
            boxImg.color = new Color(0, 1, 0, 0.2f);
            boxGO.SetActive(false);
            
            SetProperty(selectionManager, "_selectionBoxVisual", boxRect);
            // LayerMask for Selection (Boids - Layer 6)
            SetProperty(selectionManager, "_selectionLayer", (int)(1 << 6));
            // LayerMask for Ground (Layer 7)
            SetProperty(selectionManager, "_groundLayer", (int)(1 << 7));

            // Configure ecosystem manager via SerializedObject
            var soEco = new SerializedObject(ecosystem);
            soEco.Update();
            soEco.FindProperty("_preyPrefab").objectReferenceValue = preyPrefab;
            soEco.FindProperty("_predatorPrefab").objectReferenceValue = predatorPrefab;
            soEco.FindProperty("_foodPrefab").objectReferenceValue = foodPrefab;
            soEco.FindProperty("_maxPrey").intValue = 800;
            soEco.FindProperty("_maxPredators").intValue = 60;
            soEco.FindProperty("_maxFood").intValue = 60;
            soEco.FindProperty("_initialPrey").intValue = 150;
            soEco.FindProperty("_initialPredators").intValue = 8;
            soEco.FindProperty("_initialFood").intValue = 40;
            soEco.FindProperty("_spawnRadius").floatValue = 60f;
            soEco.FindProperty("_predatorLifespan").floatValue = 30f;
            soEco.FindProperty("_predatorReproductionChance").floatValue = 0.15f;
            soEco.FindProperty("_preyReproductionChance").floatValue = 1.0f; // 100% - every bite spawns new prey
            soEco.ApplyModifiedProperties();
            EditorUtility.SetDirty(ecosystem);

            // Create Prey FlockManager
            var preyManagerGO = new GameObject("PreyFlockManager");
            var preyManager = preyManagerGO.AddComponent<FlockManager>();
            var preyManagerProvider = preyManagerGO.AddComponent<TransformTargetProvider>();
            var preyFormation = preyManagerGO.AddComponent<UnityFormationController>();
            SetProperty(preyFormation, "_currentFormation", circleFormation);
            
            var soPreyManager = new SerializedObject(preyManager);
            soPreyManager.Update();
            var preyBehaviorsArray = soPreyManager.FindProperty("_defaultBehaviours");
            preyBehaviorsArray.ClearArray();
            preyBehaviorsArray.arraySize = 1;
            preyBehaviorsArray.GetArrayElementAtIndex(0).objectReferenceValue = preyStateMachine;
            soPreyManager.FindProperty("_boidPrefab").objectReferenceValue = preyPrefab;
            soPreyManager.FindProperty("_spawnCount").intValue = 0; // EcosystemManager handles spawning
            soPreyManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(preyManager);

            // Create Predator FlockManager
            var predatorManagerGO = new GameObject("PredatorFlockManager");
            var predatorManager = predatorManagerGO.AddComponent<FlockManager>();
            var predatorManagerProvider = predatorManagerGO.AddComponent<TransformTargetProvider>();
            var predatorFormation = predatorManagerGO.AddComponent<UnityFormationController>();
            SetProperty(predatorFormation, "_currentFormation", wedgeFormation);
            
            var soPredManager = new SerializedObject(predatorManager);
            soPredManager.Update();
            var predBehaviorsArray = soPredManager.FindProperty("_defaultBehaviours");
            predBehaviorsArray.ClearArray();
            predBehaviorsArray.arraySize = 1;
            predBehaviorsArray.GetArrayElementAtIndex(0).objectReferenceValue = predatorStateMachine;
            soPredManager.FindProperty("_boidPrefab").objectReferenceValue = predatorPrefab;
            soPredManager.FindProperty("_spawnCount").intValue = 0; // EcosystemManager handles spawning
            soPredManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(predatorManager);

            // Wire predators as threats for prey
            var soPreyProvider = new SerializedObject(preyManagerProvider);
            soPreyProvider.Update();
            var threatsList = soPreyProvider.FindProperty("_threats");
            if (threatsList != null)
            {
                threatsList.ClearArray();
                threatsList.arraySize = 1;
                threatsList.GetArrayElementAtIndex(0).objectReferenceValue = predatorManagerGO.transform;
            }
            soPreyProvider.ApplyModifiedProperties();
            EditorUtility.SetDirty(preyManagerProvider);

            // Update EcosystemManager with FlockManager references and 3D bounds
            soEco.Update();
            soEco.FindProperty("_preyFlockManager").objectReferenceValue = preyManager;
            soEco.FindProperty("_predatorFlockManager").objectReferenceValue = predatorManager;
            soEco.FindProperty("_spawnRadius").floatValue = 60f;
            soEco.FindProperty("_minHeight").floatValue = 5f;
            soEco.FindProperty("_maxHeight").floatValue = 45f; // Below water surface
            soEco.ApplyModifiedProperties();
            EditorUtility.SetDirty(ecosystem);

            // 11. Position Camera - higher up for larger map
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 80, -60);
                cam.transform.rotation = Quaternion.Euler(50, 0, 0);
                cam.fieldOfView = 70; // Wider FOV
                
                // Set clearing based on whether we have a skybox
                if (RenderSettings.skybox != null)
                {
                    cam.clearFlags = CameraClearFlags.Skybox;
                }
                else
                {
                    cam.backgroundColor = new Color(0.02f, 0.03f, 0.06f); // Very dark blue
                    cam.clearFlags = CameraClearFlags.SolidColor;
                }
                
                // Add camera controller for in-game navigation
                cam.gameObject.AddComponent<EcosystemCameraController>();
            }

            // 12. Create Water Surface (decorative ocean plane)
            var waterMat = new Material(Shader.Find("Sprites/Default"));
            waterMat.color = new Color(0.1f, 0.3f, 0.5f, 0.3f); // Semi-transparent blue
            AssetDatabase.CreateAsset(waterMat, materialsFolder + "/WaterSurface.mat");
            
            var waterSurface = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterSurface.name = "WaterSurface";
            waterSurface.transform.position = new Vector3(0, 50f, 0); // High water surface for 3D underwater world
            waterSurface.transform.localScale = new Vector3(20, 1, 20); // 200x200 units
            waterSurface.GetComponent<Renderer>().sharedMaterial = waterMat;
            Object.DestroyImmediate(waterSurface.GetComponent<Collider>());
            waterSurface.tag = "Water"; // For underwater detection

            // 13. Try to add Post Processing Volume with pre-configured profile
            try
            {
                // Create a global volume for atmosphere
                var volumeGO = new GameObject("PostProcessingVolume");
                
                // Try to add URP Volume component
                var volumeType = System.Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
                var profileType = System.Type.GetType("UnityEngine.Rendering.VolumeProfile, Unity.RenderPipelines.Core.Runtime");
                
                if (volumeType != null && profileType != null)
                {
                    var volume = volumeGO.AddComponent(volumeType);
                    
                    // Set it as global
                    var isGlobalProp = volumeType.GetProperty("isGlobal");
                    if (isGlobalProp != null) isGlobalProp.SetValue(volume, true);
                    
                    // Check if profile already exists (preserve user settings!)
                    string profilePath = sampleFolder + "/UnderwaterVolumeProfile.asset";
                    var existingProfile = AssetDatabase.LoadAssetAtPath(profilePath, profileType);
                    UnityEngine.Object profile;
                    bool profileExisted = existingProfile != null;
                    
                    if (existingProfile != null)
                    {
                        profile = existingProfile;
                        Debug.Log("[EcosystemGenerator] ♻️ Reusing existing UnderwaterVolumeProfile (your settings are preserved!)");
                    }
                    else
                    {
                        // Create new Volume Profile asset
                        profile = ScriptableObject.CreateInstance(profileType);
                        AssetDatabase.CreateAsset(profile, profilePath);
                    
                    // Try to add Bloom
                    var bloomType = System.Type.GetType("UnityEngine.Rendering.Universal.Bloom, Unity.RenderPipelines.Universal.Runtime");
                    if (bloomType != null)
                    {
                        var addMethod = profileType.GetMethod("Add", new[] { typeof(System.Type), typeof(bool) });
                        if (addMethod != null)
                        {
                            var bloom = addMethod.Invoke(profile, new object[] { bloomType, true });
                            
                            // Configure Bloom via reflection
                            var thresholdField = bloomType.GetField("threshold");
                            var intensityField = bloomType.GetField("intensity");
                            var scatterField = bloomType.GetField("scatter");
                            
                            if (thresholdField != null)
                            {
                                var thresholdParam = thresholdField.GetValue(bloom);
                                var overrideProp = thresholdParam.GetType().GetProperty("overrideState");
                                var valueProp = thresholdParam.GetType().GetProperty("value");
                                if (overrideProp != null) overrideProp.SetValue(thresholdParam, true);
                                if (valueProp != null) valueProp.SetValue(thresholdParam, 0.8f);
                            }
                            if (intensityField != null)
                            {
                                var intensityParam = intensityField.GetValue(bloom);
                                var overrideProp = intensityParam.GetType().GetProperty("overrideState");
                                var valueProp = intensityParam.GetType().GetProperty("value");
                                if (overrideProp != null) overrideProp.SetValue(intensityParam, true);
                                if (valueProp != null) valueProp.SetValue(intensityParam, 1.5f);
                            }
                            if (scatterField != null)
                            {
                                var scatterParam = scatterField.GetValue(bloom);
                                var overrideProp = scatterParam.GetType().GetProperty("overrideState");
                                var valueProp = scatterParam.GetType().GetProperty("value");
                                if (overrideProp != null) overrideProp.SetValue(scatterParam, true);
                                if (valueProp != null) valueProp.SetValue(scatterParam, 0.7f);
                            }
                            
                            Debug.Log("[EcosystemGenerator] Added Bloom effect to Volume Profile");
                        }
                    }
                    
                    // Try to add Vignette
                    var vignetteType = System.Type.GetType("UnityEngine.Rendering.Universal.Vignette, Unity.RenderPipelines.Universal.Runtime");
                    if (vignetteType != null)
                    {
                        var addMethod = profileType.GetMethod("Add", new[] { typeof(System.Type), typeof(bool) });
                        if (addMethod != null)
                        {
                            var vignette = addMethod.Invoke(profile, new object[] { vignetteType, true });
                            
                            // Configure Vignette
                            var colorField = vignetteType.GetField("color");
                            var intensityField = vignetteType.GetField("intensity");
                            var smoothnessField = vignetteType.GetField("smoothness");
                            
                            if (colorField != null)
                            {
                                var colorParam = colorField.GetValue(vignette);
                                var overrideProp = colorParam.GetType().GetProperty("overrideState");
                                var valueProp = colorParam.GetType().GetProperty("value");
                                if (overrideProp != null) overrideProp.SetValue(colorParam, true);
                                if (valueProp != null) valueProp.SetValue(colorParam, new Color(0.05f, 0.15f, 0.25f));
                            }
                            if (intensityField != null)
                            {
                                var intensityParam = intensityField.GetValue(vignette);
                                var overrideProp = intensityParam.GetType().GetProperty("overrideState");
                                var valueProp = intensityParam.GetType().GetProperty("value");
                                if (overrideProp != null) overrideProp.SetValue(intensityParam, true);
                                if (valueProp != null) valueProp.SetValue(intensityParam, 0.4f);
                            }
                            if (smoothnessField != null)
                            {
                                var smoothnessParam = smoothnessField.GetValue(vignette);
                                var overrideProp = smoothnessParam.GetType().GetProperty("overrideState");
                                var valueProp = smoothnessParam.GetType().GetProperty("value");
                                if (overrideProp != null) overrideProp.SetValue(smoothnessParam, true);
                                if (valueProp != null) valueProp.SetValue(smoothnessParam, 0.5f);
                            }
                            
                            Debug.Log("[EcosystemGenerator] Added Vignette effect to Volume Profile");
                        }
                    }
                    
                        Debug.Log("[EcosystemGenerator] ✅ Created new UnderwaterVolumeProfile with Bloom + Vignette!");
                    } // end of else block (new profile creation)
                    
                    // Assign profile to volume (always, whether existing or new)
                    var profileProp = volumeType.GetProperty("sharedProfile");
                    if (profileProp != null)
                    {
                        profileProp.SetValue(volume, profile);
                    }
                    else
                    {
                        var profileField = volumeType.GetField("sharedProfile");
                        if (profileField != null) profileField.SetValue(volume, profile);
                    }
                    
                    EditorUtility.SetDirty(profile);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    Debug.Log("[EcosystemGenerator] URP Volume types not found. Using standard lighting only.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[EcosystemGenerator] Could not add Post Processing: " + ex.Message);
            }

            // 14. Try to add Underwater Effect (if available)
            try
            {
                var underwaterType = System.Type.GetType("UnderwaterEffect.CameraUnderwaterEffect, Assembly-CSharp");
                if (underwaterType != null && cam != null)
                {
                    var underwater = cam.gameObject.AddComponent(underwaterType);
                    Debug.Log("[EcosystemGenerator] Added CameraUnderwaterEffect! Configure water layers in Inspector.");
                }
                else
                {
                    Debug.Log("[EcosystemGenerator] Underwater Effect not found in project. Import it from Assets folder if needed.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[EcosystemGenerator] Could not add Underwater Effect: " + ex.Message);
            }

            // 13. Save Scene
            string scenePath = sampleFolder + "/EcosystemSample.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.ImportAsset(scenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[EcosystemGenerator] SUCCESS! Created Ecosystem Sample Scene at: " + scenePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[EcosystemGenerator] FAILED: " + ex.Message);
                Debug.LogException(ex);
            }
        }

        private static Shader GetLitShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null) return shader;
            shader = Shader.Find("HDRP/Lit");
            if (shader != null) return shader;
            shader = Shader.Find("Standard");
            if (shader != null) return shader;
            shader = Shader.Find("Unlit/Color");
            return shader;
        }

        private static Material CreateColorMaterial(Color baseColor, Color? emissionColor = null)
        {
            var shader = GetLitShader();
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            mat.color = baseColor;
            if (emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor.Value);
            }
            return mat;
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
            }
            so.ApplyModifiedProperties();
        }

        private static void SetProperties(Object target, Dictionary<string, object> properties)
        {
            var so = new SerializedObject(target);
            so.Update();
            foreach (var kvp in properties)
            {
                var prop = so.FindProperty(kvp.Key);
                if (prop != null)
                {
                    if (kvp.Value is int i) prop.intValue = i;
                    else if (kvp.Value is float f) prop.floatValue = f;
                    else if (kvp.Value is bool b) prop.boolValue = b;
                    else if (kvp.Value is Object o) prop.objectReferenceValue = o;
                }
            }
            so.ApplyModifiedProperties();
        }
    }
}
