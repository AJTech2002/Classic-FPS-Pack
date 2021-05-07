using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using UnityEditor.Audio;
using ClassicFPS.Door_System;

namespace ClassicFPS.Editor
{
    // IngredientDrawerUIE
    [CustomPropertyDrawer(typeof(KeyReference))]
    public class KeyPropertyDrawer : PropertyDrawer
    {   
        float totalHeight = 0;

        int selectedGroup = 0;
        int selectedID = 0;
    
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
    
           EditorGUI.BeginProperty(position,label,property);

            KeyManager[] managers = Resources.FindObjectsOfTypeAll<KeyManager>();
        
            if (managers.Length > 0) {
                var editorLabelRect = new Rect(position.x, position.y, position.width, 20);
                var groupRect = new Rect(position.x, position.y+20+5, position.width, 20);
            
                List<string> _groups = new List<string>();
                _groups.Add("");
                foreach (KeySettings group in managers[0].keys)
                {
                    _groups.Add(group.keyID);
                }
            
                selectedGroup = _groups.IndexOf(property.FindPropertyRelative("keyReference").stringValue);
            
                EditorGUI.LabelField(editorLabelRect,label.text);

                if (selectedGroup < 0) selectedGroup = 0;

                if (selectedGroup >= 0 || _groups.Count > 1) {
                    totalHeight = 60;
                    GUI.contentColor = Color.white;
                    selectedGroup = EditorGUI.Popup(groupRect,"Key",selectedGroup,_groups.ToArray());

                    property.FindPropertyRelative("keyReference").stringValue = _groups[selectedGroup];
                }
                else {
                    EditorGUI.Popup(groupRect,"Key",0,_groups.ToArray());
                }
            }
            else {
                var labelRect = new Rect(position.x, position.y, position.width, 20);
                EditorGUI.LabelField(labelRect, "You need at least 1 Key Manager Scriptable Object");
            }

           EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return totalHeight;
        }

   
    }
}