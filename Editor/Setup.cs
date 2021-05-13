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

    Dictionary<int, string> layerDictionary = new Dictionary<int, string>() 
    {
        {3, "Player"}

    };


    void OnGUI ()
    {
        GUILayout.Label("Welcome to the Classic FPS Pack, \n we need to setup some quick things such as Tags and Layers to get you started!");
       
        GUILayout.Space(20);

        if (GUILayout.Button("Setup Tags"))
        {
            
        }
        GUILayout.Space(20);
        GUILayout.Label("Will Override the Layers you have from index of 0 - 10");
        if (GUILayout.Button("Setup Layers"))
        {
            
        }
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