using UnityEditor;
using UnityEngine;
using dev.ReiraLab.Runtime;

namespace dev.ReiraLab.Editor
{
    [CustomEditor(typeof(ParameterQueue))]
    public class ParameterQueueEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generator", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate Parameter Queue", GUILayout.Height(40)))
            {
                ParameterQueue parameterQueue = (ParameterQueue)target;
                ParameterQueueGenerator.Generate(parameterQueue);
                EditorUtility.SetDirty(parameterQueue);
            }
        }
    }
}
