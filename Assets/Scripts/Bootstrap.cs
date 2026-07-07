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
    [Tooltip("Orbit speed in degrees/second for Q/E and arrow keys.")]
    [SerializeField] float keyboardRotateSpeed = 90f;
    [Tooltip("Orbit degrees per screen pixel when dragging on empty ground.")]
    [SerializeField] float dragRotateSpeed = 0.25f;

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

        GameConfig.MaxCityHits = maxCityHits;

        GameConfig.MinZoom = minZoom;
        GameConfig.MaxZoom = maxZoom;
        GameConfig.DefaultZoom = defaultZoom;
        GameConfig.ScrollZoomStep = scrollZoomStep;
        GameConfig.PinchZoomSpeed = pinchZoomSpeed;
        GameConfig.KeyboardRotateSpeed = keyboardRotateSpeed;
        GameConfig.DragRotateSpeed = dragRotateSpeed;
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
        // off it at a fixed isometric pitch, so the map stays put and the
        // camera orbits around it.
        var rigGo = new GameObject("CameraRig");
        rigGo.transform.position = Vector3.zero;

        Transform camTransform = mainCam.transform;
        camTransform.SetParent(rigGo.transform, false);
        Quaternion pitch = Quaternion.Euler(35.264f, 0f, 0f);
        camTransform.localRotation = pitch;
        camTransform.localPosition = -(pitch * Vector3.forward) * 80f;
        mainCam.orthographic = true;
        mainCam.orthographicSize = GameConfig.DefaultZoom;
        mainCam.nearClipPlane = 1f;
        mainCam.farClipPlane = 300f;

        rigGo.AddComponent<CameraRig>();
        rigGo.AddComponent<InputRouter>();
    }
}
