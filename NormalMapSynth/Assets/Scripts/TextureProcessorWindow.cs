using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ImageMagick;
using System.Reflection;
using System;
using System.Linq;
public class TextureProcessorWindow : EditorWindow
{
    public enum Effects
    {
        EMBOSS,
        BLUR,
        ADAPTIVE_THRESHOLD
    }
    //positioning variables
    private Vector2 size = new Vector2(400, 400);
    private float leftPadding = 10.0f;
    private float topPadding = 10.0f;

    private float m_scale = 1.75f;
    private float offsetX = 225.0f;
    private float offsetY = 30.0f;
    //configure Normal
    private bool m_DisplayHeightMap = true;
    private float m_NormalDepth = 500.0f;

    private string m_FileName = "NAME";
    //textures
    private Texture2D m_Texture;
    private Texture2D m_Heightmap;
    private Texture2D m_NormalMap;

    private MagickImageFactory m_MagickFactory;


    private List<AbstractEffect> m_effectsList = new List<AbstractEffect>();
    private List<SerializedObject> m_serializedObjects;

    private Dictionary<SerializedObject, AbstractEffect> m_SerializedObject_effectMap;
    private Dictionary<SerializedObject, List<SerializedProperty>> m_SerializedObject_PropertyMap;
    //init window
    [MenuItem("Texture Processor/Normal Generator")]
    private static void Init()
    {
        var window = GetWindow<TextureProcessorWindow>("Normal Generator");
        window.position = new Rect(25, 25, 1000, 900);
        window.Show();
    }
    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
    private void InitEffects()
    {
        //init lists
        List<Type> typeList = new List<Type>();
        m_effectsList = new List<AbstractEffect>();
        m_serializedObjects = new List<SerializedObject>();
        m_SerializedObject_effectMap = new Dictionary<SerializedObject, AbstractEffect>();
        m_SerializedObject_PropertyMap = new Dictionary<SerializedObject, List<SerializedProperty>>();

        //find all Implementations of the abstract class "AbstractEffect"
        foreach (Type effect in Assembly.GetAssembly(typeof(AbstractEffect)).GetTypes()
            .Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(typeof(AbstractEffect))))
            typeList.Add(effect);


        //create instances for each Effect && store the effect & the instance in a list
        foreach (Type currentType in typeList)
        {
            var effect = CreateInstance(currentType);
            m_effectsList.Add((AbstractEffect)effect);
            m_serializedObjects.Add(new SerializedObject(effect));
        }


        //create serialized properties to display in editor window
        int index = 0;
        foreach (SerializedObject SObject in m_serializedObjects)
        {
            FieldInfo[] fieldInfo;
            AbstractEffect currentEffect = m_effectsList[index];
            //   m_SerializedObject_effectMap.Add(SObject, currentEffect);
            fieldInfo = currentEffect.GetType().GetFields();
            m_SerializedObject_effectMap.Add(SObject, currentEffect);
            List<SerializedProperty> tempList = new List<SerializedProperty>();

            foreach (FieldInfo info in fieldInfo)
            {
                // m_serializedPropertiyList.Add(SObject.FindProperty(info.Name));
                SerializedProperty tempProperty = SObject.FindProperty(info.Name);

                if (info.Name == "ExecutionOrder")
                {
                    tempProperty.intValue = index;
                    index++;
                }

                tempList.Add(tempProperty);
            }
            SObject.ApplyModifiedProperties();
            m_SerializedObject_PropertyMap.Add(SObject, tempList);

        }
    }
    public void Awake()
    {
        ClearLog();
        InitEffects();
    }
    private SerializedProperty FindProperty(string name, List<SerializedProperty> properties)
    {
        foreach (SerializedProperty property in properties)
            if (property.name == name) return property;

        return null;
    }
    //display data
    private void OnGUI()
    {
        int index = 0;
        //display properties of effects
        foreach (SerializedObject sObject in m_SerializedObject_PropertyMap.Keys)
        {
            //find Name of effect
            SerializedProperty NameProperty = FindProperty("Name", m_SerializedObject_PropertyMap[sObject]);
            string EffectName = "Missing Name Field";
            if (NameProperty != null) EffectName = NameProperty.stringValue;
            //dispaly Effect label && collapse checkbox
            EditorGUI.LabelField(new Rect(leftPadding, topPadding + offsetY * index, 200, 20), EffectName);


            bool collapse = false;
            //skim for Collapse bool
            SerializedProperty collapseProperty = FindProperty("Collapse", m_SerializedObject_PropertyMap[sObject]);
            if (collapseProperty != null)
            {
                EditorGUI.PropertyField
                (new Rect(leftPadding + offsetX, topPadding + offsetY * index, 200, 20), collapseProperty, new GUIContent(collapseProperty.name));
                sObject.ApplyModifiedProperties();
                collapse = collapseProperty.boolValue;
                index++;
            }

            //skip rest of property dispaly if collapse is true
            if (collapse) continue;

            //display rest of properties skip collapse
            foreach (SerializedProperty property in m_SerializedObject_PropertyMap[sObject])
            {
                //skip properties
                if (property.name == "Collapse" || property.name == "Name") continue;
                EditorGUI.PropertyField
                (new Rect(leftPadding, topPadding + offsetY * index, 200, 20), property, new GUIContent(property.name));

                index++;
            }
            index++;
            //apply changed properties
            sObject.ApplyModifiedProperties();
        }

        //display Textures
        Vector2 rectPos = new Vector2(leftPadding + offsetX * 2.25f, topPadding);
        Vector2 rectSize = new Vector2(200, 200);

        m_FileName = EditorGUI.TextField(new Rect(rectPos.x + offsetX, rectPos.y, 200, 20), "File Name", m_FileName);

        //scale textures
        m_scale = EditorGUI.Slider(new Rect(rectPos, new Vector2(200, 20)), "Zoom", m_scale, 1f, 2.5f);
        rectPos.y += offsetY;

        if (m_Texture) rectSize = new Vector2(m_Texture.width * m_scale * 2, m_Texture.height * m_scale * 2);
        m_Texture = (Texture2D)EditorGUI.ObjectField(new Rect(rectPos, rectSize), "texture", m_Texture, typeof(Texture2D));

        rectPos.y += rectSize.y * 0.5f + offsetY * 0.5f;
        rectPos.x += offsetX * 0.66f;

        if (m_Heightmap != null && m_DisplayHeightMap)
        {
            EditorGUI.DrawPreviewTexture(new Rect(rectPos, rectSize * 0.5f), m_Heightmap);
            rectPos.y += rectSize.y * 0.5f + offsetY * 0.5f;
        }

        if (m_NormalMap != null)
        {
            EditorGUI.DrawPreviewTexture(new Rect(rectPos, rectSize * 0.5f), m_NormalMap);
        }


        //export // Buttons
        if (GUI.Button(new Rect(leftPadding, topPadding + offsetY * index, 125, 25), "Apply!"))
            UpdateWindow();
        index++;

        if (GUI.Button(new Rect(leftPadding, topPadding + offsetY * index, 125, 25), "Export Texture!"))
            ExportTexture();

    }

    private void UpdateWindow()
    {
        if (m_Texture == null) return;
        UpdateTextures();
        ApplyGrayScale();
        Execute();
        GenerateNormal();
    }

    private void Execute()
    {
        //create temporary dictionary 
        Dictionary<int, AbstractEffect> tempDictionary = new Dictionary<int, AbstractEffect>();
        //read in data
        foreach (SerializedObject sObj in m_SerializedObject_effectMap.Keys)
        {
            SerializedProperty executeProperty = FindProperty("Execute", m_SerializedObject_PropertyMap[sObj]);
            if (executeProperty.boolValue == false) continue;

            SerializedProperty property = FindProperty("ExecutionOrder", m_SerializedObject_PropertyMap[sObj]);
            tempDictionary.Add(property.intValue, m_SerializedObject_effectMap[sObj]);
        }
        //sort 
        var mylist = tempDictionary.ToList();
        mylist.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));
        //Execute
        foreach (var obj in mylist)
            obj.Value.ApplyEffect(ref m_Heightmap, m_Texture);
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


    //update data
    private void UpdateTextures()
    {
        if (m_Texture == null) return;
        m_Heightmap = new Texture2D(m_Texture.width, m_Texture.height, m_Texture.format, false);
        m_Heightmap.Apply();
    }
    private void ExportTexture()
    {
        if (m_Texture == null) return;
        UpdateWindow();
        var bytes = m_NormalMap.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + @"\" + m_FileName + ".png", bytes);
        Debug.Log("Succesfully exported!");
    }
}
