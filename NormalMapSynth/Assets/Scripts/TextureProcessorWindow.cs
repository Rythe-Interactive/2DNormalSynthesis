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
    private Vector2 m_size = new Vector2(400, 400);
    private float m_leftPadding = 10.0f;
    private float m_topPadding = 10.0f;
    private float m_rowSpacing = 25.0f;
    private Vector2 m_ButtonSizeSmall = new Vector2(75, 20);
    private Vector2 m_ButtonSizeLarge = new Vector2(150, 25);


    private int m_executionOrder = 0;


    private float m_scale = 1.75f;
    private float offsetX = 225.0f;
    private float offsetY = 30.0f;
    //configure Normal
    private bool m_DisplayHeightMap = true;
    private float m_NormalDepth = 500.0f;

    private string m_FileName = "NAME";
    private int m_selectedEffectIndex;
    //textures
    private Texture2D m_Texture;
    private Texture2D m_Heightmap;
    private Texture2D m_NormalMap;

    private MagickImageFactory m_MagickFactory;

    private string[] m_EffectsDropDown;
    private List<AbstractEffect> m_effectsList = new List<AbstractEffect>();
    private List<SerializedObject> m_serializedObjects;

    private Dictionary<SerializedObject, AbstractEffect> m_SerializedObject_effectMap;
    private Dictionary<int, List<SerializedProperty>> m_indexProperty_map;

    private List<SerializedObject> m_ActiveEffects;
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
        m_indexProperty_map = new Dictionary<int, List<SerializedProperty>>();
        m_ActiveEffects = new List<SerializedObject>();
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


        m_EffectsDropDown = new string[m_effectsList.Count];
        int i = 0;
        foreach (AbstractEffect effect in m_effectsList)
        {
            m_EffectsDropDown[i] = effect.ToString();
            i++;
        }
        m_selectedEffectIndex = 0;
        m_executionOrder = 0;
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
    private void AddEffect()
    {
        if (m_effectsList == null) return;

        //init effect list & fetch effect based on selected index
        if (m_ActiveEffects == null) m_ActiveEffects = new List<SerializedObject>();
        AbstractEffect currentEffect = m_effectsList[m_selectedEffectIndex];

        //init serialized object list & add new serialized object
        if (m_serializedObjects == null) m_serializedObjects = new List<SerializedObject>();
        SerializedObject currentSObj = new SerializedObject(currentEffect);
        m_serializedObjects.Add(currentSObj);
        m_ActiveEffects.Add(currentSObj);

        //Store serialized object & effect in a dictionary
        if (m_SerializedObject_effectMap == null) m_SerializedObject_effectMap = new Dictionary<SerializedObject, AbstractEffect>();
        m_SerializedObject_effectMap.Add(currentSObj, currentEffect);

        //store properties in a dictionary, also set execution order 
        if (m_indexProperty_map == null) m_indexProperty_map = new Dictionary<int, List<SerializedProperty>>();

        FieldInfo[] fieldInfo;
        fieldInfo = currentEffect.GetType().GetFields();
        List<SerializedProperty> tempList = new List<SerializedProperty>();
        foreach (FieldInfo info in fieldInfo)
        {
            SerializedProperty tempProperty = currentSObj.FindProperty(info.Name);
            if (info.Name == "ExecutionOrder")
            {
                tempProperty.intValue = m_executionOrder;
            }
            if (info.Name == "ID")
            {
                tempProperty.intValue = m_executionOrder;
            }
            tempList.Add(tempProperty);
        }
        currentSObj.ApplyModifiedProperties();
        m_indexProperty_map.Add(m_executionOrder, tempList);

        m_executionOrder++;
    }
    private void RemoveObject(SerializedObject selectedObject)
    {
        int id = selectedObject.FindProperty("ID").intValue;
        m_indexProperty_map.Remove(id);
        m_ActiveEffects.Remove(selectedObject);
        m_SerializedObject_effectMap.Remove(selectedObject);
    }
    //display data
    private void OnGUI()
    {

        ///
        ///Adding effects
        ///
        GUI.BeginGroup(new Rect(m_leftPadding, m_topPadding, 300, 200));
        m_selectedEffectIndex = EditorGUILayout.Popup("Effect", m_selectedEffectIndex, m_EffectsDropDown);
        if (GUI.Button(new Rect(new Vector2(0, m_topPadding + m_rowSpacing), m_ButtonSizeSmall), "Add Effect"))
            AddEffect();
        GUI.EndGroup();

        ///
        ///Displaying effects & their properties
        ///
        GUI.BeginGroup(new Rect(m_leftPadding, m_topPadding + 100, 450, 600));
        //display properties of effects
        int index = 0;
        if (m_ActiveEffects != null)
            if (m_ActiveEffects.Count > 0)
                foreach (SerializedObject sObject in m_ActiveEffects)
                {
                    int id = sObject.FindProperty("ID").intValue;
                    //find Name of effect
                    SerializedProperty NameProperty = FindProperty("Name", m_indexProperty_map[id]);
                    string EffectName = "Missing Property Field";
                    if (NameProperty != null) EffectName = NameProperty.stringValue;
                    //dispaly Effect label && collapse checkbox
                    EditorGUI.LabelField(new Rect(0, offsetY * index * 0.88f, 200, 20), EffectName);

                    bool collapse = false;
                    //skim for Collapse bool
                    SerializedProperty collapseProperty = FindProperty("Collapse", m_indexProperty_map[id]);
                    if (collapseProperty != null)
                    {
                        EditorGUI.PropertyField
                        (new Rect(offsetX, offsetY * index * 0.88f, 200, 20), collapseProperty, new GUIContent(collapseProperty.name));
                        sObject.ApplyModifiedProperties();
                        collapse = collapseProperty.boolValue;
                    }
                    index++;

                    //skip rest of property dispaly if collapse is true
                    if (collapse) continue;

                    //remove effect, call onGUI again and return this OnGUI since the collection of effects has been modified
                    if (GUI.Button(new Rect(new Vector2(offsetX, offsetY * index), m_ButtonSizeSmall), "Remove"))
                    {
                        RemoveObject(sObject);
                        OnGUI();
                        return;
                    }

                    //  display rest of properties skip collapse
                    foreach (SerializedProperty property in m_indexProperty_map[id])
                    {
                        //skip properties
                        if (property.name == "Collapse" || property.name == "Name" || property.name == "ID") continue;
                        EditorGUI.PropertyField
                        (new Rect(0, offsetY * index * 0.88f, 200, 20), property, new GUIContent(property.name));

                        index++;
                    }
                    index++;
                    //apply changed properties
                    sObject.ApplyModifiedProperties();
                }
        GUI.EndGroup();

        ///
        ///Display textures & zoom lvl & file name
        ///
        GUI.BeginGroup(new Rect(m_leftPadding + 350, m_topPadding, 800, 800));
        //display Textures
        Vector2 rectPos = new Vector2(0, 0);
        Vector2 rectSize = new Vector2(200, 200);

        m_FileName = EditorGUI.TextField(new Rect(rectPos.x + offsetX, rectPos.y, 200, 20), "File Name", m_FileName);

        //scale textures
        m_scale = EditorGUI.Slider(new Rect(rectPos, new Vector2(200, 20)), "Zoom", m_scale, 1f, 3.0f);
        rectPos.y += offsetY;

        if (m_Texture) rectSize = new Vector2(m_Texture.width * m_scale * 1.5f, m_Texture.height * m_scale * 1.5f);
        m_Texture = (Texture2D)EditorGUI.ObjectField(new Rect(rectPos, rectSize), "texture", m_Texture, typeof(Texture2D));

        rectSize *= 0.5f;
        // rectPos.y += rectSize.y * 0.5f + offsetY * 0.5f;
        rectPos.y += rectSize.y + offsetY * m_scale;

        rectPos.x += offsetX * 0.66f;

        if (m_Heightmap != null && m_DisplayHeightMap)
        {
            EditorGUI.DrawPreviewTexture(new Rect(rectPos, rectSize), m_Heightmap);
            rectPos.y += rectSize.y + offsetY * m_scale * 0.66f;
        }

        if (m_NormalMap != null)
        {
            EditorGUI.DrawPreviewTexture(new Rect(rectPos, rectSize), m_NormalMap);
        }
        GUI.EndGroup();

        ///
        ///Buttons for applying effect & exporting 
        ///
        GUI.BeginGroup(new Rect(m_leftPadding + 800, m_topPadding, 400, 400));
        //export // Buttons
        if (GUI.Button(new Rect(Vector2.zero, m_ButtonSizeLarge), "Clear all  Effects!"))
            RemoveAllEffects();
        if (GUI.Button(new Rect(new Vector2(0, offsetY), m_ButtonSizeLarge), "Apply Effects!"))
            UpdateWindow();
        if (GUI.Button(new Rect(new Vector2(0, offsetY * 2), m_ButtonSizeLarge), "Export Texture!"))
            ExportTexture();
        GUI.EndGroup();

    }
    private void RemoveAllEffects()
    {
        m_indexProperty_map.Clear();
        m_ActiveEffects.Clear();
        m_indexProperty_map.Clear();
        m_SerializedObject_effectMap.Clear();
        m_executionOrder = 0;
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
        //foreach (SerializedObject sObject in m_ActiveEffects)
        //{
        //    //find Name of effect
        //    SerializedProperty NameProperty = FindProperty("Name", m_SerializedObject_PropertyMap[sObject]);
        //    string EffectName = "Missing Property Field";
        //    if (NameProperty != null) EffectName = NameProperty.stringValue;
        //    //dispaly Effect label && collapse checkbox
        //    EditorGUI.LabelField(new Rect(0, offsetY * index * 0.88f, 200, 20), EffectName);


        if (m_SerializedObject_effectMap == null) return;
        foreach (SerializedObject sObj in m_ActiveEffects)
        {
            int id = sObj.FindProperty("ID").intValue;
            SerializedProperty executeProperty = FindProperty("Execute", m_indexProperty_map[id]);
            if (executeProperty.boolValue == false) continue;

            SerializedProperty property = FindProperty("ExecutionOrder", m_indexProperty_map[id]);
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
