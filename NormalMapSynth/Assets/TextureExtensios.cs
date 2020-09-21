//using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

public static class TextureExtensions
{

    //we assume that the vector pased in is positiv
    public static Color NormalToColor(Vector3 normal)
    {

        Color c;
        float r, g, b, a;
       
            r = 1 / normal.x * 256 ;
        
            g = 1 / normal.y * 256;

     
            b = 1 / normal.z * 256;


        c = new Color(r, g, b, 1);
        return c;
    }

    public static float3 NormalToColorSpace(Vector3 normal)
    {

        Color32 c;
        float r, g, b, a;
        if (normal.x == 0) r = 128;
        else
            r = 1 / normal.x * 126 + 128;

        if (normal.y == 0) g = 128;
        else
            g = 1 / normal.y * 126 + 128;

        if (normal.z == 0) b = 128;
        else
            b = 1 / normal.z * 126 + 128;


        return new float3(r, g, b);
    }



    public static Color32[] Convolve(Texture2D srcImage, double[,] kernel)
    {
        int width = srcImage.width;
        int height = srcImage.height;
        Color32[] ColorBuffer = new Color32[width * height];

        ColorBuffer = srcImage.GetPixels32();

        int bytes = width * height;
        byte[] buffer = new byte[bytes];

        int colorChannels = 3;
        double[] rgb = new double[colorChannels];

        int foff = (kernel.GetLength(0) - 1) / 2;
        int kcenter = 0;
        int kpixel = 0;

        for (int y = foff; y < height - foff; y++)
        {
            for (int x = foff; x < width - foff; x++)
            {
                //init rgb variales
                for (int c = 0; c < colorChannels; c++)
                {
                    rgb[c] = 0.0;
                }
                kcenter = y * width + x * 4;
                //do calculation
                for (int fy = -foff; fy <= foff; fy++)
                {
                    for (int fx = -foff; fx <= foff; fx++)
                    {
                        kpixel = kcenter + fy * width + fx * 4;
                        for (int c = 0; c < colorChannels; c++)
                        {
                            rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + foff, fx + foff];
                        }
                    }
                }
                //clamp color values
                for (int c = 0; c < colorChannels; c++)
                {
                    if (rgb[c] > 255)
                    {
                        rgb[c] = 255;
                    }
                    else if (rgb[c] < 0)
                    {
                        rgb[c] = 0;
                    }
                }
                ColorBuffer[y + x * height] = new Color32((byte)rgb[0], (byte)rgb[1], (byte)rgb[2], 255);
            }
        }
        return ColorBuffer;
    }


    public static double[,] GaussianBlur(int lenght, double weight)
    {
        double[,] kernel = new double[lenght, lenght];
        double kernelSum = 0;
        int foff = (lenght - 1) / 2;
        double distance = 0;
        double constant = 1d / (2 * math.PI * weight * weight);
        for (int y = -foff; y <= foff; y++)
        {
            for (int x = -foff; x <= foff; x++)
            {
                distance = ((y * y) + (x * x)) / (2 * weight * weight);
                kernel[y + foff, x + foff] = constant * math.exp(-distance);
                kernelSum += kernel[y + foff, x + foff];
            }
        }
        for (int y = 0; y < lenght; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                kernel[y, x] = kernel[y, x] * 1d / kernelSum;
            }
        }
        return kernel;
    }
}
