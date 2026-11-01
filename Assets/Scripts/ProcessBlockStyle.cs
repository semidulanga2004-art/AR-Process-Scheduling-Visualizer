
using UnityEngine;
using UnityEngine.Rendering;

public static class ProcessBlockStyle
{
    private static readonly Color[] processColors =
    {
        new Color32(185, 164, 245, 255), // Lavender
        new Color32(139, 198, 245, 255), // Baby blue
        new Color32(142, 218, 187, 255), // Mint
        new Color32(255, 184, 153, 255), // Peach
        new Color32(242, 170, 210, 255)  // Soft pink
    };

    private static Material[] materials;

    public static void Apply(
        GameObject block,
        int index,
        TextMesh label)
    {
        if (block == null) return;

        int colorIndex = index % processColors.Length;

        if (materials == null)
            materials = new Material[processColors.Length];

        Renderer rend = block.GetComponent<Renderer>();

        if (rend != null)
        {
            if (materials[colorIndex] == null)
            {
                Shader shader;

                if (GraphicsSettings.currentRenderPipeline != null)
                    shader = Shader.Find("Universal Render Pipeline/Lit");
                else
                    shader = Shader.Find("Standard");

                if (shader == null)
                    shader = Shader.Find("Unlit/Color");

                if (shader == null)
                {
                    Debug.LogError("No compatible shader found!");
                    return;
                }

                Material mat = new Material(shader);

                mat.color = processColors[colorIndex];

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", processColors[colorIndex]);

                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", processColors[colorIndex]);

                materials[colorIndex] = mat;
            }

            rend.sharedMaterial = materials[colorIndex];
        }

        if (label != null)
        {
            if (label != null)
        {
            label.color = new Color32(20, 18, 48, 255);
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 64;
            label.characterSize = 0.03f;
        }
        }
    }
}
