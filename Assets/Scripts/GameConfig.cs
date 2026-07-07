using UnityEngine;

// Map layout and input feel constants live here.
// The gameplay/camera values below are DEFAULTS ONLY: Bootstrap exposes them as
// Inspector fields on the Bootstrap object in the Main scene and overwrites
// these statics on startup (see Bootstrap.ApplyTuning). Tune in the Inspector.
public static class GameConfig
{
    public static readonly Vector3[] CityPositions =
    {
        new Vector3(-30, 0, -15),
        new Vector3(-15, 0, -18),
        new Vector3(0, 0, -20),
        new Vector3(15, 0, -18),
        new Vector3(30, 0, -15),
    };

    public static readonly Vector3[] DomePositions =
    {
        new Vector3(-22, 0, -10),
        new Vector3(0, 0, -12),
        new Vector3(22, 0, -10),
    };

    public const float MapHalfWidth = 45f;
    public const float MapHalfDepth = 45f;

    // --- Enemy missiles (overwritten by Bootstrap Inspector values) ---
    public static float MinSpawnInterval = 6f;
    public static float MaxSpawnInterval = 10f;
    public static float SpawnDistance = 75f;
    public static float SpawnHeight = 1f;
    public static float MissileFlightDuration = 20f;
    public static float MissileArcHeight = 18f;

    // --- Interceptors (overwritten by Bootstrap Inspector values) ---
    public static float InterceptorSpeed = 26f;
    public static float InterceptorEngageHeight = 10f;
    public static float DomeFireCooldown = 1.25f;
    public static float InterceptHitRadius = 3.5f;
    public static float VerticalInterceptTolerance = 6f;
    public static float DomeRange = 28f;
    public static bool ShowRangeDomes = true;

    // --- Difficulty (overwritten by Bootstrap Inspector values) ---
    public static int MaxCityHits = 10;

    // --- Camera (overwritten by Bootstrap Inspector values) ---
    public static float MinZoom = 10f;
    public static float MaxZoom = 50f;
    public static float DefaultZoom = 26f;
    public static float ScrollZoomStep = 2f;
    public static float PinchZoomSpeed = 0.05f;
    public static float KeyboardRotateSpeed = 90f;   // degrees per second (Q/E, A/D, left/right)
    public static float DragRotateSpeed = 0.25f;     // degrees per screen pixel dragged
    public static float KeyboardTiltSpeed = 60f;     // degrees per second (W/S, up/down)
    public static float DragTiltSpeed = 0.2f;        // degrees per screen pixel dragged
    public static float DefaultPitch = 35.264f;      // classic isometric angle
    public static float MinPitch = 20f;              // shallow, near-horizon view
    public static float MaxPitch = 85f;              // near top-down view

    // --- Input feel ---
    public const float DomePickRadius = 4f;
    public const float MinPathPointDistance = 0.6f;
    public const float PathPreviewHeight = 0.3f;
}
