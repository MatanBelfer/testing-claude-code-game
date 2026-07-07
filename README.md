# Iron Dome Defender

A 3D isometric defense game inspired by *Flight Control*, reimagined as an Iron Dome
interception game. Draw a flight path from an Iron Dome battery to intercept incoming
ballistic missiles before they hit your cities.

## ⚠️ Important: how this project was built

This project was authored entirely by hand (C# scripts + a hand-written Unity `.unity`
scene, `ProjectSettings`, and `Packages/manifest.json`) in an environment **without the
Unity Editor or a .NET/Mono runtime installed**. That means none of this was compiled or
run before being committed — there is a real chance the Editor reports an error the
first time you open it (typically a C# compile error surfaced in the Console).

To keep risk as low as possible, the project is built so that **only one object is
placed in the scene by hand** (a `Bootstrap` component on a single GameObject, plus the
Main Camera and a Directional Light). Every other GameObject — ground, cities, Iron Dome
batteries, missiles, interceptors, UI — is constructed procedurally in code at runtime
via `Bootstrap.cs`. This avoids the most fragile part of hand-written Unity projects
(serialized scene hierarchies/prefabs with cross-referenced GUIDs) and means that if
something is wrong, it will almost certainly be a plain C# compiler error, not a broken
scene file.

**If you see errors in the Console when you first open the project, paste them back and
they can be fixed** — a C# compile error is easy to diagnose and fix from the error text
alone, even without the ability to run the Editor directly.

Recommended Editor version: **Unity 2022.3 LTS** (any 2022.3.x patch will prompt to
"open with this Editor" — that's fine). Built-in Render Pipeline (no URP/HDRP needed).

## How to open it

1. Install Unity Hub + Unity 2022.3 LTS.
2. In Unity Hub, "Add" this repository folder as a project, then open it.
3. Open `Assets/Scenes/Main.unity` if it doesn't open automatically.
4. Press Play.

## Controls

**PC (mouse + keyboard)**
- Click-drag starting *on* an Iron Dome battery: draws the interceptor's flight path.
  Release to fire.
- Click-drag starting on empty ground: pans the camera.
- Mouse scroll wheel: zoom in/out.
- WASD / arrow keys: pan the camera.

**Mobile (touch)**
- One-finger drag starting on a dome: draws the interceptor's flight path.
- One-finger drag on empty ground: pans the camera.
- Two-finger pinch: zoom in/out.

## Gameplay

- Enemy missiles spawn from the edge of the map on a fixed ballistic arc toward a
  randomly chosen city. A red ring marks the targeted city as soon as a missile is
  launched, so the threat is always visible ahead of time.
- Draw a path from any Iron Dome battery to intercept a missile. The interceptor flies
  exactly the path you draw at a constant speed — timing and shaping the curve to meet
  the missile is the core skill (same principle as *Flight Control*'s path drawing).
- Each dome can fire again after a short cooldown (no ammo limit).
- The game ends when cities have been hit a total of `GameConfig.MaxCityHits` times
  (10 by default). Score increases with each successful intercept.
- Difficulty is intentionally flat/stable — spawn rate and missile speed do not scale up
  over time.

## Tuning

All gameplay constants (city/dome positions, spawn timing, missile/interceptor speed,
zoom limits, hit limit, etc.) live in `Assets/Scripts/GameConfig.cs`.

## Project layout

```
Assets/
  Scenes/Main.unity       - minimal scene: Main Camera, Directional Light, Bootstrap
  Scripts/
    GameConfig.cs         - tunable constants
    Bootstrap.cs          - builds the entire world procedurally at runtime
    GameManager.cs        - spawning, score, hit-count/game-over state
    CameraRig.cs           - isometric camera pan/zoom
    InputRouter.cs         - unified mouse/touch input (draw path vs. pan vs. pinch-zoom)
    IronDome.cs            - dome visuals, cooldown, path preview, firing
    Interceptor.cs         - interceptor missile movement + hit detection
    EnemyMissile.cs        - ballistic arc, target warning ring, impact
    City.cs                - building visuals, damage feedback
    HUDController.cs       - runtime-built UGUI HUD (score, hits, game over/restart)
    ExplosionFX.cs         - simple scale/fade explosion effect
```

## Deploying to PC / mobile

Use standard Unity build targets from `File > Build Settings`:
- **PC**: Windows/Mac/Linux standalone build, or WebGL for a browser build playable on
  desktop.
- **Mobile**: switch platform to Android or iOS, build and deploy as usual. Touch input
  is already handled by `InputRouter.cs`.
