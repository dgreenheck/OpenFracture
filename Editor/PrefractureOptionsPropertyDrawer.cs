using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PrefractureOptions))]
public class PrefractureOptionsPropertyDrawer : PropertyDrawer
{   
    private static bool foldout = true;

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var unfreezeAll = property.FindPropertyRelative("unfreezeAll");
        var saveFragmentsToDisk = property.FindPropertyRelative("saveFragmentsToDisk");
        var saveLocation = property.FindPropertyRelative("saveLocation");

        EditorGUI.indentLevel = 0;
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);

        if (foldout)
        {
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(unfreezeAll, new GUIContent("Unfreeze All"));
            EditorGUILayout.PropertyField(saveFragmentsToDisk, new GUIContent("Save Fragments To Disk"));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(saveLocation, new GUIContent("Save Location"));

            if (GUILayout.Button(" . . . ", GUILayout.ExpandWidth(false)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Location", "", "");
                if (path.StartsWith(Application.dataPath))
                {
                    saveLocation.stringValue = "Assets" + path.Substring(Application.dataPath.Length);
                    saveLocation.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    throw new System.ArgumentException("Full path does not contain the current project's Assets folder", "absolutePath");
                }
            }        
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            if (GUILayout.Button("Prefracture Mesh", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(32) }))
            {
                ((Prefracture)property.serializedObject.targetObject).ComputeFracture();
            }
            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUI.indentLevel = 0;
    }
    
    // Hack to prevent extra space at top of property drawer. This is due to using EditorGUILayout
    // in OnGUI, but I don't want to have to manually specify control sizes
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0; }
}