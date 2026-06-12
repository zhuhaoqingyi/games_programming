# Space Builder

A 2D space survival base-building game built with Unity. Start from a damaged spaceship and build a self-sufficient, powerful spacecraft from scratch.

## Features

- **Grid-Based Building System**: Place and rotate buildings on a spatial grid with collision detection
- **Resource Management**: Collect space ore, refine metals, manufacture parts through production chains
- **Power System**: Manage power generation (Nuclear Reactor, Solar Array) and consumption across all buildings
- **Production Pipeline**: Multi-stage resource processing from raw ore to advanced components
- **Save/Load System**: Press `Ctrl+S` to save, load from the main menu
- **Audio System**: Background music, placement sounds, mining sounds, thruster effects, and explosion effects
- **Dynamic Space Background**: Scrolling space environment with procedurally generated ore deposits

## Getting Started

### Prerequisites

- Unity 2021.3 LTS or later
- Unity Package Manager (for built-in packages)

### Installation

1. Clone or download this repository
2. Open the project in Unity Hub
3. Open `Scenes/MainMenu.unity` to start the game
4. Press Play to begin

### Controls

| Action                | Control                                    |
| --------------------- | ------------------------------------------ |
| Place buildings       | Click building UI icon, then click on grid |
| Rotate buildings      | `R` key while placing                      |
| Delete mode           | Click delete button in building UI         |
| Flight mode           | `Tab` key to toggle                        |
| Open production panel | Right-click production buildings           |
| Save game             | `Ctrl+S`                                   |

## Game Guide

### Starting Resources

- 70 Space Ore
- 40 Metal Material

### Resource Types

| Resource       | Description                                  | How to Obtain                                           |
| -------------- | -------------------------------------------- | ------------------------------------------------------- |
| Space Ore      | Natural ore floating in space                | Mining Platform                                         |
| Metal Material | Refined advanced metal                       | Furnace Refinery (1 Ore -> 1 Metal, 5s)                 |
| Basic Part     | Precision machined mechanical parts          | Part Assembly (4 Metal -> 1 Part, 8s)                   |
| Advanced Part  | Top-tier material for spaceship construction | Advanced Factory (4 Basic Part -> 1 Advanced Part, 12s) |

### Buildings

| Building              | Size | Power | Cost                                     | Description                                       |
| --------------------- | ---- | ----- | ---------------------------------------- | ------------------------------------------------- |
| **Emergency Shelter** | 4x4  | 0     | Starting                                 | Core base where you begin                         |
| **Mining Platform**   | 2x2  | 10    | 20 Space Ore                             | Automatically mines nearby space ore              |
| **Nuclear Reactor**   | 3x3  | +50   | 50 Metal, 30 Basic Part, 5 Advanced Part | Massive power generation, no resource consumption |
| **Solar Array**       | 2x1  | +10   | 20 Metal Material                        | Clean infinite energy                             |
| **Storage Dock**      | 2x2  | 2     | 30 Space Ore                             | Increases storage capacity by 500                 |
| **Furnace Refinery**  | 2x2  | 15    | 40 Space Ore                             | Refines ore into metal material                   |
| **Part Assembly**     | 3x3  | 20    | 30 Metal, 20 Ore                         | Converts metal into basic parts                   |
| **Advanced Factory**  | 4x4  | 40    | 50 Metal, 30 Basic Part                  | Processes basic parts into advanced parts         |
| **Thruster**          | 2x2  | 20    | 40 Metal, 20 Basic Part                  | Propels the spaceship (rotatable)                 |
| **Basic Space Board** | 1x1  | 0     | 10 Space Ore                             | Foundation platform for expansion                 |

### Victory Condition

Collect **100 Advanced Parts** and **80 Basic Parts** to complete the interstellar spacecraft and win the game.

## Technical Architecture

### Design Patterns

- **Singleton Pattern**: All global managers (`GameManager`, `GridManager`, `AudioManager`, `PowerManager`, `SaveSystem`) use the singleton pattern for centralized access
- **Component-Based Architecture**: Buildings use composition over inheritance with attachable components (`ContainerComponent`, `Productor`, `BuildingComponent`, `PowerConsumer`)
- **Data-Driven Design**: All game data centralized in `DataConfig.cs` static class for easy balancing and extension
- **Event/Delegate System**: Loose coupling between modules through events (e.g., `BuildingPlacer` events)
- **State Machine**: Production logic in `Productor.cs` uses a state machine for idle/producing/pause transitions

### Code Organization

```
Assets/Scripts/
├── GameCore/           # Core systems (DataConfig, enums)
├── GridSystem/         # Grid management and building placement
├── PowerSystem/        # Power generation and consumption
├── ProductionSystem/   # Building base classes and production logic
├── LogisticsSystem/    # Resource storage and management
├── Resources/          # Resource entities (SpaceOre)
├── UI/                 # Building UI and interaction
└── SaveSystem.cs       # JSON serialization for save/load
```

### Key Systems

- **Sparse Dictionary Grid**: Efficient storage using `Dictionary<GridPosition, PlacedBuilding>` instead of 2D arrays
- **Cross-Scene Communication**: Static flags (`GameManager.PendingLoad`) for passing state between scenes
- **Lifecycle Management**: Careful use of Unity's deterministic message ordering (`Awake` -> `Start` -> `Update`) to ensure dependency availability
- **Backup Save System**: Automatic backup creation before overwriting save files

## Project Structure

```
SpaceWar1/
├── Assets/
│   ├── Scenes/                 # GameScene, MainMenu
│   ├── Scripts/                # All game logic
│   ├── Resources/
│   │   ├── Prefabs/            # Building and board prefabs
│   │   └── Icons/              # UI icons for buildings and resources
│   ├── Prefab/                 # Ore prefabs, UI prefabs
│   └── [Third-party assets]    # Space background, effects, fonts
├── Docs/                       # Documentation (presentation script, etc.)
└── README.md
```

## Third-Party Assets

- **Dynamic Space Background** by DinV
- **Free Quick Effects Vol1** by Gabriel Aguiar Productions
- **Bubble Font Free** by JazzCreate
- **Sci-Fi UI Skin**
- Sound effects from freesound.org community

## Development Notes

- Built as a student game project for the Game Programming course
- Focus on clean architecture, modular design, and maintainable code
- All build costs and production times are configurable in `DataConfig.cs`

## License

This project was created for educational purposes as part of a university game programming course.
