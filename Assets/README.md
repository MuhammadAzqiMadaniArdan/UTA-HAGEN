# Unity Stealth Game Project

## Game Concept
First-person stealth game dengan sistem event, dialog choices, dan multiple endings. Player harus melakukan transaksi keuangan secara diam-diam sambil menghindari deteksi dari NPC dan CCTV.

## Core Systems

### 1. Game Manager
- Mengelola game state (Playing, Paused, Dialog, Event, GameOver)
- Tracking greed level dan suspicion level
- Sistem multiple endings berdasarkan player choices
- Story progression tracking

### 2. Event System
- Random event generation dengan cooldown
- Event types: Transaction, Stealth, Dialog, Puzzle
- Timer-based events dengan reward/penalty system
- Player response evaluation

### 3. First Person Controller
- Standard FPS movement dengan stealth mechanics
- Crouching system untuk mengurangi noise
- Noise generation system yang mempengaruhi detection
- Mouse look dengan sensitivity settings

### 4. NPC AI System
- Patrol behavior dengan waypoints
- Player detection dengan field of view
- Line of sight checking dengan raycasting
- Suspicion system ketika player terdeteksi

### 5. Dialog System
- Branching dialog dengan multiple choices
- Choice effects pada greed dan suspicion levels
- Dynamic dialog flow dengan node-based system
- UI integration dengan button generation

### 6. Stealth System
- CCTV camera detection dengan field of view
- Hiding spots system
- Player visibility tracking
- Integration dengan suspicion system

### 7. Game UI
- HUD dengan greed dan suspicion bars
- Laptop interface untuk money transfer
- Event notification system
- Emergency laptop closing (ESC key)

### 8. Save System
- JSON-based save/load functionality
- Persistent unlocked endings tracking
- Game state preservation

## Setup Instructions

1. **Create Unity Project**
   - Unity 2022.3 LTS atau lebih baru
   - 3D Template

2. **Import Required Packages**
   - TextMeshPro
   - NavMesh Components (Window > Package Manager)

3. **Setup Scene**
   - Create Player GameObject dengan FirstPersonController script
   - Add Camera sebagai child dari Player
   - Setup NavMesh untuk NPC patrolling
   - Create UI Canvas dengan GameUI script

4. **Configure Components**
   - Assign semua references di inspector
   - Setup patrol points untuk NPCs
   - Configure CCTV cameras dan hiding spots
   - Setup dialog data dan event lists

## Key Features

- **Stealth Gameplay**: Hindari NPC dan CCTV detection
- **Money Transfer System**: Laptop interface untuk transaksi
- **Dynamic Events**: Random events yang membutuhkan player response
- **Multiple Endings**: Berdasarkan greed dan suspicion levels
- **Dialog Choices**: Mempengaruhi story progression
- **Save/Load System**: Persistent game state

## Controls

- WASD: Movement
- Mouse: Look around
- Shift: Run (increases noise)
- C: Crouch (decreases noise)
- L: Open/Close Laptop
- ESC: Emergency close laptop
- Space: Jump

