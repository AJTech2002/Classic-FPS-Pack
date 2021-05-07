using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ClassicFPS.Dialogue;

[CustomEditor(typeof(Dialogue))]
[CanEditMultipleObjects]
public class DialogueEditor : Editor
{
    SerializedProperty Dialogue;
    Dialogue dialogue;
    GUIStyle style = new GUIStyle();

    void OnEnable()
    {
        dialogue = (Dialogue)target;
        
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

    }

    private int currentIndex {
        get {
            return dialogue.currentIndex;
        }
        set{
            dialogue.currentIndex = value;
        }
    }
    private bool showProps {
        get {
            return dialogue.showProperties;

        }
        set {
            dialogue.showProperties = value;
        }
    }
    private int lastIndex = -1; 
    public string tempString;
    private string lastCommandsString = "--";
    private string lastTempString = "--";

    public override void OnInspectorGUI()
    {

        if (target != null) {

            if (dialogue.interactions.Count > 0 && currentIndex == -1 && dialogue.interactions[0].isGhost == false) currentIndex = 0;

            showProps = EditorGUILayout.Foldout(showProps, showProps?"Hide Properties":"Show Properties");
            
            if (showProps) {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);

            //TextAsset dialogueData = Resources.Load<TextAsset>("Dialogue/"+dialogue.name);
            

            }

                EditorGUILayout.Space(10);
            dialogue.showSave = EditorGUILayout.Foldout(dialogue.showSave, dialogue.showSave?"Hide File Options":"Show File Options");

            if (dialogue.showSave)
            {
                if (!File.Exists(Application.dataPath+"/Resources/Dialogue/"+dialogue.name+".txt"))
                {   
                    EditorGUILayout.LabelField("Dialogue File Doesn't Exist");
                    LoadSave(false, null);
                }
                else {
                    string dialogueData = File.ReadAllText(Application.dataPath+"/Resources/Dialogue/"+dialogue.name+".txt");
                    EditorGUILayout.LabelField("Found Dialogue File");
                    LoadSave(true, dialogueData);
                }

                EditorGUILayout.Space(20);
            }

            EditorGUILayout.Space(10);

                if (dialogue.interactions.Count == 0) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Start NPC Dialogue"))
                {
                    dialogue.interactions.Add(new DialogueInteraction(index: 0, isPlayer: false, line: "Starting Line"));
                    currentIndex = 0;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);

            if (currentIndex >= 0) {
      
                if (dialogue.interactions[currentIndex].parentIndex != -1) {
                    string interactionName = (dialogue.interactions[dialogue.interactions[currentIndex].parentIndex].isPlayer)?"Player Said : ":"NPC Said : ";

                   
                    if (GUILayout.Button("Go Back"))
                    {
                        currentIndex = dialogue.interactions[currentIndex].parentIndex;
                        Repaint();
                        EditorUtility.SetDirty(this);
                        return;
                    }

                    EditorGUILayout.Space(10);
                    
                    EditorGUILayout.LabelField(interactionName + dialogue.interactions[dialogue.interactions[currentIndex].parentIndex].Line, style);
                    EditorGUILayout.Space(10);
                }

                EditorGUILayout.LabelField("Current Interaction : ",style);
                EditorGUILayout.Space(10);

                string replyName = (dialogue.interactions[currentIndex].isPlayer)?"Player Replies : ":"NPC Replies : ";
                Repaint();
                dialogue.interactions[currentIndex].Line =  EditorGUILayout.TextField(replyName, dialogue.interactions[currentIndex].Line);

                dialogue.interactions[currentIndex].sfx = (AudioClip) EditorGUILayout.ObjectField("SFX",dialogue.interactions[currentIndex].sfx, typeof(AudioClip), true);

                EditorGUILayout.BeginHorizontal();
                dialogue.interactions[currentIndex].waitTime = EditorGUILayout.FloatField("Run Time", dialogue.interactions[currentIndex].waitTime);

                if (GUILayout.Button("Match SFX Length"))
                {
                    if (dialogue.interactions[currentIndex].sfx != null)
                    {
                        dialogue.interactions[currentIndex].waitTime = dialogue.interactions[currentIndex].sfx.length;
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (dialogue.interactions[currentIndex].Commands != null) {
                    string commandsString = "";
                    for (int i = 0; i < dialogue.interactions[currentIndex].Commands.Count; i++)
                    {
                        string add = ",";
                        if (i == dialogue.interactions[currentIndex].Commands.Count-1 ) add = "";

                        commandsString += dialogue.interactions[currentIndex].Commands[i] + add;
                    }

                    if (commandsString != lastCommandsString) {
                        tempString = commandsString;
                        lastCommandsString = commandsString;
                        lastTempString = tempString;
                    }

                    EditorGUILayout.BeginHorizontal();
                    tempString = EditorGUILayout.TextField("Commands (Seperate with , )",tempString);

                    if (tempString != lastTempString) {
                        GUI.color = Color.red;
                    }
                    if (GUILayout.Button("Apply"))
                    {
                        dialogue.interactions[currentIndex].Commands = new List<string>();

                        string[] commands = tempString.Split(',');

                        for (int c = 0; c < commands.Length; c++)
                        {
                            commands[c] = commands[c].Trim();
                        }

                        dialogue.interactions[currentIndex].Commands.AddRange(commands);
                        lastTempString = tempString;
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();


                }

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Options for " + (!(dialogue.interactions[currentIndex].isPlayer)?"Player":"NPC"));
                
                EditorGUILayout.Space(10);

                    if (dialogue.interactions[currentIndex].childAIndex != -1) {
                     
                    if (GUILayout.Button(dialogue.interactions[dialogue.interactions[currentIndex].childAIndex ].Line))
                    {
                        currentIndex = dialogue.interactions[currentIndex].childAIndex;
                    }

                    EditorGUILayout.Space(5);
                    
                    if (dialogue.interactions[currentIndex].childBIndex != -1) {
                        if (GUILayout.Button(dialogue.interactions[dialogue.interactions[currentIndex].childBIndex ].Line))
                        {
                            currentIndex = dialogue.interactions[currentIndex].childBIndex;
                        }
                    }
                    else if (!dialogue.interactions[currentIndex].isPlayer) {
                        GUI.color = Color.green;
                        GUI.contentColor = Color.white;
                        if (GUILayout.Button("New Player Option"))
                        {
                            DialogueInteraction interaction = new DialogueInteraction(index: dialogue.interactions.Count, isPlayer: true, line: "Player Line", parentIndex: currentIndex);
                            dialogue.interactions[currentIndex].childBIndex = interaction.index;
                            dialogue.interactions.Add(interaction);
                        }
                        GUI.color = Color.white;
                        GUI.contentColor = Color.black;
                    }
                    }
                    else {
                    if (dialogue.interactions[currentIndex].isPlayer)
                    {
                        GUI.color = Color.green;
                        GUI.contentColor = Color.white;
                        if (GUILayout.Button("NPC Dialogue"))
                        {
                            DialogueInteraction interaction = new DialogueInteraction(index: dialogue.interactions.Count, isPlayer: false, line: "NPC Line", parentIndex: currentIndex);
                            dialogue.interactions[currentIndex].childAIndex = interaction.index;
                            dialogue.interactions.Add(interaction);
                        }
                        GUI.color = Color.white;
                        GUI.contentColor = Color.black;
                    }
                    else {
                        GUI.color = Color.green;
                        GUI.contentColor = Color.white;
                        if (GUILayout.Button("New Player Option"))
                        {
                            DialogueInteraction interaction = new DialogueInteraction(index: dialogue.interactions.Count, isPlayer: true, line: "Player Line", parentIndex: currentIndex);
                            dialogue.interactions[currentIndex].childAIndex = interaction.index;
                            dialogue.interactions.Add(interaction);
                        }

                        GUI.color = Color.white;
                        GUI.contentColor = Color.black;
                    }
                    }
                

                EditorGUILayout.Space(20);
                
                if (currentIndex > 0) {
                    GUI.color = Color.red;
                    if (GUILayout.Button("Delete Option and Children"))
                    {
                        
                            int newIndex = dialogue.interactions[currentIndex].parentIndex;
                            DeleteIndex(currentIndex);
                            currentIndex = newIndex;
                        }
                    }
                    GUI.color = Color.white;
                }

            


        }
    }

    private void DeleteIndex (int index)
    {
        for (int i = 0; i < dialogue.interactions.Count; i++)
        {
            if (dialogue.interactions[i].childAIndex == index) {
                dialogue.interactions[i].childAIndex = dialogue.interactions[i].childBIndex;
                dialogue.interactions[i].childBIndex = -1;
            }
            if (dialogue.interactions[i].childBIndex ==index)
                dialogue.interactions[i].childBIndex = -1;
            if (dialogue.interactions[i].parentIndex == index) {
                dialogue.interactions[i].parentIndex = -1;
                DeleteIndex(i);
            }
        }

        //Create ghost
        dialogue.interactions[index] = new DialogueInteraction();
        dialogue.interactions[index].isGhost  = true;

    }

    public void LoadSave (bool exists, string file)
    {
        EditorGUILayout.BeginHorizontal();
        if (exists)
        {
            if (GUILayout.Button("Load")) {

                dialogue.interactions.Clear();
                dialogue.interactions = DialogueUtils.Deserialize(file);

                if (dialogue.interactions.Count > 0)
                    currentIndex = 0;
                EditorUtility.SetDirty(dialogue);
            }
            
            if (GUILayout.Button("Save")) {
                
                //TODO: Update the text of existing file

                File.WriteAllText(Application.dataPath+"/Resources/Dialogue/"+dialogue.name+".txt", DialogueUtils.Serialize(dialogue.interactions));
                EditorUtility.SetDirty(dialogue);

            }

            if (dialogue.interactions.Count > 0)
            if (GUILayout.Button("Clear"))
            {
                dialogue.interactions.Clear();
                currentIndex = -1;
                EditorUtility.SetDirty(dialogue);
            }
        }
        else {
            
            if (dialogue.interactions.Count > 0) {
                if (GUILayout.Button("Create"))
                {
                        File.WriteAllText(Application.dataPath+"/Resources/Dialogue/"+dialogue.name+".txt", DialogueUtils.Serialize(dialogue.interactions));
                        EditorUtility.SetDirty(dialogue);
                    
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

}
