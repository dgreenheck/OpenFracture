using UnityEditor;
using UnityEngine;

// IngredientDrawerUIE
[CustomPropertyDrawer(typeof(TriggerOptions))]
public class TriggerOptionsPropertyDrawer : PropertyDrawer
{   
    private static bool foldout = true;

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Create property container element.
        var minimumCollisionForce = property.FindPropertyRelative("minimumCollisionForce");
        var triggerType = property.FindPropertyRelative("triggerType");
        var triggerKey = property.FindPropertyRelative("triggerKey");
        var filterCollisionsByTag = property.FindPropertyRelative("filterCollisionsByTag");
        var triggerAllowedTags = property.FindPropertyRelative("triggerAllowedTags");

        EditorGUI.indentLevel = 0;
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);

        if (foldout)
        {
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(triggerType);

            switch (triggerType.enumValueIndex)
            {
                case ((int)TriggerType.Collision):
                    EditorGUILayout.PropertyField(minimumCollisionForce);
                    EditorGUILayout.PropertyField(filterCollisionsByTag, new GUIContent("Limit collisions to selected tags?"));
                    if (filterCollisionsByTag.boolValue)
                    {
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        EditorGUILayout.PropertyField(triggerAllowedTags, new GUIContent("Included Tags"));
                    }
                    break;
                case ((int)TriggerType.Trigger):
                    EditorGUILayout.PropertyField(filterCollisionsByTag);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.PropertyField(triggerAllowedTags);
                    break;
                case ((int)TriggerType.Keyboard):
                    EditorGUILayout.PropertyField(triggerKey);
                    break;
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUI.indentLevel = 0;
    }

    // Hack to prevent extra space at top of property drawer. This is due to using EditorGUILayout
    // in OnGUI, but I don't want to have to manually specify control sizes
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0; }
}