# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-01-27

### Added
- Initial release
- Core boid system with `IBoid`, `IFlock`, `IBehaviour` interfaces
- Basic behaviors: Cohesion, Separation, Alignment, Seek, Flee, Wander
- State machine system with `IState`, `IStateMachine`
- Event system with `IEventManager`
- Unity wrappers: `BoidAgent`, `FlockManager`
- Visual debugging with `IDebugRenderer`
