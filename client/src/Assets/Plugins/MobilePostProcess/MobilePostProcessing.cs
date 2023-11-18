using UnityEngine;

[ExecuteInEditMode]
public class MobilePostProcessing : MonoBehaviour
{
    public bool Blur = false;
    [Range(0, 1)]
    public float BlurAmount = 1f;
    public Texture2D BlurMask;
    public bool Bloom = false;
    public Color BloomColor = Color.white;
    [Range(0, 5)]
    public float BloomAmount = 1f;
    [Range(0, 1)]
    public float BloomDiffuse = 1f;
    [Range(0, 1)]
    public float BloomThreshold = 0f;
    [Range(0, 1)]
    public float BloomSoftness = 0f;

    public bool LUT = false;
    [Range(0, 1)]
    public float LutAmount = 0.0f;
    public Texture2D SourceLut = null;

    public bool ImageFiltering = false;
    public Color Color = Color.white;
    [Range(0, 1)]
    public float Contrast = 0f;
    [Range(-1, 1)]
    public float Brightness = 0f;
    [Range(-1, 1)]
    public float Saturation = 0f;
    [Range(-1, 1)]
    public float Exposure = 0f;
    [Range(-1, 1)]
    public float Gamma = 0f;
    [Range(0, 1)]
    public float Sharpness = 0f;

    public bool ChromaticAberration = false;
    public float Offset = 0;
    [Range(-1, 1)]
    public float FishEyeDistortion = 0;
    [Range(0, 1)]
    public float GlitchAmount = 0;

    public bool Distortion = false;
    [Range(0, 1)]
    public float LensDistortion = 0;

    public bool Vignette = false;
    public Color VignetteColor = Color.black;
    [Range(0, 1)]
    public float VignetteAmount = 0f;
    [Range(0.001f, 1)]
    public float VignetteSoftness = 0.0001f;

    static readonly int blurTexString = Shader.PropertyToID("_BlurTex");
    static readonly int maskTextureString = Shader.PropertyToID("_MaskTex");
    static readonly int blurAmountString = Shader.PropertyToID("_BlurAmount");
    static readonly int bloomColorString = Shader.PropertyToID("_BloomColor");
    static readonly int blDiffuseString = Shader.PropertyToID("_BloomDiffuse");
    static readonly int blDataString = Shader.PropertyToID("_BloomData");
    static readonly int lutTextureString = Shader.PropertyToID("_LutTex");
    static readonly int lutAmountString = Shader.PropertyToID("_LutAmount");
    static readonly int colorString = Shader.PropertyToID("_Color");
    static readonly int contrastString = Shader.PropertyToID("_Contrast");
    static readonly int brightnessString = Shader.PropertyToID("_Brightness");
    static readonly int saturationString = Shader.PropertyToID("_Saturation");
    static readonly int centralFactorString = Shader.PropertyToID("_CentralFactor");
    static readonly int sideFactorString = Shader.PropertyToID("_SideFactor");
    static readonly int offsetString = Shader.PropertyToID("_Offset");
    static readonly int fishEyeString = Shader.PropertyToID("_FishEye");
    static readonly int lensdistortionString = Shader.PropertyToID("_LensDistortion");
    static readonly int vignetteColorString = Shader.PropertyToID("_VignetteColor");
    static readonly int vignetteAmountString = Shader.PropertyToID("_VignetteAmount");
    static readonly int vignetteSoftnessString = Shader.PropertyToID("_VignetteSoftness");

    static readonly string bloomKeyword = "BLOOM";
    static readonly string blurKeyword = "BLUR";
    static readonly string chromaKeyword = "CHROMA";
    static readonly string lutKeyword = "LUT";
    static readonly string filterKeyword = "FILTER";
    static readonly string shaprenKeyword = "SHARPEN";
    static readonly string distortionKeyword = "DISTORTION";

    public Material material;

    private Texture2D previous;
    private Texture3D converted3D = null;
    private float t, a, knee;
    private int numberOfPasses = 3;

    public void Start()
    {
        if (BlurMask==null)
        {
            Shader.SetGlobalTexture(maskTextureString, Texture2D.whiteTexture);
        }
        else
            Shader.SetGlobalTexture(maskTextureString, BlurMask);
    }

    public void Update()
    {
        if (SourceLut != previous)
        {
            previous = SourceLut;
            Convert3D(SourceLut);
        }
    }

    private void OnDestroy()
    {
        if (converted3D != null)
        {
            DestroyImmediate(converted3D);
        }
        converted3D = null;
    }

    private void Convert3D(Texture2D temp3DTex)
    {
        var color = temp3DTex.GetPixels();
        var newCol = new Color[color.Length];

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 16; k++)
                {
                    int val = 16 - j - 1;
                    newCol[i + (j * 16) + (k * 256)] = color[k * 16 + i + val * 256];
                }
            }
        }
        if (converted3D)
            DestroyImmediate(converted3D);
        converted3D = new Texture3D(16, 16, 16, TextureFormat.ARGB32, false);
        converted3D.SetPixels(newCol);
        converted3D.Apply();
        converted3D.wrapMode = TextureWrapMode.Clamp;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Blur || Bloom)
        {
            material.DisableKeyword(blurKeyword);
            material.DisableKeyword(bloomKeyword);
            if (Bloom)
            {
                material.EnableKeyword(bloomKeyword);
                material.SetColor(bloomColorString, BloomColor * BloomAmount);
                material.SetFloat(blDiffuseString, BloomDiffuse);
                numberOfPasses = Mathf.Max(Mathf.CeilToInt(BloomDiffuse * 4), 1);
                material.SetFloat(blDiffuseString, numberOfPasses > 1 ? (BloomDiffuse * 4 - Mathf.FloorToInt(BloomDiffuse * 4 - 0.001f)) * 0.5f + 0.5f : BloomDiffuse * 4);
                knee = BloomThreshold * BloomSoftness;
                material.SetVector(blDataString, new Vector4(BloomThreshold, BloomThreshold - knee, 2f * knee, 1f / (4f * knee + 0.00001f)));
            }
            if (Blur) 
            {
                material.EnableKeyword(blurKeyword);
                numberOfPasses = Mathf.Max(Mathf.CeilToInt(BlurAmount * 4), 1);
                material.SetFloat(blurAmountString, numberOfPasses > 1 ? (BlurAmount * 4 - Mathf.FloorToInt(BlurAmount * 4 - 0.001f)) * 0.5f + 0.5f : BlurAmount * 4);
            }

            if (BlurAmount > 0 || !Blur)
            {
                RenderTexture blurTex = null;

                if (numberOfPasses == 1)
                {
                    blurTex = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, source.format);
                    Graphics.Blit(source, blurTex, material, 0);
                }
                else if (numberOfPasses == 2)
                {
                    blurTex = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, source.format);
                    var temp1 = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
                    Graphics.Blit(source, temp1, material, 0);
                    Graphics.Blit(temp1, blurTex, material, 0);
                    RenderTexture.ReleaseTemporary(temp1);
                }
                else if (numberOfPasses == 3)
                {
                    blurTex = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
                    var temp1 = RenderTexture.GetTemporary(Screen.width / 8, Screen.height / 8, 0, source.format);
                    Graphics.Blit(source, blurTex, material, 0);
                    Graphics.Blit(blurTex, temp1, material, 0);
                    Graphics.Blit(temp1, blurTex, material, 0);
                    RenderTexture.ReleaseTemporary(temp1);
                }              
                else if (numberOfPasses == 4)
                {
                    blurTex = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
                    var temp1 = RenderTexture.GetTemporary(Screen.width / 8, Screen.height / 8, 0, source.format);
                    var temp2 = RenderTexture.GetTemporary(Screen.width / 16, Screen.height / 16, 0, source.format);
                    Graphics.Blit(source, blurTex, material, 0);
                    Graphics.Blit(blurTex, temp1, material, 0);
                    Graphics.Blit(temp1, temp2, material, 0);
                    Graphics.Blit(temp2, temp1, material, 0);
                    Graphics.Blit(temp1, blurTex, material, 0);
                    RenderTexture.ReleaseTemporary(temp1);
                    RenderTexture.ReleaseTemporary(temp2);
                }

                material.SetTexture(blurTexString, blurTex);
                RenderTexture.ReleaseTemporary(blurTex);
            }
            else
            {
                material.SetTexture(blurTexString, source);
            }
        }
        else
        {
            material.DisableKeyword(blurKeyword);
            material.DisableKeyword(bloomKeyword);
        }

        if (LUT)
        {
            material.EnableKeyword(lutKeyword);
            material.SetFloat(lutAmountString, LutAmount);
            material.SetTexture(lutTextureString, converted3D);
        }
        else
        {
            material.DisableKeyword(lutKeyword);
        }

        if (ImageFiltering)
        {
            material.EnableKeyword(filterKeyword);
            material.SetColor(colorString, (Mathf.Pow(2, Exposure) - Gamma) * Color);
            material.SetFloat(contrastString, Contrast + 1f);
            material.SetFloat(brightnessString, Brightness * 0.5f - Contrast);
            material.SetFloat(saturationString, Saturation + 1f);

            if (Sharpness > 0)
            {
                material.EnableKeyword(shaprenKeyword);
                material.SetFloat(centralFactorString, 1.0f + (3.2f * Sharpness));
                material.SetFloat(sideFactorString, 0.8f * Sharpness);
            }
            else
            {
                material.DisableKeyword(shaprenKeyword);
            }
        }
        else
        {
            material.DisableKeyword(filterKeyword);
            material.DisableKeyword(shaprenKeyword);
        }

        if (ChromaticAberration)
        {
            material.EnableKeyword(chromaKeyword);

            if (GlitchAmount > 0)
            {
                t = Time.realtimeSinceStartup;
                a = (1.0f + Mathf.Sin(t * 6.0f)) * ((0.5f + Mathf.Sin(t * 16.0f) * 0.25f)) * (0.5f + Mathf.Sin(t * 19.0f) * 0.25f) * (0.5f + Mathf.Sin(t * 27.0f) * 0.25f);
                material.SetFloat(offsetString, 10 * Offset + GlitchAmount * Mathf.Pow(a, 3.0f) * 200);
            }
            else
                material.SetFloat(offsetString, 10 * Offset);

            material.SetFloat(fishEyeString, 0.1f * FishEyeDistortion);
        }
        else
        {
            material.DisableKeyword(chromaKeyword);
        }

        if (Distortion)
        {
            material.SetFloat(lensdistortionString, -LensDistortion);
            material.EnableKeyword(distortionKeyword);
        }
        else
        {
            material.DisableKeyword(distortionKeyword);
        }

        if (Vignette)
        {
            material.SetColor(vignetteColorString, VignetteColor);
            material.SetFloat(vignetteAmountString, 1 - VignetteAmount);
            material.SetFloat(vignetteSoftnessString, 1 - VignetteSoftness - VignetteAmount);
        }
        else
        {
            material.SetFloat(vignetteAmountString, 1f);
            material.SetFloat(vignetteSoftnessString, 0.999f);
        }

        Graphics.Blit(source, destination, material, 1);
    }
}
