# 🎮 Null Command RTS

A real-time strategy game prototype with placeholder graphics, built as a foundation for full art asset implementation.

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![.NET](https://img.shields.io/badge/.NET-6.0--windows-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

## 🎯 Game Overview

Null Command is a minimalist RTS game where players build a hospital base, spawn soldier units, and defend against enemy waves. The current version uses colored placeholder squares for all visual elements.

## 🎮 Gameplay Features

### Core Mechanics
- **Hospital Construction**: Shift+LMB on empty screen to place hospital
- **Soldier Spawning**: Shift+LMB on hospital to spawn soldiers (2-second cooldown)
- **Unit Selection**: Shift+LMB on soldiers to select individual/squad
- **Movement Orders**: LMB to command selected units to move
- **Boss Mode**: F12 to hide/show entire game (workplace stealth mode)

### Game Systems
- **Squad Formation**: Soldiers automatically group into squads of 5
- **Health Regeneration**: Units within 256 pixels of hospital regenerate health
- **Enemy AI**: Enemies spawn at opposite screen edges, move toward hospital
- **Combat System**: Automatic firing when units are in range
- **Guard Behavior**: Soldiers return to guard positions after combat

## 🏗️ Technical Architecture

### Built With
- **Framework**: WPF (Windows Presentation Foundation)
- **Target**: .NET 6.0-windows
- **Input Handling**: Global mouse/keyboard hooks
- **Rendering**: Canvas-based with placeholder Rectangle shapes

### Project Structure
```
null-command/
├── NullCommand.csproj      # Project configuration
├── MainWindow.xaml         # Window layout
├── NullCommandGame.cs      # Main game logic
└── README.md              # This documentation
```

### Core Classes
- **Hospital**: Building with construction/idle states, soldier spawning
- **Soldier**: Unit with AI states (Guard, Moving, Seeking, Combat)
- **Squad**: Formation system for coordinated unit movement
- **GameEntity**: Base class for all game objects

## 🎨 Current Visual Design

### Placeholder Assets
- **Hospital**: 64x64 green rectangle with dark green border
- **Player Soldiers**: 16x16 red rectangles with dark red border
- **Enemy Soldiers**: 16x16 blue rectangles with dark blue border
- **Selection Indicator**: Yellow border on selected units

### Animation System
- **Hospital**: 9-frame construction → 4-frame idle animation (placeholder timing)
- **Units**: Position interpolation for smooth movement
- **Frame Rate**: 60 FPS game loop with 16ms intervals

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- .NET 6.0 Runtime (Windows)

### Building from Source
```bash
# Navigate to project directory
cd null-command

# Build the project
dotnet build NullCommand.csproj

# Run the application
dotnet run
```

### Controls
| Input | Action |
|-------|--------|
| Shift + LMB | Place hospital (if none exists) |
| Shift + LMB | Spawn soldier (on hospital) |
| Shift + LMB | Select soldier (on unit) |
| LMB | Move selected unit/squad |
| F12 | Toggle boss mode |

## 📋 Game Balance

### Unit Statistics
- **Hospital**: 500 HP, 256px healing range, 10 HP/sec regeneration
- **Soldiers**: 80 HP, 80px attack range, 120px detection range
- **Combat**: 15 damage per shot, 1-second fire cooldown
- **Movement**: 2.0 pixels/frame base speed

### Spawn Mechanics
- **Soldier Cooldown**: 2 seconds between hospital spawns
- **Enemy Spawning**: 1 second cooldown, groups of 3 units
- **Squad Size**: Maximum 5 soldiers per squad

---

# 🎨 ART & DEVELOPMENT HANDOFF DOCUMENT

## Asset Requirements Overview

The following document outlines all visual assets needed to replace the current placeholder graphics with final artwork.

## 🏥 Building Assets

### Hospital
**File Format**: PNG with transparency
**Dimensions**: 64x64 pixels
**Required Animations**:

1. **Construction Animation**
   - **Files**: `hospital_build_01.png` through `hospital_build_09.png`
   - **Frame Count**: 9 frames
   - **Duration**: ~1.2 seconds total (8 ticks per frame at 60 FPS)
   - **Description**: Building construction from foundation to completion

2. **Idle Animation**
   - **Files**: `hospital_idle_01.png` through `hospital_idle_04.png`
   - **Frame Count**: 4 frames
   - **Duration**: Loops continuously (8 ticks per frame at 60 FPS)
   - **Description**: Subtle ambient animation (lights, smoke, etc.)

**Visual Guidelines**:
- Medical/military aesthetic
- Clear distinction from background
- Visible from top-down perspective
- Green color scheme preferred (matches current placeholder)

## 👥 Unit Assets

### Player Soldiers
**File Format**: PNG with transparency
**Dimensions**: 16x16 pixels
**Required States**:

1. **Idle Animation**
   - **Files**: `soldier_idle_01.png` through `soldier_idle_04.png`
   - **Frame Count**: 4 frames
   - **Description**: Standing guard animation

2. **Moving Animation**
   - **Files**: `soldier_move_01.png` through `soldier_move_06.png`
   - **Frame Count**: 6 frames
   - **Description**: Walking/running cycle

3. **Combat Animation**
   - **Files**: `soldier_combat_01.png` through `soldier_combat_04.png`
   - **Frame Count**: 4 frames
   - **Description**: Firing weapon animation

**Visual Guidelines**:
- Red color scheme (matches current placeholder)
- Clear silhouette at small size
- Distinguishable from enemy units
- Military/tactical appearance

### Enemy Soldiers
**File Format**: PNG with transparency
**Dimensions**: 16x16 pixels
**Required States**: Same as Player Soldiers

**Visual Guidelines**:
- Blue color scheme (matches current placeholder)
- Same animation set as player soldiers
- Clearly hostile/opposing faction design
- Distinct from player units

## 🎯 UI & Effect Assets

### Selection Indicators
**File Format**: PNG with transparency
**Dimensions**: Variable (overlay on units)

1. **Unit Selection**
   - **File**: `selection_ring.png`
   - **Description**: Highlight for selected individual units
   - **Color**: Yellow/Gold

2. **Squad Selection**
   - **File**: `squad_selection.png`
   - **Description**: Group selection indicator
   - **Color**: Yellow/Gold variations

### Combat Effects
**File Format**: PNG with transparency
**Dimensions**: 8x8 to 16x16 pixels

1. **Muzzle Flash**
   - **Files**: `muzzle_flash_01.png` through `muzzle_flash_03.png`
   - **Frame Count**: 3 frames
   - **Description**: Weapon firing effect

2. **Impact Effect**
   - **Files**: `impact_01.png` through `impact_03.png`
   - **Frame Count**: 3 frames
   - **Description**: Projectile hit effect

## 🔧 Technical Specifications

### File Requirements
- **Format**: PNG-24 with alpha transparency
- **Color Profile**: sRGB
- **Naming Convention**: `entity_state_##.png` (zero-padded frame numbers)
- **Directory Structure**:
  ```
  Graphics/
  ├── Buildings/
  │   ├── hospital_build_01.png → hospital_build_09.png
  │   └── hospital_idle_01.png → hospital_idle_04.png
  ├── Units/
  │   ├── soldier_idle_01.png → soldier_idle_04.png
  │   ├── soldier_move_01.png → soldier_move_06.png
  │   ├── soldier_combat_01.png → soldier_combat_04.png
  │   ├── enemy_idle_01.png → enemy_idle_04.png
  │   ├── enemy_move_01.png → enemy_move_06.png
  │   └── enemy_combat_01.png → enemy_combat_04.png
  ├── UI/
  │   ├── selection_ring.png
  │   └── squad_selection.png
  └── Effects/
      ├── muzzle_flash_01.png → muzzle_flash_03.png
      └── impact_01.png → impact_03.png
  ```

### Animation Timing
- **Frame Rate**: All animations play at 12 FPS (5 engine ticks per frame)
- **Looping**: Idle animations loop continuously
- **One-Shot**: Construction, combat effects play once

### Integration Notes
- Assets will be loaded via `LoadBitmapFromFile()` method
- Animation frame switching handled automatically
- Fallback system creates placeholder rectangles if files missing
- No code changes required for asset integration

## 🎨 Style Guidelines

### Overall Aesthetic
- **Genre**: Military/Medical RTS
- **Perspective**: Top-down isometric view
- **Color Palette**: 
  - Player: Red/Crimson tones
  - Enemy: Blue/Navy tones
  - Hospital: Green/Medical tones
  - UI: Yellow/Gold accents

### Visual Clarity
- High contrast for visibility at small sizes
- Clear faction identification through color coding
- Readable silhouettes for gameplay recognition
- Consistent lighting direction across all assets

## 📦 Delivery Requirements

### Asset Package
1. All PNG files organized in specified directory structure
2. Reference sheet showing all animations
3. Color palette guide
4. Any custom fonts used (if applicable)

### Quality Assurance
- All assets tested at target resolution (16x16, 64x64)
- Transparency edges clean and anti-aliased
- Consistent style across all game elements
- Frame timing verified for smooth animation

---

## 🔄 Development Status

### Completed Features ✅
- Hospital placement and spawning system
- Soldier AI with squad formation
- Enemy spawn and combat mechanics
- Health regeneration system
- Boss mode functionality
- Global input handling

### Ready for Art Integration 🎨
- Asset loading system implemented
- Animation framework in place
- Placeholder graphics clearly defined
- File structure prepared

### Future Enhancements 🚀
- Sound effects integration
- Additional unit types
- Building upgrades
- Save/load game state
- Multiplayer networking

---

*This prototype provides a solid foundation for the full Null Command RTS experience. All systems are functional and ready for visual enhancement through the art asset pipeline.*
