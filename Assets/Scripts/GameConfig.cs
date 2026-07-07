using UnityEngine;

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

    public const float MinSpawnInterval = 2.5f;
    public const float MaxSpawnInterval = 4.5f;
    public const float SpawnEdgeZ = 42f;
    public const float SpawnHeight = 1f;
    public const float MissileFlightDuration = 9f;
    public const float MissileArcHeight = 14f;

    public const float InterceptorSpeed = 26f;
    public const float InterceptorEngageHeight = 12f;
    public const float DomeFireCooldown = 1.25f;
    public const float InterceptHitRadius = 3f;

    public const int MaxCityHits = 10;

    public const float MinZoom = 8f;
    public const float MaxZoom = 32f;
    public const float DefaultZoom = 20f;
    public const float ScrollZoomStep = 1.5f;
    public const float PinchZoomSpeed = 0.05f;
    public const float KeyboardPanSpeed = 22f;

    public const float DomePickRadius = 4f;
    public const float MinPathPointDistance = 0.6f;
}
