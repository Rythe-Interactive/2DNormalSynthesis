using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ImageMagick;
public class TextureProcessorWindow : EditorWindow
{

    //positioning variables
    private Vector2 size = new Vector2(400, 400);
    private float leftPadding = 10.0f;
    private float topPadding = 10.0f;

    private float m_scale = 2.5f;
    private float offset = 25.0f;

    //configure Normal
    private bool m_DisplayHeightMap = true;
    private float m_NormalDepth = 500.0f;
    private bool m_blur = true;
    private int m_BlurRadius = 2;
    //textures
    private Texture2D m_Texture;
    private Texture2D m_Heightmap;
    private Texture2D m_NormalMap;
    private Texture2D m_ThresholdMap;

    private MagickImageFactory m_MagickFactory;

    //init window
    [MenuItem("Texture Processor/Normal Generator")]
    private static void Init()
    {
        var window = GetWindow<TextureProcessorWindow>("Normal Generator");
        window.position = new Rect(0, 0, 500, 300);
        window.Show();
        Init();
    }

    //display data
    private void OnGUI()
    {
        //get Data
        m_blur = EditorGUI.Toggle
                                (new Rect(leftPadding + offset * 10, topPadding, 200, 20), "Blur", m_blur);

        m_BlurRadius = EditorGUI.IntSlider
                                (new Rect(leftPadding + offset * 10, topPadding + 25, 200, 20), m_BlurRadius, 1, 5);

        m_DisplayHeightMap = EditorGUI.Toggle
                                (new Rect(leftPadding, topPadding, 200, 20), "Show Heightmap", m_DisplayHeightMap);

        m_NormalDepth = EditorGUI.Slider
                                (new Rect(leftPadding, topPadding + 25, 200, 20), "Normal Depth", m_NormalDepth, 1f, 2000f);


        m_scale = EditorGUI.Slider
                                (new Rect(leftPadding, topPadding + 100, 200, 20), "Zoom", m_scale, 1f, 5.0f);

        //update!
        //  UpdateTextures();

        //display Textures
        Vector2 rectPos = new Vector2(leftPadding, 150);
        Vector2 rectSize = new Vector2(200, 200);
        if (m_Texture) rectSize = new Vector2(m_Texture.width * m_scale * 2, m_Texture.height * m_scale * 2);


        m_Texture = (Texture2D)EditorGUI.ObjectField(new Rect(rectPos, rectSize), "texture", m_Texture, typeof(Texture2D));

        rectPos += new Vector2(offset + rectSize.x, 0);

        if (m_Heightmap != null && m_DisplayHeightMap)
        {
            EditorGUI.DrawPreviewTexture(new Rect(rectPos, rectSize * 0.5f), m_Heightmap);
            rectPos += new Vector2(rectSize.x * 0.5f + offset, 0);
        }

        if (m_NormalMap != null)
        {
            EditorGUI.DrawPreviewTexture(new Rect(rectPos, rectSize * 0.5f), m_NormalMap);
        }


        //export
        if (GUI.Button(new Rect(leftPadding, topPadding + rectPos.y + rectSize.y * 0.5f, 125, 25), "Apply!"))
            UpdateWindow();

        rectPos.y += (offset * 1.25f);

        if (GUI.Button(new Rect(leftPadding, topPadding + rectPos.y + rectSize.y * 0.5f, 125, 25), "Export Texture!"))
            ExportTexture();

    }

    private void UpdateWindow()
    {
        if (m_Texture == null) return;
        UpdateTextures();
        ApplyGrayScale();
        ApplyBlur();
        GenerateNormal();

    }
    private void ApplyBlur()
    {
        if (m_blur == false) return;
        if (m_Heightmap == null) return;
        MagickImage newImage = GenerateMagicImage(m_Heightmap);

        newImage.AdaptiveBlur(m_BlurRadius);
        WriteToTexture(newImage, ref m_Heightmap);
    }
    private void WriteToTexture(MagickImage input, ref Texture2D output)
    {
        //temp write to PNG
        input.Write(Application.dataPath + @"\TestImageExport.png");

        //read bytes form temp PNG
        byte[] rawDataFromDisc = System.IO.File.ReadAllBytes(Application.dataPath + @"\TestImageExport.png");
        //read bytes into texture && discard all alpha 0 values
        output.LoadImage(rawDataFromDisc);
        output.Mask(m_Texture);
        output.Apply();
        System.IO.File.Delete(Application.dataPath + @"\TestImageExport.png");
    }
    private MagickImage GenerateMagicImage(Texture2D tex)
    {
        //make null checks
        if (tex == null) return null;
        if (m_MagickFactory == null) m_MagickFactory = new MagickImageFactory();
        //create Magic image
        var temp = m_MagickFactory.Create(tex.EncodeToPNG());
        temp.Format = MagickFormat.Png32;
        MagickImage image = new MagickImage(temp);
        return image;
    }

    private void ApplyGrayScale()
    {
        //return if input to grayScale is null
        MagickImage tempImage = GenerateMagicImage(m_Texture);
        //copy image
        MagickImage grayScaleImage = tempImage;

        //apply gray scale
        grayScaleImage.Grayscale();

        WriteToTexture(grayScaleImage, ref m_Heightmap);
    }
    private void GenerateNormal()
    {
        m_NormalMap = m_Heightmap.GenerateNormalFromHeight(m_NormalDepth);
    }

    private void Blur(Texture2D tex)
    {

    }
    //update data
    private void UpdateTextures()
    {
        if (m_Texture == null) return;
        m_Heightmap = new Texture2D(m_Texture.width, m_Texture.height, m_Texture.format, false);
        m_Heightmap.Apply();

        //  Debug.Log("updating textures!");
        //  m_Heightmap = m_Texture.GenerateHeightMap();
        //   if (m_blur) GaussianBlur.Blur(ref m_Heightmap, m_BlurRadius);
        //   m_ThresholdMap = OtsuThreshold.ApplyThreshold(m_Heightmap, (int)OtsuThreshold.GetThreshold(m_Heightmap));
        //   m_NormalMap = m_Heightmap.GenerateNormalFromHeight(m_blur, m_NormalDepth);
    }
    private void ExportTexture()
    {
        if (m_Texture == null) return;
        UpdateWindow();
        var bytes = m_NormalMap.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "_normal.png", bytes);
        Debug.Log("Succesfully exported!");
    }
}
