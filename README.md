# IT-Waves: Horror Snake Game

## Project Structure

```
Assets/
  ├── Audio/              # Sound effects and music
  ├── Materials/          # URP 2D materials
  ├── Prefabs/            # Reusable game objects
  ├── Scenes/             # Game scenes (Boot, MainMenu, Game, Win, GameOver)
  ├── Scripts/
  │   ├── Core/           # Core systems and utilities (GameLoader, GameManager)
  │   ├── Player/         # Player controller, shooting, health, dash
  │   ├── Snake/          # Main antagonist with wave motion
  │   ├── Enemies/        # Additional enemy types (Crawler, Skitterer)
  │   ├── Props/          # Destructible boxes and obstacles
  │   ├── Level/          # Level management, difficulty, procedural generation
  │   ├── Systems/        # Combat, pooling, save/load
  │   ├── UI/             # HUD, menus, screens
  │   ├── Managers/       # Existing managers (LevelManager)
  │   └── Editor/         # Editor tools
  ├── Settings/           # ScriptableObject assets (LevelConfig, DifficultyProfile, SnakeConfig)
  ├── Lighting/           # URP 2D lighting presets
  └── Fonts/              # TextMeshPro fonts
```

## Game Overview

A retro horror game where you hunt a single snake-like monstrosity across 20 procedurally generated levels. The snake escapes when damaged enough (levels 1-19), forcing you to chase it through increasingly difficult arenas. On Level 20, you finally defeat it.

## Controls

**The player is stationary at the center of the screen, frozen in fear. You can only aim and shoot.**

### Keyboard & Mouse
- **Aim**: Mouse
- **Fire**: Left Mouse Button
- **Pause**: Escape

### Gamepad
- **Aim**: Right Stick
- **Fire**: RT
- **Pause**: Start

## Core Systems

### Level Progression
- 20 levels total
- Difficulty scales via `LevelDifficultyProfile` curves
- Procedural arena generation with destructible boxes
- Snake escapes at low health (levels 1-19)
- Final defeat on Level 20 triggers victory

### Snake Mechanics
- Wave/sinusoidal movement pattern
- Segments follow head in chain
- Shared health pool across segments
- Spawns boxes when hit (Centipede homage)
- Behaviours: Patrol, Chase, Enrage, Escape

### Player Systems
- Top-down omnidirectional movement
- Aim-based shooting
- Dash with invulnerability frames
- Health with i-frames on hit

### Additional Enemies
- **Crawler**: Burst movement, leaves slow puddles
- **Skitterer**: Zig-zag wander, periodic charges

## Swapping Placeholder Art

All placeholder art uses Unity 2D shapes (Capsules, Circles, Squares). To replace with final sprites:

1. **Import sprites** into `ArtPlaceholders/` with appropriate import settings:
   - Texture Type: Sprite (2D and UI)
   - Pixels Per Unit: 16 (or your chosen PPU)
   - Filter Mode: Point (no filter) for crisp pixels
   - Compression: None

2. **Update prefabs**:
   - Open prefab in Prefab Mode
   - Replace SpriteRenderer sprite reference
   - Adjust collider sizes if needed
   - Update sorting layer if required

3. **Lighting**: 2D lights are already configured. Add normal maps to sprites for enhanced lighting effects.

## Build Settings

- **Platform**: WebGL (primary target)
- **Scenes**: Boot → MainMenu → Game → Win/GameOver
- **URP**: 2D Renderer with global dim lighting
- **Physics2D**: Zero gravity (top-down)

## Assembly Definitions

- **ITWaves.Runtime**: Main game code
- **ITWaves.Editor**: Editor-only tools

## Tags & Layers

### Tags
- Player
- Snake
- Enemy
- Box
- Bullet

### Layers
- Player
- Enemy
- Snake
- Props
- Projectiles
- FX
- UI
- Default

### Sorting Layers (back to front)
- Background
- Props
- Enemies
- Snake
- Player
- FX
- UI

## Save System

Saves to `Application.persistentDataPath` with JSON + HMAC integrity check:
- Highest level reached (for Continue)
- Volume settings
- Basic options

## Development Notes

- Unity 6, .NET Standard 2.1
- New Input System (PlayerInput component)
- URP 2D with 2D Lights
- Deterministic procedural generation using seeded System.Random
- Object pooling for bullets, enemies, FX
- Event-driven architecture for state transitions

