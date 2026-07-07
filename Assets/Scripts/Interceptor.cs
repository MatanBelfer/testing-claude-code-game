using System.Collections.Generic;
using UnityEngine;

public class Interceptor : MonoBehaviour
{
    List<Vector3> path;
    int segmentIndex;

    // Waypoints arrive on the ground plane (as drawn). The interceptor
    // launches from the dome and flies the same path lifted to engage height.
    public void Init(List<Vector3> groundWaypoints)
    {
        path = new List<Vector3>(groundWaypoints.Count);
        path.Add(groundWaypoints[0] + Vector3.up * 1.5f);
        for (int i = 1; i < groundWaypoints.Count; i++)
            path.Add(groundWaypoints[i] + Vector3.up * GameConfig.InterceptorEngageHeight);

        transform.position = path[0];
        BuildVisual();
    }

    void BuildVisual()
    {
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(transform, false);
        body.transform.localScale = new Vector3(0.35f, 0.9f, 0.35f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<MeshRenderer>().material = VisualUtil.NewLitMaterial(new Color(0.85f, 0.9f, 0.95f));

        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.8f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0.02f;
        trail.material = VisualUtil.NewUnlitTransparentMaterial(Color.white);
        trail.startColor = new Color(1f, 1f, 1f, 0.9f);
        trail.endColor = new Color(0.6f, 0.6f, 0.6f, 0f);
    }

    void Update()
    {
        if (path == null || segmentIndex >= path.Count - 1)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 target = path[segmentIndex + 1];
        Vector3 toTarget = target - transform.position;
        float step = GameConfig.InterceptorSpeed * Time.deltaTime;

        if (toTarget.magnitude <= step)
        {
            transform.position = target;
            segmentIndex++;
        }
        else
        {
            Vector3 moveDir = toTarget.normalized;
            transform.position += moveDir * step;
            transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up) * Quaternion.Euler(90, 0, 0);
        }

        CheckIntercepts();
    }

    // Proximity fuse: generous horizontal radius plus a separate vertical
    // tolerance, so the player times the path crossing rather than having to
    // match the enemy's exact altitude.
    void CheckIntercepts()
    {
        foreach (var missile in GameManager.Instance.ActiveMissiles)
        {
            if (missile == null || missile.IsResolved) continue;
            Vector3 d = missile.transform.position - transform.position;
            float horizontal = new Vector2(d.x, d.z).magnitude;
            if (horizontal <= GameConfig.InterceptHitRadius &&
                Mathf.Abs(d.y) <= GameConfig.VerticalInterceptTolerance)
            {
                missile.InterceptedAt(transform.position);
                Destroy(gameObject);
                return;
            }
        }
    }
}
