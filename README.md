# Slot Game Assignment

A 2D UI-based 3-reel classic slot machine game created in Unity using provided graphic assets and audio feedback. The project features reel spinning with sequential stopping, a center payline win evaluation system, configurable payouts, sound effects, and balance/betting controls.

## Unity Version
- **Unity 6.3 LTS (6000.3.9f1)**
- Build Target: **WebGL** (also playable in Editor / Standalone)

## Features
- **3-Reel Layout:** 3 reels displaying symbols with sequential stopping (left to right) and landing bounce.
- **RNG Outcome Generation:** Outcomes are determined before reel movement begins and visualized accurately.
- **Payline Evaluation:** Evaluates center row symbols across all 3 reels for winning combinations.
- **Configurable Payouts:** Symbol multipliers and payouts are defined via serialized data.
- **Wallet & Betting Controls:** Adjustable bet sizes (10, 50, 100 credits) with balance tracking and win payouts.
- **Sound Effects:** Audio feedback for spin button clicks, lever pulls, continuous reel spinning, individual reel stops, bet changes, wins, and jackpot payouts.
- **Win Presentation:** In-game status banner and modal popup on winning spins.
- **Responsive UI:** Built with Unity UI Canvas Scaler (1920x1080 reference resolution) to adapt across common aspect ratios.

## Symbols and Payout Table

| Symbol | Asset | Multiplier | Example Payout (at 10 Bet) |
| :---: | :--- | :---: | :---: |
| 7 | `slot-symbol1.png` | 100x | 1,000 |
| 3x BAR | `slot-symbol4.png` | 50x | 500 |
| Bell | `slot-symbol3.png` | 25x | 250 |
| Cherry | `slot-symbol2.png` | 10x | 100 |

*Payout formula:* `Payout = Current Bet × Symbol Multiplier` (Center horizontal payline).

## Controls / How to Play
1. **Choose Bet:** Click `10G`, `50G`, or `100G` to select your bet amount.
2. **Spin:** Click the **SPIN** button or the red **Lever** handle on the right.
3. **Reel Stop:** Reels spin and decelerate sequentially from left to right.
4. **Collect Winnings:** On a win, your balance updates and a victory modal appears. Click **COLLECT** to continue.

## Technologies Used
- **C# / Unity Engine** (Unity 6.3 LTS)
- **Unity UI (uGUI):** Canvas, RectTransform, RectMask2D, Button, Image
- **TextMesh Pro:** UI typography
- **WebGL:** WebAssembly export for browser playback

## Assets and Audio Credits
- **Graphics & UI:** Provided assignment slot machine art and symbol assets.
- **Sound Effects:** Free [Raw Signal: Casino & Slots LITE](https://hopefulsham.itch.io/raw-signal-casino-slots-138-synthesized-sfx) audio pack.

## Project Structure
```
Slot-Game-Assignment/
├── Assets/
│   ├── Game/
│   │   ├── Audio/                     # Sound effects (Raw Signal Casino & Slots LITE)
│   │   ├── Scene/
│   │   │   └── SlotGame_Main.unity    # Main playable slot machine scene
│   │   └── Slot Machine/              # Cabinet, lever, background, and symbol sprites
│   ├── Scripts/                       # C# gameplay scripts
│   │   ├── Editor/
│   │   │   └── SceneSetupTool.cs      # Editor tool to wire UI references
│   │   ├── AudioManager.cs            # Sound effects manager and audio playback
│   │   ├── GameManager.cs             # Session management
│   │   ├── ReelController.cs          # Reel movement, wrap-around, and deceleration
│   │   ├── SlotMachineController.cs   # Main coordinator: spin lifecycle and UI flow
│   │   ├── SymbolData.cs              # Symbol data definition and payout multipliers
│   │   ├── WalletManager.cs           # Balance, bet state, and events
│   │   └── WinEvaluator.cs            # Center payline win checks
│   └── TextMesh Pro/                  # TextMeshPro font and resource assets
├── Build/
│   └── WebGL/                         # Compiled WebGL browser build
├── Packages/                          # Package dependencies and manifest
├── ProjectSettings/                   # Unity project configuration settings
└── README.md
```

## How to Open and Run in Unity
1. Clone the repository:

```bash
git clone https://github.com/akshay-8285/Slot-Game-Assignment.git
```

2. Open **Unity Hub**.
3. Click **Add** and select the cloned folder `Slot-Game-Assignment`.
4. Open the project with **Unity 6.3 LTS (6000.3.9f1)**.
5. In the Project window, open `Assets/Game/Scene/SlotGame_Main.unity`.
6. Press the **Play** button in the Unity Editor toolbar.

## WebGL Build Instructions
1. Open **File > Build Settings...**.
2. Select **WebGL** as the platform and ensure `SlotGame_Main.unity` is checked.
3. In **Player Settings > Publishing Settings**, ensure **Decompression Fallback** is enabled if hosting on basic HTTP servers.
4. Click **Build** and specify `Build/WebGL` as the output directory.
5. To test the build locally, run an HTTP server from the build directory:

```bash
python -m http.server 8080 --directory Build/WebGL
```

Then navigate to `http://localhost:8080` in any modern web browser.
