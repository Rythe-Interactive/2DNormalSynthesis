using System.Collections.Generic;
using Unity.Collections;
using System.Threading.Tasks;
using UnityEngine;
public static class GaussianBlur
{
    // The MIT License
    // Copyright © 2020 Roger Cabo Ashauer
    // Permission is hereby granted, free of charge,
    //to any person obtaining a copy of this software and associated documentation files (the "Software"),
    //to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute,
    // sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: 
    // The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. 
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,  
    // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,  
    // WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    // https://de.wikipedia.org/wiki/MIT-Lizenz

    // This solution is based on Fast image convolutions by Wojciech Jarosz.
    // http://elynxsdk.free.fr/ext-docs/Blur/Fast_box_blur.pdf
    // And Ivan Kutskir
    // http://blog.ivank.net/fastest-gaussian-blur.html
    // And Mike Demyl
    // https://github.com/mdymel  // https://github.com/mdymel/superfastblur

    private static readonly ParallelOptions _pOptions = new ParallelOptions
    {
        MaxDegreeOfParallelism = 8
    };
    public static void Blur(ref Texture2D tex, int radius = 1)
    {
        //init variables
        NativeArray<Color32> RawTextureData = tex.GetRawTextureData<Color32>();
        int width = tex.width;
        int height = tex.height;
        int pixelCount = width * height;
        int[] m_red = new int[pixelCount];
        int[] m_green = new int[pixelCount];
        int[] m_blue = new int[pixelCount];
        int[] m_alpha = new int[pixelCount];

        //copy values
        Parallel.For(0, pixelCount, _pOptions, i =>
        {
            m_red[i] = RawTextureData[i].r;
            m_green[i] = RawTextureData[i].g;
            m_blue[i] = RawTextureData[i].b;
            m_alpha[i] = RawTextureData[i].a;
        });

        //init new color arrays
        int[] newAlpha = new int[pixelCount];
        int[] newRed = new int[pixelCount];
        int[] newGreen = new int[pixelCount];
        int[] newBlue = new int[pixelCount];

        //invoke blur
        Parallel.Invoke(
            //      () => gaussBlur_4(m_alpha, newAlpha, radius, width, height),
            () => gaussBlur_4(m_red, newRed, radius, width, height),
            () => gaussBlur_4(m_green, newGreen, radius, width, height),
            () => gaussBlur_4(m_blue, newBlue, radius, width, height));

        //clamp values && write values to texture data
        Parallel.For(0, pixelCount, _pOptions, i =>
        {

            if (newAlpha[i] > 255) newAlpha[i] = 255;
            if (newRed[i] > 255) newRed[i] = 255;
            if (newGreen[i] > 255) newGreen[i] = 255;
            if (newBlue[i] > 255) newBlue[i] = 255;

            if (newAlpha[i] < 0) newAlpha[i] = 0;
            if (newRed[i] < 0) newRed[i] = 0;
            if (newGreen[i] < 0) newGreen[i] = 0;
            if (newBlue[i] < 0) newBlue[i] = 0;

            RawTextureData[i] = new Color32((byte)newRed[i], (byte)newGreen[i], (byte)newBlue[i], (byte)m_alpha[i]);
        });
        tex.Apply(false);
    }
    private static void gaussBlur_4(int[] colorChannel, int[] destChannel, int r, int width, int height)
    {
        int[] bxs = boxesForGauss(r, 3);
        boxBlur_4(colorChannel, destChannel, width, height, (bxs[0] - 1) / 2);
        boxBlur_4(destChannel, colorChannel, width, height, (bxs[1] - 1) / 2);
        boxBlur_4(colorChannel, destChannel, width, height, (bxs[2] - 1) / 2);
    }

    private static void boxBlur_4(int[] colorChannel, int[] destChannel, int w, int h, int r)
    {
        for (var i = 0; i < colorChannel.Length; i++) destChannel[i] = colorChannel[i];
        boxBlurH_4(destChannel, colorChannel, w, h, r);
        boxBlurT_4(colorChannel, destChannel, w, h, r);
    }

    private static void boxBlurH_4(int[] colorChannel, int[] dest, int w, int h, int radial)
    {
        var iar = (double)1 / (radial + radial + 1);
        Parallel.For(0, h, _pOptions, i =>
        {
            var ti = i * w;
            var li = ti;
            var ri = ti + radial;
            var fv = colorChannel[ti];
            var lv = colorChannel[ti + w - 1];
            var val = (radial + 1) * fv;
            for (var j = 0; j < radial; j++) val += colorChannel[ti + j];
            for (var j = 0; j <= radial; j++)
            {
                val += colorChannel[ri++] - fv;
                dest[ti++] = (int)System.Math.Round(val * iar);
            }
            for (var j = radial + 1; j < w - radial; j++)
            {
                val += colorChannel[ri++] - dest[li++];
                dest[ti++] = (int)System.Math.Round(val * iar);
            }
            for (var j = w - radial; j < w; j++)
            {
                val += lv - colorChannel[li++];
                dest[ti++] = (int)System.Math.Round(val * iar);
            }
        });
    }
    private static void boxBlurT_4(int[] colorChannel, int[] dest, int w, int h, int r)
    {
        var iar = (double)1 / (r + r + 1);
        Parallel.For(0, w, _pOptions, i =>
        {
            var ti = i;
            var li = ti;
            var ri = ti + r * w;
            var fv = colorChannel[ti];
            var lv = colorChannel[ti + w * (h - 1)];
            var val = (r + 1) * fv;
            for (var j = 0; j < r; j++) val += colorChannel[ti + j * w];
            for (var j = 0; j <= r; j++)
            {
                val += colorChannel[ri] - fv;
                dest[ti] = (int)System.Math.Round(val * iar);
                ri += w;
                ti += w;
            }
            for (var j = r + 1; j < h - r; j++)
            {
                val += colorChannel[ri] - colorChannel[li];
                dest[ti] = (int)System.Math.Round(val * iar);
                li += w;
                ri += w;
                ti += w;
            }
            for (var j = h - r; j < h; j++)
            {
                val += lv - colorChannel[li];
                dest[ti] = (int)System.Math.Round(val * iar);
                li += w;
                ti += w;
            }
        });
    }

    private static int[] boxesForGauss(int sigma, int n)
    {
        double wIdeal = System.Math.Sqrt((12 * sigma * sigma / n) + 1);
        int wl = (int)System.Math.Floor(wIdeal);
        if (wl % 2 == 0) wl--;
        int wu = wl + 2;

        double mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
        double m = System.Math.Round(mIdeal);

        var sizes = new List<int>();
        for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
        return sizes.ToArray();
    }
}