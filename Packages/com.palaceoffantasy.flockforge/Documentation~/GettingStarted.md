# FlockForge - Getting Started

This guide walks you through creating a simple test scene to verify flocks are moving correctly.

## 1. Scene Setup
1.  Create a new Unity Scene (Basic 3D).
2.  Add a **Plane** (Scale 10,1,10) to serve as the ground.
3.  Add a **Directional Light**.

## 2. Create a Boid Prefab
1.  Create a simple 3D Object (e.g., Capsule or Cone) or drag in a model.
    *   *Tip: Ensure the model faces Z-forward.*
2.  Add the `BoidAgent` component to it.
3.  In the Project window, right-click and ensure you have a `BoidProfile` created (`Create > FlockForge > Boid Profile`).
4.  Assign the `BoidProfile` to the `BoidAgent` component on your prefab.
5.  Drag the object into your Project folder to make it a Prefab, then delete it from the scene.


## 3. Create Behaviors (The Flocking Recipe)
To get a real flocking movement, you need to combine 3 core behaviors + Wander.

1.  **Create the Assets**:
    *   Right-click in Project > `Create > FlockForge > Behaviors > Wander`.
    *   Right-click > `Create > FlockForge > Behaviors > Separation`.
    *   Right-click > `Create > FlockForge > Behaviors > Alignment`.
    *   Right-click > `Create > FlockForge > Behaviors > Cohesion`.

2.  **Recommended Weights** (You can tweak these in the Flock Manager later):
    *   **Separation**: 1.5 (Keeps them apart)
    *   **Alignment**: 1.0 (Makes them move together)
    *   **Cohesion**: 1.0 (Keeps them grouped)
    *   **Wander**: 0.5 (Adds randomness)

## 4. State Machine (Optional - Advanced)
If you want dynamic switching (e.g. Wander -> Flee when Predator appears):

1.  **Create a State**:
    - Right-click > `Create > FlockForge > AI > State`. Name it "WanderState".
    - Open it and drag your `Wander`, `Alignment`, `Separation`, `Cohesion` behaviors into its **Behaviors** list.

2.  **Create the State Machine**:
    - Right-click > `Create > FlockForge > AI > State Machine`. Name it "SimpleMachine".
    - Drag "WanderState" into the **Initial State** field.
    - **Important**: Also add "WanderState" to the **All States** list to ensure it's registered.

3.  **Assign to Flock**:
    - If using a State Machine, you typically don't use the "Default Behaviours" list on the FlockManager.
    - Instead, assign this "SimpleMachine" asset to your `BoidProfile` or the relevant `StateMachine` slot (depending on your Agent setup). for this simple guide, stick to "Default Behaviours" in Step 3 for now.

## 5. Setup Flock Manager
1.  Create an Empty GameObject in the scene named "FlockManager".
2.  Add the `FlockManager` component.
3.  **Assignments**:
    - `Boid Prefab`: Assign your Boid prefab.
    - `Boid Count`: Set to 50 or 100.
    - `Spawn Radius`: 10.
    - `Default Behaviours`: **Crucial Step!** Drag your behavior assets (e.g., `Wander`, `Alignment`, `Cohesion`, `Separation`) into this list. The `Weight` slider acts as a modifier for each behavior.
    - `State Machine`: Leave empty for this simple test.

    *   *Note: Behavior assets created in Project window (`Create > FlockForge > Behaviors`) must be assigned here (or in a State Machine) for the flock to calculate any movement forces.*

4.  **Target (Optional)**:
    - Create a Sphere named "Target".
    - Add `TransformTargetProvider` to the "FlockManager" object (or separate manager).
    - Assign the "Target" sphere to the `Seek Target` field.
    - *Note: If using Wander, a target is not strictly required.*

## 6. Play
Press Play! You should see boids spawn and start wandering (moving with jitter) within the defined radius.

### Troubleshooting
- **Boids not moving?** Ensure `Wander` behavior is enabled and has a weight > 0. Check `Max Speed` in the `BoidProfile`.
- **Boids flying up?** Check `Movement Plane` settings or Rigidbody gravity (if used).
- **Target ignored?** Verify `TransformTargetProvider` is on the same GameObject as `FlockManager` if using auto-discovery.
