using UnityEditor;
using UnityEngine;

// IngredientDrawerUIE
[CustomPropertyDrawer(typeof(FractureOptions))]
public class FractureOptionsPropertyDrawer : PropertyDrawer
{   
    private static bool foldout = true;

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.indentLevel = 0;
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);

        if (foldout)
        {
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(property.FindPropertyRelative("fragmentCount"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("asynchronous"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("detectFloatingFragments"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("xAxis"), new GUIContent("Fracture Along X Plane"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("yAxis"), new GUIContent("Fracture Along Y Plane"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("zAxis"), new GUIContent("Fracture Along Z Plane"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("insideMaterial"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("textureScale"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("textureOffset"));
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUI.indentLevel = 0;
    }
    
    // Hack to prevent extra space at top of property drawer. This is due to using EditorGUILayout
    // in OnGUI, but I don't want to have to manually specify control sizes
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0; }
}