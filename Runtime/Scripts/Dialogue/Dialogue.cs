using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace ClassicFPS.Dialogue
{
    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Classic FPS/Dialogue")]
    public class Dialogue : ScriptableObject
    {
        [Header("Properties")]
        public string interactionName;

        [HideInInspector]
        public string InputText;

        [Header("UI Options")]
        public string PlayerName = "Player";
        public string NPCName = "NPC";

        [HideInInspector]
        [SerializeField]
        public List<DialogueInteraction> interactions = new List<DialogueInteraction>();

        [HideInInspector]
        public int currentIndex = -1;

        [HideInInspector]
        public bool showProperties;

        [HideInInspector]
        public bool showSave;

        public bool ValidIndex(int index)
        {
            if (index < interactions.Count && index >= 0)
            {
                return true;
            }

            return false;
        }

        public DialogueInteraction At(int index)
        {
            return interactions[index];
        }

        public List<DialogueInteraction> Next(int currentIndex)
        {
            List<DialogueInteraction> temps = new List<DialogueInteraction>();

            if (ValidIndex(currentIndex))
            {
                if (ValidIndex(interactions[currentIndex].childAIndex))
                {
                    temps.Add(At(At(currentIndex).childAIndex));
                }

                if (ValidIndex(interactions[currentIndex].childBIndex))
                {
                    temps.Add(At(At(currentIndex).childBIndex));
                }
            }

            return temps;
        }

    }

    public class DialogueUtils
    {

        public static string SerializeIndex(List<DialogueInteraction> interactions, int index, int depth)
        {
            if (interactions[index].isGhost == true)
                return "";

            int spaces = 4 * depth;
            string spacesString = "";
            for (int i = 0; i < spaces; i++)
            {
                spacesString += " ";
            }

            string commandString = "";

            if (interactions[index].Commands.Count > 0)
            {
                commandString = "[";
                for (int i = 0; i < interactions[index].Commands.Count; i++)
                {
                    string add = ",";
                    if (i == interactions[index].Commands.Count - 1)
                        add = "";

                    commandString += interactions[index].Commands[i] + add;
                }
                commandString += "]";

                commandString.Replace('\n', ' ');
                commandString.Trim();
            }

            interactions[index].Line = interactions[index].Line.Trim();

            if (interactions[index].childAIndex == -1)
            {
                return spacesString + "- " + interactions[index].Line + " " + commandString;
            }
            else
            {

                if (interactions[index].childBIndex == -1)
                    return spacesString + "- " + interactions[index].Line + " " + commandString + '\n' + SerializeIndex(interactions, interactions[index].childAIndex, depth + 1);
                else
                    return spacesString + "- " + interactions[index].Line + " " + commandString + '\n' + SerializeIndex(interactions, interactions[index].childAIndex, depth + 1) + '\n' + SerializeIndex(interactions, interactions[index].childBIndex, depth + 1);

            }
        }

        public static string Serialize(List<DialogueInteraction> interactions)
        {

            if (interactions.Count > 0)
            {
                return SerializeIndex(interactions, 0, 0);
            }

            return "not implemented";
        }

        public static string DepthLevelKey(int depth, int level)
        {
            return "L" + (level.ToString()) + "D" + (depth.ToString());
        }

        public static List<DialogueInteraction> Deserialize(string s)
        {
            s = s.Replace('\t', ' ');
            string[] lines = s.Split('\n');

            int createdDialogue = 0;

            Dictionary<string, DialogueInteraction> indexedInteractions = new Dictionary<string, DialogueInteraction>();
            List<DialogueInteraction> tempInteractions = new List<DialogueInteraction>();

            int tabLength = 1;

            for (int d = 0; d < lines.Length; d++)
            {
                string line = lines[d];

                if (line.Length < 3) continue;

                int start = line.IndexOf('-');

                if (start != 0 && d == 1) tabLength = start;

                Debug.Log(tabLength);

                line = lines[d].Substring(start, lines[d].Length - start);

                DialogueInteraction interaction = new DialogueInteraction();

                interaction.index = createdDialogue;
                interaction.Line = line;
                interaction.level = d;
                interaction.depth = start / tabLength;
                interaction.isPlayer = (interaction.depth + 1) % 2 == 0;
                indexedInteractions["L" + (interaction.level.ToString()) + "D" + (interaction.depth.ToString())] = interaction;

                tempInteractions.Add(interaction);

                createdDialogue += 1;
            }

            for (int i = 0; i < tempInteractions.Count; i++)
            {
                tempInteractions[i].Line = tempInteractions[i].Line.Substring(2, tempInteractions[i].Line.Length - 2);

                if (tempInteractions[i].Line.Contains("[") && tempInteractions[i].Line.Contains("]"))
                {
                    int start = tempInteractions[i].Line.LastIndexOf("[");
                    int end = tempInteractions[i].Line.LastIndexOf("]");

                    string sub = tempInteractions[i].Line.Substring(start, end - start + 1);

                    sub = sub.Replace('[', ' ');
                    sub = sub.Replace(']', ' ');
                    sub = sub.Trim();

                    string[] commands = sub.Split(',');

                    tempInteractions[i].Commands = new List<string>();

                    tempInteractions[i].Commands.AddRange(commands);

                    tempInteractions[i].Line = tempInteractions[i].Line.Remove(start, end - start + 1);
                }

                int parentDepth = tempInteractions[i].depth - 1;
                int possibleLevel = tempInteractions[i].level - 1;

                tempInteractions[i].parentIndex = -1;

                for (int t = 0; t < tempInteractions.Count; t++)
                {
                    if (tempInteractions[t].depth == parentDepth && tempInteractions[t].level < tempInteractions[i].level)
                    {
                        tempInteractions[i].parentIndex = t;
                    }
                }

                int childLevel = tempInteractions[i].level + 1;
                int childDepth = tempInteractions[i].depth + 1;

                if (indexedInteractions.ContainsKey(DepthLevelKey(childDepth, childLevel)))
                {
                    tempInteractions[i].childAIndex = indexedInteractions[DepthLevelKey(childDepth, childLevel)].index;

                    tempInteractions[i].childBIndex = -1;
                    for (int t = 0; t < tempInteractions.Count; t++)
                    {
                        if (tempInteractions[t].index != tempInteractions[i].index && tempInteractions[t].level > tempInteractions[i].level && tempInteractions[t].depth <= tempInteractions[i].depth)
                            break;

                        if (tempInteractions[t].depth == childDepth && t != tempInteractions[i].childAIndex && tempInteractions[t].level > tempInteractions[i].level)
                        {
                            tempInteractions[i].childBIndex = t;
                        }
                    }
                }

            }

            return tempInteractions;
        }

    }

    [System.Serializable]
    public class DialogueInteraction
    {

        [SerializeField]
        public bool isGhost = false;
        public bool isPlayer;
        public float waitTime = 1;
        public string Line;
        public List<string> Commands = new List<string>();
        public AudioClip sfx;

        public int depth = 0;
        public int level = 0;

        public int index;
        public int childAIndex = -1;
        public int childBIndex = -1;
        public int parentIndex = -1;

        public DialogueInteraction(int index = 0, bool isPlayer = false, string line = "", int parentIndex = -1)
        {
            this.index = index;
            this.isPlayer = isPlayer;
            this.Line = line;
            this.parentIndex = parentIndex;
        }

    }
}