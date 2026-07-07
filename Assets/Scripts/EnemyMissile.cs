using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    public Vector3 StartPos { get; private set; }
    public Vector3 TargetPos { get; private set; }
    public City TargetCity { get; private set; }
    public bool IsResolved => resolved;

    float elapsed;
    float flightDuration;
    bool resolved;
    GameObject warningRing;
    GameObject shadow;
    LineRenderer trackLine;

    public void Init(Vector3 start, City targetCity)
    {
        StartPos = start;
        TargetCity = targetCity;
        TargetPos = targetCity.transform.position;
        flightDuration = GameConfig.MissileFlightDuration;
        transform.position = start;
        BuildVisual();
        SpawnWarningRing();
        SpawnGroundTrack();
    }

    void BuildVisual()
    {
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(transform, false);
        body.transform.localScale = new Vector3(0.6f, 1.4f, 0.6f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<MeshRenderer>().material = VisualUtil.NewLitMaterial(new Color(0.75f, 0.15f, 0.1f));

        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 1.2f;
        trail.startWidth = 0.4f;
        trail.endWidth = 0.05f;
        trail.material = VisualUtil.NewUnlitTransparentMaterial(Color.white);
        trail.startColor = new Color(1f, 0.5f, 0.2f, 0.8f);
        trail.endColor = new Color(0.4f, 0.4f, 0.4f, 0f);
    }

    void SpawnWarningRing()
    {
        warningRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(warningRing.GetComponent<Collider>());
        warningRing.transform.position = TargetPos + Vector3.up * 0.05f;
        warningRing.transform.localScale = new Vector3(6f, 0.02f, 6f);
        warningRing.GetComponent<MeshRenderer>().material =
            VisualUtil.NewUnlitTransparentMaterial(new Color(1f, 0.1f, 0.1f, 0.5f));
    }

    // The intercept check is purely horizontal, so the map plane is where the
    // player reasons: a shadow disc marks the spot directly beneath the
    // missile, and a faded line shows the rest of its ground track to the
    // target. Draw an interceptor path across the line ahead of the shadow.
    void SpawnGroundTrack()
    {
        shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shadow.name = "MissileShadow";
        Destroy(shadow.GetComponent<Collider>());
        shadow.transform.localScale = new Vector3(1.6f, 0.02f, 1.6f);
        shadow.GetComponent<MeshRenderer>().material =
            VisualUtil.NewUnlitTransparentMaterial(new Color(0.9f, 0.1f, 0.1f, 0.65f));

        var trackGo = new GameObject("MissileGroundTrack");
        trackLine = trackGo.AddComponent<LineRenderer>();
        trackLine.material = VisualUtil.NewUnlitTransparentMaterial(Color.white);
        trackLine.startColor = new Color(1f, 0.25f, 0.15f, 0.55f);
        trackLine.endColor = new Color(1f, 0.25f, 0.15f, 0.2f);
        trackLine.startWidth = trackLine.endWidth = 0.35f;
        trackLine.positionCount = 2;
        trackLine.useWorldSpace = true;
        UpdateGroundTrack(StartPos);
    }

    void UpdateGroundTrack(Vector3 missilePos)
    {
        Vector3 flat = new Vector3(missilePos.x, 0.08f, missilePos.z);
        shadow.transform.position = flat;
        trackLine.SetPosition(0, flat);
        trackLine.SetPosition(1, new Vector3(TargetPos.x, 0.08f, TargetPos.z));
    }

    void Update()
    {
        if (resolved) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / flightDuration);

        Vector3 prev = transform.position;
        Vector3 flat = Vector3.Lerp(StartPos, TargetPos, t);
        float arc = Mathf.Sin(t * Mathf.PI) * GameConfig.MissileArcHeight;
        Vector3 next = flat + Vector3.up * arc;

        Vector3 dir = next - prev;
        transform.position = next;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(90, 0, 0);

        UpdateGroundTrack(next);

        if (t >= 1f)
            Impact();
    }

    void Impact()
    {
        if (resolved) return;
        resolved = true;
        TargetCity.TakeHit();
        GameManager.Instance.OnCityHit();
        ExplosionFX.Spawn(transform.position, new Color(1f, 0.4f, 0.1f));
        Cleanup();
    }

    public void InterceptedAt(Vector3 pos)
    {
        if (resolved) return;
        resolved = true;
        GameManager.Instance.OnMissileIntercepted();
        ExplosionFX.Spawn(pos, Color.white);
        Cleanup();
    }

    void Cleanup()
    {
        if (warningRing) Destroy(warningRing);
        if (shadow) Destroy(shadow);
        if (trackLine) Destroy(trackLine.gameObject);
        Destroy(gameObject);
    }
}
