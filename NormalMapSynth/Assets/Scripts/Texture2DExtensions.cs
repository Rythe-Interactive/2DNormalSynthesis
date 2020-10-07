//using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

public static class Texture2DExtensions
{
    public static void Mask(this Texture2D tex, Texture2D mask)
    {

        Color[] c = mask.GetPixels();

        for (int x = 0; x < mask.width; x++)
        {
            for (int y = 0; y < mask.height; y++)
            {
                int index = x + y * mask.width;
                Color currentC = c[index];
                if (currentC.a == 0)
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }

    }
    public static void DiscardNullAlpha(this Texture2D tex)
    {
        Color[] c = tex.GetPixels();

        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                int index = x + y * tex.width;
                Color currentC = c[index];
                if (currentC.a == 0)
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
    }


    //we assume that the vector pased in is positiv
    public static Color NormalToUV(Vector3 normal)
    {

        Color c;
        float r, g, b;

        r = 1 / normal.x * 256;

        g = 1 / normal.y * 256;

        b = 1 / normal.z * 256;

        c = new Color(r, g, b, 1);
        return c;
    }
    public static Texture2D GenerateHeightMap(this Texture2D tex)
    {
        if (tex == null) return null;
        Texture2D grayScale = new Texture2D(tex.width, tex.height, tex.format, false);
        Graphics.CopyTexture(tex, grayScale);
        Color[] alpha = grayScale.GetPixels();
        //read color data
        Color32[] pixels = tex.GetPixels32();
        Color32[] changedPixels = new Color32[tex.width * tex.height];

        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                //read sample value
                Color32 pixel = pixels[x + y * tex.width];
                //calculate gray value from color value
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                //store new color
                float a = alpha[x + y * tex.width].a;

                Color c = new Color(l, l, l, a);

                if (a == 0) c = new Color(0, 0, 0, 0);
                changedPixels[x + y * tex.width] = c;
            }
        }
        //set && apply GreyScale texture 
        grayScale.SetPixels32(changedPixels);
        grayScale.Apply(false);

        return grayScale;
    }
    public static Texture2D GenerateNormalFromHeight(this Texture2D tex, float depth = 1000.0f, bool useAlphe = true)
    {

        if (tex == null) return null;
        Texture2D newTexture = new Texture2D(tex.width, tex.height);

        Color32[] NormalMapData = new Color32[tex.width * tex.height];
        Color[] pixels = tex.GetPixels();
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                //execute if alpha should be taken into account
                if (useAlphe)
                {
                    //if alpha is 0 store no color and skip normal calculation
                    if (tex.SampleHeightValue(x, y) == 0)
                    {
                        NormalMapData[x + tex.width * y] = new Color32(0, 0, 1, 1);
                        continue;
                    }
                }

                float dx = -tex.SampleHeightValue(x - 1, y) + tex.SampleHeightValue(x + 1, y);
                float dy = -tex.SampleHeightValue(x, y - 1) + tex.SampleHeightValue(x, y + 1);
                float3 n = new float3(-dx * (depth / 1000.0f), dy * (depth / 1000.0f), 1);

                NormalMapData[x + tex.width * y] = Texture2DExtensions.NormalToUV(math.normalize(n));
            }
        }
        //store && set data
        newTexture.SetPixels32(NormalMapData);
        newTexture.Apply(false);
        return newTexture;
    }
    private static float SampleHeightValue(this Texture2D tex, int x, int y)
    {
        //return 0 if texture does not exist
        if (tex == null) return 0;

        //return 0 if out of texture bounds
        if (x < 0 || x > tex.width || y < 0 || y > tex.height) return 0;

        //sample height
        Color sample = tex.GetPixel(x, y);
        //Return 0 if the alpha is 0 
        if (sample.a == 0) return 0;
        //else return any color value which is the height value
        return sample.r;
    }
}
