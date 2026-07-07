using UnityEngine;

public class ExplosionFX : MonoBehaviour
{
    const float Life = 0.35f;

    float elapsed;
    Vector3 startScale;
    Renderer rend;

    public static void Spawn(Vector3 pos, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.3f;

        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        go.GetComponent<MeshRenderer>().material = mat;

        var fx = go.AddComponent<ExplosionFX>();
        fx.rend = go.GetComponent<Renderer>();
        fx.startScale = go.transform.localScale;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / Life;
        transform.localScale = startScale * Mathf.Lerp(1f, 6f, t);

        if (rend != null)
        {
            Color c = rend.material.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            rend.material.color = c;
        }

        if (t >= 1f) Destroy(gameObject);
    }
}
