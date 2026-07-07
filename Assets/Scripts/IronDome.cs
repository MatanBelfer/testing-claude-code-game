using System.Collections.Generic;
using UnityEngine;

public class IronDome : MonoBehaviour
{
    public float CooldownRemaining { get; private set; }
    public bool IsReady => CooldownRemaining <= 0f;

    LineRenderer previewLine;

    void Awake()
    {
        BuildVisual();

        previewLine = gameObject.AddComponent<LineRenderer>();
        previewLine.material = new Material(Shader.Find("Sprites/Default"));
        previewLine.startColor = previewLine.endColor = new Color(0.2f, 1f, 0.4f, 0.9f);
        previewLine.startWidth = previewLine.endWidth = 0.25f;
        previewLine.positionCount = 0;
        previewLine.useWorldSpace = true;
    }

    void BuildVisual()
    {
        var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.transform.SetParent(transform, false);
        baseObj.transform.localScale = new Vector3(2f, 0.4f, 2f);
        baseObj.transform.localPosition = new Vector3(0, 0.4f, 0);
        Destroy(baseObj.GetComponent<Collider>());
        var baseMat = new Material(Shader.Find("Standard"));
        baseMat.color = new Color(0.25f, 0.3f, 0.25f);
        baseObj.GetComponent<MeshRenderer>().material = baseMat;

        var turret = GameObject.CreatePrimitive(PrimitiveType.Cube);
        turret.transform.SetParent(transform, false);
        turret.transform.localScale = new Vector3(0.6f, 2.2f, 1.2f);
        turret.transform.localPosition = new Vector3(0, 1.9f, 0);
        turret.transform.localRotation = Quaternion.Euler(-25, 0, 0);
        Destroy(turret.GetComponent<Collider>());
        var turretMat = new Material(Shader.Find("Standard"));
        turretMat.color = new Color(0.5f, 0.55f, 0.5f);
        turret.GetComponent<MeshRenderer>().material = turretMat;
    }

    void Update()
    {
        if (CooldownRemaining > 0f)
            CooldownRemaining -= Time.deltaTime;
    }

    public void ShowPreview(List<Vector3> points)
    {
        previewLine.positionCount = points.Count;
        previewLine.SetPositions(points.ToArray());
    }

    public void ClearPreview()
    {
        previewLine.positionCount = 0;
    }

    public bool Fire(List<Vector3> path)
    {
        if (!IsReady || path.Count < 2) return false;
        CooldownRemaining = GameConfig.DomeFireCooldown;

        var go = new GameObject("Interceptor");
        var interceptor = go.AddComponent<Interceptor>();
        interceptor.Init(path);
        return true;
    }
}
