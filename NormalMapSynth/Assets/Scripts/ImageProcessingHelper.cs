using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImageMagick;
public static class ImageProcessingHelper
{
    public static MagickImageFactory magickFactory = new MagickImageFactory();
    public static void WriteToTexture(MagickImage input, ref Texture2D output, Texture2D mask = null)
    {
        //temp write to PNG
        input.Write(Application.dataPath + @"\TestImageExport.png");

        //read bytes form temp PNG
        byte[] rawDataFromDisc = System.IO.File.ReadAllBytes(Application.dataPath + @"\TestImageExport.png");
        //read bytes into texture && discard all alpha 0 values
        output.LoadImage(rawDataFromDisc);
        if (mask != null) output.Mask(mask);

        output.Apply();
        System.IO.File.Delete(Application.dataPath + @"\TestImageExport.png");
    }
    public static MagickImage GenerateMagicImage(Texture2D tex)
    {
        //make null checks
        if (tex == null) return null;
        //  if (magickFactory == null) magickFactory = new MagickImageFactory();
        //create Magic image
        var temp = magickFactory.Create(tex.EncodeToPNG());
        temp.Format = MagickFormat.Png32;
        MagickImage image = new MagickImage(temp);
        return image;
    }
}
