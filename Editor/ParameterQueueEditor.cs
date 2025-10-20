using UnityEditor;
using UnityEngine;
using dev.ReiraLab.Runtime;

[CustomEditor(typeof(ParameterQueue))]
public class ParameterQueueEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Parameter Queue", GUILayout.Height(40)))
        {
            ParameterQueue parameterQueue = (ParameterQueue)target;
            parameterQueue.GenerateParameterQueue();
            EditorUtility.SetDirty(parameterQueue);
        }
    }
}
