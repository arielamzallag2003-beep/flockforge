# FlockForge

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

**FlockForge** is a modular, designer-friendly boid system for Unity. Create flocking behaviors, state machines, and formations with ease.

## ✨ Features

### Major Features
- **🧠 Behavior System** — Composable behaviors (Seek, Flee, Cohesion, Separation, Alignment, Wander) as ScriptableObjects with adjustable weights
- **🔄 State Machine** — Define states (Idle, Alert, Chase, Flee) with configurable transitions via Inspector
- **📐 Formation System** — Tactical formations (Line, Wedge, Circle) with automatic slot reassignment
- **🎯 Command & Selection** — RTS-style selection and commands (move, patrol, hold position)

### Minor Features
- **🔍 Visual Debugger** — Real-time visualization of perception, velocity, and forces
- **🚧 Obstacle Avoidance** — Raycast-based obstacle detection and avoidance
- **📡 Event System** — Custom events (OnReachedFood, OnThreatDetected) with decoupled handlers
- **📋 Profile System** — BoidProfiles as ScriptableObjects with included presets

## 📦 Installation

### Via Package Manager (Git URL)
1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter: `https://github.com/palaceoffantasy/flockforge.git?path=Packages/com.palaceoffantasy.flockforge`

### Via manifest.json
Add to your `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.palaceoffantasy.flockforge": "https://github.com/palaceoffantasy/flockforge.git?path=Packages/com.palaceoffantasy.flockforge"
  }
}
```

## 🚀 Quick Start

1. Create an empty GameObject and add the **FlockManager** component
2. Create a **BoidProfile** asset (Right-click > Create > FlockForge > Boid Profile)
3. Add **BoidAgent** components to your agent prefabs
4. Assign behaviors and watch them flock!

## 📖 Documentation

See the [Documentation](Documentation~/flockforge.md) for detailed usage guides.

## 🎮 Samples

Import the **L'Épervier Demo** sample from the Package Manager to see FlockForge in action!

## 📄 License

MIT License - see [LICENSE.md](LICENSE.md)

---

Made with ❤️ by **Palace of Fantasy**
