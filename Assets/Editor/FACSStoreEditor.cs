
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(FACSStore))]
class FACSStoreEditor : Editor
{
    public FACSStore facsStore;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        facsStore = (FACSStore)target;

        EditorGUILayout.HelpBox($"Stored facs: {facsStore.FacialActions.Length}", MessageType.Info);
    }
}