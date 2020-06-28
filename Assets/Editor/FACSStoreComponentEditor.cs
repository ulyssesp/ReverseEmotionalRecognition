using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(FACSStoreComponent))]
class FACSStoreComponentEditor : Editor
{
    private FACSStoreComponent facsStore;

    protected void OnEnable()
    {
        facsStore = (FACSStoreComponent)target;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(Application.persistentDataPath + facsStore.FACStoreJSONPath), facsStore.facsstore);
        Debug.Log($"Loaded {facsStore.facsstore.FacialActions.Length} facial actions");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("Use the button below to create a snapshot of the expression. All changed vectors will be recorded", MessageType.Info);

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create Snapshot"))
        {
            AssetDatabase.Refresh();
            List<Transform> changed = FindRecursiveChanged(facsStore.Root);
            facsStore.facsstore.AddFacialAction(facsStore.FacName, FacialAction.fromTransforms(changed));
            // Undo.RecordObject(facsStore.facsstore, "Added facial action");
            // AssetDatabase.SaveAssets();
            string jsondata = JsonUtility.ToJson(facsStore.facsstore);
            File.WriteAllText(Application.persistentDataPath + facsStore.FACStoreJSONPath, jsondata);

            foreach (Transform change in changed)
            {
                change.localPosition = Vector3.zero;
            }
        }

        if (GUILayout.Button("Reset"))
        {
            List<Transform> changed = FindRecursiveChanged(facsStore.Root);
            foreach (Transform change in changed)
            {
                change.localPosition = Vector3.zero;
            }
        }

        EditorGUI.EndChangeCheck();
    }

    static List<Transform> FindRecursiveChanged(Transform transform)
    {
        if (transform.childCount == 0)
        {
            if (transform.localPosition.sqrMagnitude > 0)
            {
                return new List<Transform>() { transform };
            }
            return new List<Transform>();
        }
        else
        {
            List<Transform> arr = new List<Transform>(transform.childCount);
            for (int i = 0; i < transform.childCount; i++)
            {
                arr.AddRange(FindRecursiveChanged(transform.GetChild(i)));
            }

            return arr;
        }
    }
}