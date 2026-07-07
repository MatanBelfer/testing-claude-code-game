using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
    public int HitCount { get; private set; }

    readonly Color baseColor = new Color(0.82f, 0.72f, 0.55f);
    Renderer[] buildingRenderers;

    void Awake()
    {
        BuildVisual();
    }

    void BuildVisual()
    {
        var rends = new List<Renderer>();
        var rnd = new System.Random(transform.position.GetHashCode());
        const int buildingCount = 4;
        for (int i = 0; i < buildingCount; i++)
        {
            var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.transform.SetParent(transform, false);
            float h = 1.5f + (float)rnd.NextDouble() * 2.5f;
            b.transform.localScale = new Vector3(1.6f, h, 1.6f);
            float offsetX = ((float)rnd.NextDouble() - 0.5f) * 5f;
            float offsetZ = ((float)rnd.NextDouble() - 0.5f) * 5f;
            b.transform.localPosition = new Vector3(offsetX, h / 2f, offsetZ);
            Destroy(b.GetComponent<Collider>());

            b.GetComponent<MeshRenderer>().material = VisualUtil.NewLitMaterial(baseColor);
            rends.Add(b.GetComponent<Renderer>());
        }
        buildingRenderers = rends.ToArray();
    }

    public void TakeHit()
    {
        HitCount++;
        float darken = Mathf.Clamp01(HitCount * 0.15f);
        foreach (var r in buildingRenderers)
        {
            r.material.color = Color.Lerp(baseColor, Color.black, darken);
        }
    }
}
