using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Y2KFullScreen : MonoBehaviour
{
    [Range(0,1)] public float scanIntensity = 0.5f;
    [Range(1,20)] public float pixelate = 8f;
    [Range(0,1)] public float rgbShift = 0.3f;
    [Range(0,1)] public float noise = 0.08f;

    private Material mat;

    void Awake()
    {
        mat = new Material(Shader.Find("BUG/Y2K"));
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (mat == null) { Graphics.Blit(src, dest); return; }

        mat.SetFloat("_ScanInt", scanIntensity);
        mat.SetFloat("_Pixelate", pixelate);
        mat.SetFloat("_RGBShift", rgbShift);
        mat.SetFloat("_Noise", noise);

        Graphics.Blit(src, dest, mat);
    }
}