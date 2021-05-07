using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using UnityEditor.Audio;
using ClassicFPS.Audio;

namespace ClassicFPS.Editor
{
    // IngredientDrawerUIE
    [CustomPropertyDrawer(typeof(Sound))]
    public class SoundPropertyDrawer : PropertyDrawer
    {   
        float totalHeight = 0;

        int selectedGroup = 0;
        int selectedID = 0;
    
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
    
           EditorGUI.BeginProperty(position,label,property);

            SoundManager[] managers = Resources.FindObjectsOfTypeAll<SoundManager>();
        
            if (managers.Length > 0) {
                var editorLabelRect = new Rect(position.x, position.y, position.width, 20);
                var groupRect = new Rect(position.x, position.y+20+5, position.width, 20);
                var nameRect = new Rect(position.x, position.y+40+5, position.width, 20);
                var labelRect = new Rect(position.x, position.y+60+10, position.width, 20);
                var buttonRect = new Rect(position.x + position.width/2, position.y+80+10, position.width/2-10, 20);
                var button2Rect = new Rect(position.x, position.y+80+10, position.width/2-10, 20);
                List<string> _groups = new List<string>();
                _groups.Add("");
                foreach (SoundGroup group in managers[0].soundGroups)
                {
                    _groups.Add(group.group);
                }
            
                selectedGroup = _groups.IndexOf(property.FindPropertyRelative("group").stringValue);
            
                EditorGUI.LabelField(editorLabelRect,label.text);

                if (selectedGroup < 0) selectedGroup = 0;

                if (selectedGroup >= 0 || _groups.Count > 1) {
                    totalHeight = 60;
                    GUI.contentColor = Color.white;
                    selectedGroup = EditorGUI.Popup(groupRect,"SFX Group",selectedGroup,_groups.ToArray());

                    property.FindPropertyRelative("group").stringValue = _groups[selectedGroup];
                }
                else {
                    EditorGUI.Popup(groupRect,"SFX Group",0,_groups.ToArray());
                }

                if (selectedGroup >= 0 && _groups != null && _groups.Count > 0 && _groups[selectedGroup] != null && _groups[selectedGroup] != "") {
                    totalHeight = 80;
                    List<string> _ids = new List<string>();
                    _ids.Add("");
                    foreach (AudioClip clip in managers[0]._SoundGroup(_groups[selectedGroup]).clips)
                    {
                        if (clip != null)
                        _ids.Add(clip.name);
                    }
                
                    selectedID = _ids.IndexOf(property.FindPropertyRelative("clipName").stringValue);

                    if (selectedID < 0) selectedID = 0;

                    if (selectedID >= 0 || _ids.Count > 1) {
                        GUI.contentColor = Color.white;
                        selectedID = EditorGUI.Popup(nameRect,"Clip Name",selectedID,_ids.ToArray());

                        property.FindPropertyRelative("clipName").stringValue = _ids[selectedID];
                    }
                    else {
                        GUI.contentColor = Color.white;
                        EditorGUI.Popup(nameRect,"Clip Name",0,_ids.ToArray());
                    }   
                }

                if (selectedID <0 || selectedGroup < 0)
                {
                    EditorGUI.LabelField(labelRect, "Either there are no groups or no SFXs under this group. Add more at the Sound Manager.");
                }
            
                if (selectedID > 0 && selectedGroup > 0)
                {
                
                    if (GUI.Button(buttonRect, "Play SFX"))
                    {
                        SoundGroup group = managers[0]._SoundGroup(property.FindPropertyRelative("group").stringValue);

                        for (int i = 0; i < group.clips.Count; i++)
                        {
                            if (group.clips[i].name == property.FindPropertyRelative("clipName").stringValue)
                            {
                                Debug.Log(group.clips[i].name);
                                PlayClip(group.clips[i]);
                                break;
                            }
                        }

                    }
                }

                totalHeight = 120;

                if (GUI.Button(button2Rect, "Clear SFX"))
                {
                    selectedGroup = 0;
                    selectedID = 0;
                    property.FindPropertyRelative("clipName").stringValue = "";
                    property.FindPropertyRelative("group").stringValue = "";
                }

            
                //property.FindPropertyRelative("group").stringValue = EditorGUI.TextField(groupRect,"Group",property.FindPropertyRelative("group").stringValue);
                //property.FindPropertyRelative("clipName").stringValue = EditorGUI.TextField(nameRect,"Clip Name",property.FindPropertyRelative("clipName").stringValue);
            }
            else {
                var groupRect = new Rect(position.x, position.y, position.width, 20);
                var nameRect = new Rect(position.x, position.y+20+5, position.width, 20);
                var labelRect = new Rect(position.x, position.y+40+10, position.width, 20);
                totalHeight = 90;
                property.FindPropertyRelative("group").stringValue = EditorGUI.TextField(groupRect,"Group",property.FindPropertyRelative("group").stringValue);
                property.FindPropertyRelative("clipName").stringValue = EditorGUI.TextField(nameRect,"Clip Name",property.FindPropertyRelative("clipName").stringValue);
            
                EditorGUI.LabelField(labelRect, "You need at least 1 Sound Manager Scriptable Object");
            
            }

           EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return totalHeight;
        }

   

        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
     
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );
 
            Debug.Log(method);
            method.Invoke(
                null,
                new object[] { clip, startSample, loop }
            );
        }
    }
}