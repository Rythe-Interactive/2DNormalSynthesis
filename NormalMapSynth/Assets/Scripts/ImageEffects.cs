using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImageMagick;


public abstract class AbstractEffect : ScriptableObject
{

    public bool Collapse = false;
    public uint ExecutionOrder = 0;
    public bool Execute = true;

    protected Texture2D tex;
    protected abstract void ExecuteEffect(ref MagickImage input);
    public void ApplyEffect(ref Texture2D otherTex)
    {
        tex = new Texture2D(otherTex.width, otherTex.height, otherTex.format, false);
        MagickImage tempImage = ImageProcessingHelper.GenerateMagicImage(otherTex);
        ExecuteEffect(ref tempImage);
        ImageProcessingHelper.WriteToTexture(tempImage, ref otherTex);
    }
    public void ApplyEffect(ref Texture2D otherTex, Texture2D mask)
    {
        tex = new Texture2D(otherTex.width, otherTex.height, otherTex.format, false);

        MagickImage tempImage = ImageProcessingHelper.GenerateMagicImage(otherTex);
        ExecuteEffect(ref tempImage);

        ImageProcessingHelper.WriteToTexture(tempImage, ref otherTex, mask);
    }
}
public class BlurEffect : AbstractEffect
{
    public string Name = typeof(BlurEffect).ToString();

    public float Radius = 3;
    public float Sigma = 1;
    protected override void ExecuteEffect(ref MagickImage input)
    {
        input.GaussianBlur(Radius, Sigma);
    }
}

public class EmbossEffect : AbstractEffect
{
    public float Radius = 10;
    public float Sigma = 1;
    public string Name = typeof(EmbossEffect).ToString();
    protected override void ExecuteEffect(ref MagickImage input)
    {
        input.Emboss(Radius, Sigma);
    }
}
public class AdapttiveThreshold : AbstractEffect
{
    public int Width = 4;
    public int Height = 4;
    public float percentage = 0.5f;
    public string Name = typeof(AdapttiveThreshold).ToString();

    protected override void ExecuteEffect(ref MagickImage input)
    {
        input.AdaptiveThreshold(Width, Height, percentage);
    }
}
public class AutoThrehsold : AbstractEffect
{
    public int Mode = 0;
    public float strenght = 0.5f;

    public string Name = typeof(AutoThrehsold).ToString();

    protected override void ExecuteEffect(ref MagickImage input)
    {
        float otherStrength = 1.0f - strenght;
        MagickImage otherImage = new MagickImage(input);

        Texture2D otherTex = new Texture2D(base.tex.width, base.tex.height, base.tex.format, false);
        ImageProcessingHelper.WriteToTexture(otherImage, ref base.tex);

        input.AutoThreshold((AutoThresholdMethod)Mode);
        ImageProcessingHelper.WriteToTexture(input, ref otherTex);

        Texture2D newTex = new Texture2D(otherTex.width, otherTex.height, otherTex.format, false);

        for (int x = 0; x < newTex.width; x++)
        {
            for (int y = 0; y < newTex.height; y++)
            {

                Color a = otherTex.GetPixel(x, y) * strenght + otherStrength * base.tex.GetPixel(x, y);
                newTex.SetPixel(x, y, a);
            }
        }
        newTex.Apply();
        input = ImageProcessingHelper.GenerateMagicImage(newTex);
    }
}

