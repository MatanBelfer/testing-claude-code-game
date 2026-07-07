using UnityEngine;
using UnityEngine.Rendering;

public static class VisualUtil
{
    static Shader litShader;

    public static Shader LitShader
    {
        get
        {
            if (litShader == null)
            {
                if (GraphicsSettings.currentRenderPipeline != null)
                    litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit");
                if (litShader == null)
                    litShader = Shader.Find("Standard");
            }
            return litShader;
        }
    }

    public static Material NewLitMaterial(Color color)
    {
        var mat = new Material(LitShader);
        mat.color = color;
        return mat;
    }

    // Unlit, vertex-colored, alpha-blended; exists in built-in, URP and HDRP,
    // so it's safe for lines, trails and transparent effects in any pipeline.
    public static Material NewUnlitTransparentMaterial(Color color)
    {
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
}
