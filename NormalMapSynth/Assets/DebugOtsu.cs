using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImageMagick;
using System;
using System.IO;
public class DebugOtsu : MonoBehaviour
{
    public int ThresholdWidth = 4;
    public int ThresholdHeight = 4;
    public float thresholdPercentage = 0.5f;
    public Texture2D tex;
    public string Name = "MagickTest";
    public string Path = @"C:\Images\MagickTest.png";
    public MagickImageFactory mIFactory;
    // Start is called before the first frame update
    void Start()
    {
        mIFactory = new MagickImageFactory();
        //  using (var image = new MagickImage(@"C:\Images\TestImage.png"))
        using (var image = mIFactory.Create(tex.EncodeToPNG()))
        {

            image.Format = MagickFormat.Png32;
            image.AdaptiveThreshold(ThresholdWidth, ThresholdHeight, thresholdPercentage);
            image.Write(@"C:\Images\TestImageExport.png");
            //  mIFactory.Create(image);
            // MagickImg.AdaptiveThreshold(ThresholdWidth, ThresholdHeight, thresholdPercentage);

        }

        //if (tex == null) return;
        //Debug.Log("Reading texture Data");
        //byte[] ImageData = tex.GetRawTextureData();
        //Debug.Log("Creating factory");
        //Debug.Log("Creating magick image");
        //var MagickImg = mIFactory.Create(ImageData);
        //Debug.Log("thresholding");
        //
        //Debug.Log("Creating file");

        //IWriteDefines defines;
        //try
        //{
        //    using (System.IO.FileStream fs = System.IO.File.OpenRead(Path))
        //    {
        //        MagickImg.Write(fs, MagickFormat.Png);
        //    }
        //}
        //catch (Exception e)
        //{
        //    Debug.Log(e);
        //}
        //Debug.Log("writing file");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
