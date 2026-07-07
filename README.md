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

Works on **Unity 2022.3 LTS through Unity 6 (6000.x)**. Built-in Render Pipeline out of
the box; URP/HDRP also supported (materials are created at runtime via `VisualUtil.cs`,
which detects the active pipeline).

## How to open it

See **[SETUP.md](SETUP.md)** for full setup steps, including the Unity 6 upgrade notes
(Active Input Handling, URP migration), mobile build steps, and troubleshooting.
Short version: add the repo folder in Unity Hub, open with Unity 6 and accept the
upgrade, open `Assets/Scenes/Main.unity`, press Play.

## Controls

The map is fixed in place; the camera orbits around it so you can watch missiles
come in from any direction.

**PC (mouse + keyboard)**
- Click-drag starting *on* an Iron Dome battery: draws the interceptor's flight path
  on the ground. Release to fire.
- Click-drag starting on empty ground: rotates the camera around the map.
- Right-click drag: rotate (horizontal) and tilt (vertical) freely.
- Mouse scroll wheel: zoom in/out.
- Q/E, A/D or left/right arrows: rotate. W/S or up/down arrows: tilt
  (from a low angle up to a near top-down view).

**Mobile (touch)**
- One-finger drag starting on a dome: draws the interceptor's flight path.
- One-finger drag on empty ground: rotates the camera around the map.
- Two-finger pinch: zoom. Move both fingers up/down together: tilt.

## Gameplay

- Enemy missiles launch from a random compass direction, far outside the map, on a
  slow ballistic arc toward a randomly chosen city. A red ring marks the targeted city
  as soon as a missile is launched, so the threat is always visible ahead of time.
- Draw a path on the ground from any Iron Dome battery; the interceptor flies that
  path at engage altitude at a constant speed — timing and shaping the curve to meet
  the missile is the core skill (same principle as *Flight Control*'s path drawing).
- Each battery has a limited reach, shown as a translucent dome; paths can't be drawn
  past its edge, so coverage gaps between batteries are part of the strategy.
- All gameplay tuning (spawn rate, missile speed, intercept radius, camera) is exposed
  in the Inspector on the **Bootstrap** object in the Main scene.
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
    VisualUtil.cs          - pipeline-aware material creation (Built-in/URP/HDRP)
```

## Deploying to PC / mobile

Use standard Unity build targets from `File > Build Settings`:
- **PC**: Windows/Mac/Linux standalone build, or WebGL for a browser build playable on
  desktop.
- **Mobile**: switch platform to Android or iOS, build and deploy as usual. Touch input
  is already handled by `InputRouter.cs`.
