using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Graph))]
public class GraphEditorScript : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Graph script = (Graph)target;
        if (GUILayout.Button("Draw Graph"))
        {
            script.DrawGraph();
        }
    }


}
