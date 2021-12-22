using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CallbackOptions))]
public class CallbackOptionsPropertyDrawer : PropertyDrawer
{   
    private static bool foldout = true;

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);

        if (foldout)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("onCompleted"));
        }
        
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    
    // Hack to prevent extra space at top of property drawer. This is due to using EditorGUILayout
    // in OnGUI, but I don't want to have to manually specify control sizes
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0; }
}