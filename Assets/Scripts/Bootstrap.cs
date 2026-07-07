using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [Header("Enemy Missiles")]
    [Tooltip("Shortest pause between two enemy launches, in seconds.")]
    [SerializeField] float minSpawnInterval = 6f;
    [Tooltip("Longest pause between two enemy launches, in seconds.")]
    [SerializeField] float maxSpawnInterval = 10f;
    [Tooltip("How far from its target city a missile spawns. Larger = more warning time.")]
    [SerializeField] float spawnDistance = 75f;
    [Tooltip("Seconds an enemy missile takes from spawn to impact.")]
    [SerializeField] float missileFlightDuration = 20f;
    [Tooltip("Peak altitude of the ballistic arc.")]
    [SerializeField] float missileArcHeight = 18f;

    [Header("Interceptors")]
    [Tooltip("Interceptor speed in units per second along the drawn path.")]
    [SerializeField] float interceptorSpeed = 26f;
    [Tooltip("Altitude the interceptor cruises at above the drawn path.")]
    [SerializeField] float interceptorEngageHeight = 10f;
    [Tooltip("Seconds a dome must wait between launches.")]
    [SerializeField] float domeFireCooldown = 1.25f;
    [Tooltip("Horizontal distance at which an interceptor destroys an enemy missile.")]
    [SerializeField] float interceptHitRadius = 3.5f;
    [Tooltip("Max height difference still counted as a hit (proximity fuse).")]
    [SerializeField] float verticalInterceptTolerance = 6f;
    [Tooltip("Max horizontal distance a path can be drawn from its dome.")]
    [SerializeField] float domeRange = 28f;
    [Tooltip("Show a translucent dome over each battery marking its reach.")]
    [SerializeField] bool showRangeDomes = true;

    [Header("Difficulty")]
    [Tooltip("Total city hits before game over.")]
    [SerializeField] int maxCityHits = 10;

    [Header("Camera")]
    [SerializeField] float minZoom = 10f;
    [SerializeField] float maxZoom = 50f;
    [SerializeField] float defaultZoom = 26f;
    [Tooltip("Zoom change per mouse-wheel notch.")]
    [SerializeField] float scrollZoomStep = 2f;
    [Tooltip("Zoom change per pixel of pinch distance change.")]
    [SerializeField] float pinchZoomSpeed = 0.05f;
    [Tooltip("Orbit speed in degrees/second for Q/E, A/D and left/right arrows.")]
    [SerializeField] float keyboardRotateSpeed = 90f;
    [Tooltip("Orbit degrees per screen pixel when dragging on empty ground.")]
    [SerializeField] float dragRotateSpeed = 0.25f;
    [Tooltip("Tilt speed in degrees/second for W/S and up/down arrows.")]
    [SerializeField] float keyboardTiltSpeed = 60f;
    [Tooltip("Tilt degrees per screen pixel (right-mouse drag / two-finger drag).")]
    [SerializeField] float dragTiltSpeed = 0.2f;
    [Tooltip("Camera pitch at startup. 35.264 is the classic isometric angle.")]
    [SerializeField] float defaultPitch = 35.264f;
    [Tooltip("Shallowest camera pitch (low, near-horizon view).")]
    [SerializeField] float minPitch = 20f;
    [Tooltip("Steepest camera pitch (near top-down view).")]
    [SerializeField] float maxPitch = 85f;

    void Awake()
    {
        ApplyTuning();

        BuildGround();
        BuildLighting();

        var gm = gameObject.AddComponent<GameManager>();

        foreach (var pos in GameConfig.CityPositions)
        {
            var cityGo = new GameObject("City");
            cityGo.transform.position = pos;
            var city = cityGo.AddComponent<City>();
            gm.RegisterCity(city);
        }

        foreach (var pos in GameConfig.DomePositions)
        {
            var domeGo = new GameObject("IronDome");
            domeGo.transform.position = pos;
            var dome = domeGo.AddComponent<IronDome>();
            gm.RegisterDome(dome);
        }

        SetupCameraRig();

        gameObject.AddComponent<HUDController>();
    }

    void ApplyTuning()
    {
        GameConfig.MinSpawnInterval = minSpawnInterval;
        GameConfig.MaxSpawnInterval = maxSpawnInterval;
        GameConfig.SpawnDistance = spawnDistance;
        GameConfig.MissileFlightDuration = missileFlightDuration;
        GameConfig.MissileArcHeight = missileArcHeight;

        GameConfig.InterceptorSpeed = interceptorSpeed;
        GameConfig.InterceptorEngageHeight = interceptorEngageHeight;
        GameConfig.DomeFireCooldown = domeFireCooldown;
        GameConfig.InterceptHitRadius = interceptHitRadius;
        GameConfig.VerticalInterceptTolerance = verticalInterceptTolerance;
        GameConfig.DomeRange = domeRange;
        GameConfig.ShowRangeDomes = showRangeDomes;

        GameConfig.MaxCityHits = maxCityHits;

        GameConfig.MinZoom = minZoom;
        GameConfig.MaxZoom = maxZoom;
        GameConfig.DefaultZoom = defaultZoom;
        GameConfig.ScrollZoomStep = scrollZoomStep;
        GameConfig.PinchZoomSpeed = pinchZoomSpeed;
        GameConfig.KeyboardRotateSpeed = keyboardRotateSpeed;
        GameConfig.DragRotateSpeed = dragRotateSpeed;
        GameConfig.KeyboardTiltSpeed = keyboardTiltSpeed;
        GameConfig.DragTiltSpeed = dragTiltSpeed;
        GameConfig.DefaultPitch = defaultPitch;
        GameConfig.MinPitch = minPitch;
        GameConfig.MaxPitch = maxPitch;
    }

    void BuildGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(
            GameConfig.MapHalfWidth * 2f / 10f, 1f, GameConfig.MapHalfDepth * 2f / 10f);

        ground.GetComponent<MeshRenderer>().material = VisualUtil.NewLitMaterial(new Color(0.65f, 0.6f, 0.45f));
    }

    void BuildLighting()
    {
        RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);
    }

    void SetupCameraRig()
    {
        var mainCam = Camera.main;
        if (mainCam == null) return;

        // The rig sits at the map center and only ever yaws; the camera hangs
        // off it at an adjustable pitch, so the map stays put and the camera
        // orbits around it. CameraRig.Apply() positions the camera from
        // yaw/pitch once the component is added.
        var rigGo = new GameObject("CameraRig");
        rigGo.transform.position = Vector3.zero;

        mainCam.transform.SetParent(rigGo.transform, false);
        mainCam.orthographic = true;
        mainCam.nearClipPlane = 1f;
        mainCam.farClipPlane = 300f;

        rigGo.AddComponent<CameraRig>();
        rigGo.AddComponent<InputRouter>();
    }
}
