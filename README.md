# Swinging Paint Bucket VR Project

A real-time physics-based simulation of a swinging paint bucket system inspired by pendulum art experiments.

## Features

- **Pendulum Physics**: Custom pendulum simulation with adjustable length, gravity, and damping
- **Paint Particle System**: Velocity-based paint emission with dynamic rates
- **Surface Painting**: Real-time texture painting where particles hit
- **No Unity Physics**: All physics implemented from scratch using numerical integration

## Project Structure

After the 2026-05-24 refactor the codebase is split into five decoupled sections, each with its own `Components/`, `Data/`, `Interfaces/`, and `Services/` sub-folders:

```
Assets/
├── Code/
│   ├── Features/
│   │   ├── Pendulum/            # Core pendulum physics & controller
│   │   │   ├── Components/      # PendulumController (MonoBehaviour)
│   │   │   ├── Data/            # PendulumConfig, PendulumState
│   │   │   ├── Interfaces/      # IPendulumPhysics, IPendulumView, IPendulumInputHandler
│   │   │   │                     # IMassProvider, IRope
│   │   │   └── Services/        # PendulumPhysicsService, PendulumInputHandler, MassSystem
│   │   │
│   │   ├── Rope/                # Rope abstraction & rendering
│   │   │   ├── Components/      # RopeRenderer (IRope + LineRenderer)
│   │   │   ├── Data/            # RopeConfig
│   │   │   ├── Interfaces/      # IRope
│   │   │   └── Services/        # FixedLengthRope
│   │   │
│   │   ├── Paint/               # Fluid emission & particle management
│   │   │   ├── Components/      # PaintEmitter, ParticleRenderer
│   │   │   ├── Data/            # PaintEmissionConfig, ParticleData
│   │   │   ├── Interfaces/      # IPaintEmissionService, IParticlePhysicsService
│   │   │   │                     # IMassLossNotifier
│   │   │   └── Services/        # DynamicPaintEmissionService, ParticlePhysicsService
│   │   │
│   │   ├── Surface/             # Paintable ground plane
│   │   │   ├── Components/      # PaintSurfaceSystem
│   │   │   ├── Interfaces/      # IPaintSurfaceService
│   │   │   └── Services/        # PaintApplicationService
│   │   │
│   │   └── ExternalForces/      # Pluggable force providers
│   │       ├── Data/            # ForceConfig
│   │       ├── Interfaces/      # IForceProvider
│   │       └── Services/        # GravityForce, DragForce
│   │
│   └── Editor/
│       └── Scripts/             # SceneBuilder.cs (composition root)
│
├── Scenes/
│   └── Main/                    # Main simulation scene (SceneBuilder)
└── Resources/
    └── Prefabs/                 # PaintParticle prefab
```

## How to Run

### Using Scene Builder (Automatic)
1. Open Unity Hub and the project
2. In Unity Editor, go to `SwingingPaintBucket > Build Main Scene`
3. The scene will be created and saved to `Assets/Scenes/Main/SwingingPaintBucketMain.unity`

### Manual Scene Setup
See [SceneSetup.md](Docs/SceneSetup.md) for manual configuration steps.

## Controls

- **Rope Length**: 1-5 meters (longer = slower swing)
- **Gravity**: 1-20 m/s² (higher = faster swing)
- **Damping**: 0-1 (0.1 for realistic decay)
- **Angle**: -90° to 90° (starting position)

## Physics Model

The pendulum uses a simple damped pendulum equation:
```
α = -(g/L)sin(θ) - dω
```
Where α is angular acceleration, g is gravity, L is rope length, θ is angle, and d is damping.