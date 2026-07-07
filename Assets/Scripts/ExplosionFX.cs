using UnityEngine;

public class ExplosionFX : MonoBehaviour
{
    const float Life = 0.35f;

    float elapsed;
    float finalDiameter;
    Renderer rend;

    // finalDiameter is the fireball's size at the end of its growth; pass the
    // blast diameter so the visual matches the gameplay radius.
    public static void Spawn(Vector3 pos, Color color, float finalDiameter = 1.8f)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.3f;

        go.GetComponent<MeshRenderer>().material = VisualUtil.NewUnlitTransparentMaterial(color);

        var fx = go.AddComponent<ExplosionFX>();
        fx.rend = go.GetComponent<Renderer>();
        fx.finalDiameter = finalDiameter;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / Life;
        transform.localScale = Vector3.one * Mathf.Lerp(0.3f, finalDiameter, t);

        if (rend != null)
        {
            Color c = rend.material.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            rend.material.color = c;
        }

        if (t >= 1f) Destroy(gameObject);
    }
}
