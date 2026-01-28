# FlockForge Documentation

## Overview

FlockForge is a modular boid system designed for Unity with a cross-engine architecture. The core is written in pure C# without Unity dependencies, making it portable to other engines.

## Architecture

```
FlockForge
├── Core (Pure C# - No Unity Dependencies)
│   ├── Math/          - FVector3, FMath utilities
│   ├── Interfaces/    - IBoid, IFlock, IBehaviour, etc.
│   ├── Implementations/ - Flock, StateMachine, EventManager
│   └── Behaviors/     - Cohesion, Separation, Alignment, Seek, Flee, Wander
│
└── Unity (Unity-Specific Wrappers)
    ├── Components/    - BoidAgent, FlockManager
    ├── Config/        - BoidProfile, BehaviourAsset
    ├── Debug/         - UnityDebugRenderer
    └── Extensions/    - Vector3 ↔ FVector3 conversions
```

## Core Concepts

### IBoid
The individual agent that moves within a flock. Has position, velocity, and can receive steering forces from behaviors.

### IBehaviour
A steering behavior that calculates a force to apply to a boid. Behaviors are composable and weighted.

### IBoidContext
A read-only snapshot passed to behaviors containing:
- The boid itself
- Neighboring boids
- Current seek targets
- Nearby threats
- Formation slot (if in formation)

### IFlock
Manages a group of boids. Responsible for:
- Calculating neighbors for each boid
- Stepping the simulation
- Tracking center of mass and average velocity

### IStateMachine
Manages behavioral states (Idle, Alert, Chase, Flee) and transitions between them based on conditions.

## Quick Start

1. Add a `FlockManager` component to an empty GameObject
2. Create a `BoidProfile` asset for your agent configuration
3. Add `BoidAgent` components to your agent prefabs
4. Assign behaviors to your agents
5. Press Play!

## Samples

### L'Épervier Demo
A prey/predator game demonstrating:
- State machine with 5 states (Forage, Alert, Flee, Scatter, Regroup)
- Multiple behaviors working together
- Event system for game logic
- Visual debugging
