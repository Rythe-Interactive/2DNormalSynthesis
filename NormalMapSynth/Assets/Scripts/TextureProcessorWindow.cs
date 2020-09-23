using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        if (GUI.Button(new Rect(leftPadding, topPadding + rectPos.y + rectSize.y * 0.5f, 100, 25), "Export Texture"))
            ExportTexture();

    }

    private void Update()
    {
        UpdateTextures();
    }
    //update data
    private void UpdateTextures()
    {
        if (m_Texture == null) return;
        Debug.Log("updating textures!");
        m_Heightmap = m_Texture.GenerateHeightMap();
        if (m_blur) GaussianBlur.Blur(ref m_Heightmap, m_BlurRadius);
        m_NormalMap = m_Heightmap.GenerateNormalFromHeight(m_blur, m_NormalDepth);


    }
    private void ExportTexture()
    {
        if (m_Texture == null) return;

        var bytes = m_NormalMap.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "_normal.png", bytes);
        Debug.Log("Succesfully exported!");
    }
}
