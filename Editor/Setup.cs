using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Setup : UnityEditor.EditorWindow
{
    GUIStyle textStyle = EditorStyles.label;

    [MenuItem("Classic FPS/Setup")]
    static void Init ()
    {
        Setup window = (Setup)EditorWindow.GetWindow(typeof(Setup));
        window.Show();

    }
    
    string[] expectedLayers = new string[] 
    {
        "Player",
        "Hidden",
        "Pushable",
        "Enemy",
        "EnemyProjectile"
    };

    string[] expectedTags = new string[]
    {
        "Respawn",
        "Finish",
        "Player",
        "GameController",
        "Pushable",
        "Crosshair",
        "GameManager",
        "Interactable",
        "SpawnPoint",
        "Weapon Mount",
        "Enemy",
        "Terrain",
        "Bridge",
        "Stairs"
    };

    Dictionary<int, string> layerDictionary = new Dictionary<int, string>() 
    {
        {3, "Player"},
        {7, "Pushable"},
        {8, "Enemy"},
        {9, "EnemyProjectile"} 
    };


    void OnGUI ()
    {
        GUILayout.Label("Welcome to the Classic FPS Pack, \n we need to setup some quick things such as Tags and Layers to get you started!");
       
        GUILayout.Space(20);

        if (GUILayout.Button("Setup Tags"))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            foreach (string tag in expectedTags)
            {
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                    if (t.stringValue.Equals(tag)) { found = true; break; }
                }

                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                    n.stringValue = tag;
                }
            }

            tagManager.ApplyModifiedProperties();

        }
        GUILayout.Space(20);
        GUILayout.Label("Will Override the Layers you have from index of 0 - 10");
        if (GUILayout.Button("Setup Layers"))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            foreach (KeyValuePair<int,string> pair in layerDictionary)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(pair.Key);
                if (sp != null) sp.stringValue = pair.Value;
            }

            tagManager.ApplyModifiedProperties();

        }

        GUILayout.Space(20);
        GUILayout.Label("Follow the remaining steps in the Setup Guide tutorial to finish the project setup! Enjoy the pack!");
    }
}

    [InitializeOnLoad]
    public static class LayerUtils
    {
        static LayerUtils()
        {
            CreateLayer();
        }

        static void EditLayerAt (int index, SerializedProperty layers, string newName)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(index);
            if (layerSP.stringValue != newName)
            {
                Debug.Log("Setting up layers.  Layer " + index.ToString() + " is now called " + newName);
                layerSP.stringValue = newName;
            }
        }
 
        static void CreateLayer()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
 
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                Debug.LogWarning("Can't set up the layers.  It's possible the format of the layers and tags data has changed in this version of Unity.");
                Debug.LogWarning("Layers is null: " + (layers == null));
                return;
            }
            

            
 
            tagManager.ApplyModifiedProperties();
        }
    }