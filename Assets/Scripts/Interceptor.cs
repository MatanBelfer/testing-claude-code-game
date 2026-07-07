using System.Collections.Generic;
using UnityEngine;

public class Interceptor : MonoBehaviour
{
    List<Vector3> path;
    int segmentIndex;

    public void Init(List<Vector3> waypoints)
    {
        path = waypoints;
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

    void CheckIntercepts()
    {
        foreach (var missile in GameManager.Instance.ActiveMissiles)
        {
            if (missile == null || missile.IsResolved) continue;
            if (Vector3.Distance(transform.position, missile.transform.position) <= GameConfig.InterceptHitRadius)
            {
                missile.InterceptedAt(transform.position);
                Destroy(gameObject);
                return;
            }
        }
    }
}
