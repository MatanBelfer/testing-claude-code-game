# Setup Guide

Step-by-step setup for opening, running, and building this project on the latest Unity.
The repo was authored against 2022.3 conventions but is written to survive an upgrade to
Unity 6 (6000.x) — the notes below cover exactly what to check after the upgrade.

## 1. Prerequisites

- **Unity Hub** (latest).
- **Unity 6 LTS (6000.x)** via the Hub, with these modules as needed:
  - *Android Build Support* (+ OpenJDK, Android SDK & NDK) for Android.
  - *iOS Build Support* for iOS (macOS + Xcode required).
  - *WebGL Build Support* if you want a browser build.

## 2. Open the project

1. Clone the repo and check out the branch.
2. In Unity Hub: **Add → Add project from disk** → select the repo root.
3. Open it with your Unity 6 editor. `ProjectVersion.txt` pins 2022.3.21f1, so the Hub
   will warn about a version change — confirm the upgrade. Let the API updater run if
   it prompts. First import takes a couple of minutes.
4. Open `Assets/Scenes/Main.unity` and press **Play**.

Expected on Play: a tan ground plane, 5 cities (clusters of boxes), 3 dome batteries,
and a HUD (Score / Hits). The first enemy missile spawns within ~2.5–4.5 s with a red
ring marking its target city.

## 3. Settings to verify after the upgrade

These are the two things most likely to differ on a "latest everything" install:

### Active Input Handling — Input System Package (New)
The project uses the **new Input System** (`com.unity.inputsystem`, declared in
`Packages/manifest.json`) and ships with
`Player → Other Settings → Active Input Handling` set to
**"Input System Package (New)"**. "Both" also works; "Input Manager (Old)" alone will
break all input, since `InputRouter.cs` / `CameraRig.cs` read `Mouse.current`,
`Keyboard.current`, and EnhancedTouch, and the UI uses `InputSystemUIInputModule`.

No Input Actions asset is required — the code reads devices directly
(`Mouse.current`, `Keyboard.current`, `Touch.activeTouches`) and the UI module
assigns its default actions at runtime. If you later want rebindable controls,
introduce an actions asset and route `InputRouter` through it.

### Render pipeline — Built-in works out of the box, URP is supported
The project ships on the **Built-in Render Pipeline** and runs as-is.

If you migrate to **URP** (recommended for mobile): install the URP package, create a
URP Asset (`Assets → Create → Rendering → URP Asset (with Universal Renderer)`), and
assign it under `Project Settings → Graphics` (and in Quality levels). No code changes
needed — all materials are created at runtime through `VisualUtil.cs`, which detects
the active pipeline and picks `Universal Render Pipeline/Lit` (or `HDRP/Lit`, or
`Standard`) automatically. If you ever see magenta objects, the assigned pipeline asset
and installed pipeline package don't match.

## 4. Building

### PC (Windows/macOS/Linux)
`File → Build Profiles` (Unity 6) → Windows/Mac/Linux → Build. No special settings.

### Android
1. Switch platform to Android.
2. `Player Settings → Other Settings`: set a package name; for release builds use
   **IL2CPP** scripting backend + **ARM64** target architecture.
3. Default orientation is auto-rotate (set in Player settings if you want
   landscape-only — the map layout favors landscape).
4. Build & Run on a device. Sanity check: one-finger drag from a dome draws a green
   path; two-finger pinch zooms; drag on empty ground pans.

### iOS
1. Switch platform to iOS, Build → open the generated Xcode project.
2. Set your signing team in Xcode, deploy to device.

### WebGL (optional, easy way to share a PC-playable build)
Switch platform to WebGL and build; host the output folder on any static server.

## 5. Tuning the game

Select the **Bootstrap** object in `Assets/Scenes/Main.unity` — all gameplay and
camera tuning is exposed there as Inspector fields with tooltips, grouped under
*Enemy Missiles*, *Interceptors*, *Difficulty*, and *Camera*. Values you set in the
Inspector are copied into the static `GameConfig` on startup and override the code
defaults (`Bootstrap.ApplyTuning`). You can tweak them live in Play mode, but as with
any Inspector value, Play-mode edits revert on exit.

| Field | Meaning |
|---|---|
| Min/Max Spawn Interval | Seconds between enemy launches (flat difficulty) |
| Spawn Distance | How far from its target a missile spawns — more distance = more warning |
| Missile Flight Duration | Seconds from spawn to impact (higher = slower missiles) |
| Missile Arc Height | Peak altitude of the ballistic arc |
| Interceptor Speed | How fast your interceptor traces the drawn path |
| Interceptor Engage Height | Altitude the interceptor cruises at above your drawn path |
| Dome Fire Cooldown | Per-dome cooldown between shots |
| Intercept Hit Radius / Vertical Intercept Tolerance | Proximity-fuse size (horizontal / vertical) |
| Dome Range / Show Range Domes | Max path-drawing reach per battery, and its translucent dome marker |
| Max City Hits | Total city hits before game over |
| Min/Max/Default Zoom, Scroll Zoom Step, Pinch Zoom Speed | Camera zoom feel |
| Keyboard/Drag Rotate Speed | Orbit speed for keys and for dragging on empty ground |
| Keyboard/Drag Tilt Speed, Default/Min/Max Pitch | Camera tilt feel and limits |

Map layout (`CityPositions` / `DomePositions`, ground size) still lives in
`Assets/Scripts/GameConfig.cs`. Input-feel constants (`DomePickRadius`,
`MinPathPointDistance`) are there too.

## 6. Architecture notes (for making changes in the editor)

- The scene contains only **Main Camera, Directional Light, and a `Bootstrap`
  GameObject**. `Bootstrap.Awake()` builds everything else procedurally: ground,
  cities, domes, camera rig + input, HUD. This was done because the project was
  authored without an editor available — it's a natural place to start replacing
  procedural primitives with real prefabs/art. To convert: create your prefabs, add
  serialized fields on `Bootstrap`, and swap the `CreatePrimitive` calls.
- UI is legacy uGUI `Text` built at runtime (`HUDController.cs`) using
  `LegacyRuntime.ttf`. Swap to TextMeshPro when you bring in real UI.
- Intercept detection is a simple distance check each frame
  (`Interceptor.CheckIntercepts`) — no physics/colliders involved; all primitive
  colliders are destroyed at creation.
- No audio, no pooling, no save state yet — intentional placeholders.

## 7. Troubleshooting

| Symptom | Cause / fix |
|---|---|
| No input at all (mouse, keyboard, touch) | Active Input Handling is "Input Manager (Old)" — set to "Input System Package (New)" or "Both" (see §3) |
| Compile errors in `UnityEngine.InputSystem` namespace | The Input System package failed to resolve — check `com.unity.inputsystem` in the Package Manager |
| Everything magenta | Graphics settings point at a pipeline asset whose package isn't installed |
| HUD buttons don't respond | The runtime-created `EventSystem` needs `InputSystemUIInputModule` with actions — `HUDController` assigns defaults; check the Console for Input System warnings |
| Console compile errors on first open | Paste them back to me — the project was never compiled before commit, so a stray C# error is possible and trivially fixable from the message |
