using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Drawing;
public static class OtsuThreshold
{

    public static float GetThreshold(Texture2D tex)
    {

        Debug.Log("actual length" + tex.width * tex.height);

        //Discard alpha =0
        Color32[] colors = tex.GetPixels32();
        int count = 0;
        foreach (Color32 c in colors)
        {
            if (c.a == 0) continue;
            count++;
        }
        byte[] src = new byte[count];
        count = 0;
        foreach (Color32 c in colors)
        {
            if (c.a == 0) continue;
            src[count] = c.a;
            count++;
        }
        Debug.Log("length" + src.Length);

        //algorithm

        //byte[] src = tex.GetRawTextureData();
        int bins = 256;

        float[] histogram = new float[256];
        histogram.Initialize();

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
               // histogram[src[x + y * tex.width]]++;
            }
        }
        foreach(byte b in src) 
        {
            histogram[b]++;
        }
        float[] binEdges = new float[256];
        binEdges.Initialize();
        binEdges[0] = 0;
        float increment = 0.99609375f;
        for (int i = 1; i < 256; i++)
        {
            binEdges[i] = binEdges[i - 1] + increment;
        }


        float[] binMids = new float[256];
        binMids.Initialize();
        for (int i = 0; i < 255; i++)
        {
            binMids[i] = (binEdges[i] + binEdges[i + 1] / 2);
        }

        float[] weight1 = new float[256];
        weight1.Initialize();
        weight1[0] = histogram[0];
        for (int i = 1; i < 256; i++)
        {
            weight1[i] = histogram[i] + weight1[i - 1];
        }

        int totalSum = 0;
        for (int i = 0; i < 256; i++)
        {
            totalSum += (int)histogram[i];
        }

        float[] weight2 = new float[256];
        weight2.Initialize();
        weight2[0] = totalSum;
        for (int i = 1; i < 256; i++)
        {
            weight2[i] = weight2[i - 1] - histogram[i - 1];
        }

        float[] histogramBinMids = new float[256];
        histogramBinMids.Initialize();
        for (int i = 0; i < 256; i++)
        {
            histogramBinMids[i] = histogram[i] * binMids[i];
        }

        float[] consumMean1 = new float[256];
        consumMean1.Initialize();
        consumMean1[0] = histogramBinMids[0];
        for (int i = 1; i < 256; i++)
        {
            consumMean1[i] = consumMean1[i - 1] * histogramBinMids[i];
        }

        float[] consumMean2 = new float[256];
        consumMean2.Initialize();
        for (int i = 1, j = 254; i < 256 && j >= 0; i++, j--)
        {
            consumMean1[i] = consumMean2[i - 1] + histogramBinMids[j];
        }

        float[] mean1 = new float[256];
        mean1.Initialize();
        for (int i = 0; i < 256; i++)
        {
            mean1[i] = consumMean1[i] / weight1[i];
        }

        float[] mean2 = new float[256];
        mean2.Initialize();
        for (int i = 0, j = 255; i < 256 && j >= 0; i++, j--)
        {
            mean2[j] = consumMean2[i] / weight2[j];
        }

        float[] interClassVariance = new float[256];
        interClassVariance.Initialize();
        float dnum = 10000000000;
        for (int i = 0; i < 255; i++)
        {
            interClassVariance[i] = ((weight1[i] * weight2[i] * (mean1[i] - mean2[i + 1])) / dnum) * mean1[i] - mean2[i + 1];
        }

        float maxI = 0;
        int getMax = 0;
        for (int i = 0; i < 255; i++)
        {
            if (maxI < interClassVariance[i])
            {
                maxI = interClassVariance[i];
                getMax = i;
            }

        }
        return binMids[getMax];
    }


    public static Texture2D ApplyThreshold(Texture2D tex, int threshold)
    {
        Texture2D newTex = new Texture2D(tex.width, tex.height);

        Color32[] colors = tex.GetPixels32();
        Color32[] newColors = tex.GetPixels32();

        int index = 0;
        foreach (Color32 c in colors)
        {
            Color newColor;
            if (c.r < threshold)
                newColor = new Color(1, 1, 1, 1);
            else
                newColor = new Color(0, 0, 0, 1);

            newColors[index] = newColor;
            index++;
        }
        newTex.SetPixels32(newColors);
        newTex.Apply(false);
        return newTex;

    }
}
