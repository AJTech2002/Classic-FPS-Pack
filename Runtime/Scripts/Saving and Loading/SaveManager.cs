using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Threading.Tasks;
using ClassicFPS.Controller.PlayerState;
using ClassicFPS.Saving_and_Loading.States;

namespace ClassicFPS.Saving_and_Loading
{
    public class SaveManager : MonoBehaviour
    {

        [HideInInspector]
        public static SaveManager instance;
        [HideInInspector]
        public bool playerIsAtSavePoint; //Whether or not the Player is currently standing on a Save Point

        [Header("Save Input Options [Default E]")]
        public InputAction saveInputAction;
        public InputAction loadAction;

        private void Awake()
        {
            instance = this;

            saveInputAction.Enable();
            saveInputAction.performed += SaveInputAction_performed;

            //TODO: Remove this, this is for testing only
            loadAction.Enable();
            loadAction.performed += LoadAction_performed;
        }


        public static void LoadLevelContents()
        {
            instance.StartCoroutine("Delayed_Load");
        }

        public IEnumerator Delayed_Load()
        {
            //Small delay to let all the items in the scene to begin 
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Debug.Log("Scene Loading Initialization");

            LoadAllStates();
        }

        private void LoadAction_performed(InputAction.CallbackContext obj)
        {
            print("Has Loaded!");
            LoadAllStates();
        }

        private void SaveInputAction_performed(InputAction.CallbackContext obj)
        {
            //Only allow for a save if the Player is at the Save point
            if (playerIsAtSavePoint)
            {
                print("Has Saved!");
                instance.StartCoroutine("SaveAllStates");
            }
        }

        public static void SavePlayerStatistics()
        {
            if (GameObject.FindObjectOfType<PlayerStatistics>() != null)
            {
                SaveStateToFile(GameObject.FindObjectOfType<PlayerStatistics>());
            }
        }

        //Saves all states
        public static void SaveAll()
        {
            instance.StartCoroutine("SaveAllStates");
        }

        public static void SaveStateToFile(State state)
        {
            if (state.ShouldSave())
            {
                string json = state.SaveState();
                Task writeFile = instance.WriteFileAsync(Application.persistentDataPath + "/" + state.GetUID() + ".save", json);
            }
        }

        public static void LoadAll()
        {
            instance.LoadAllStates();
        }

        public static bool HasSaveFiles()
        {
            if (GameObject.FindObjectOfType<GameState>() == null)
            {
                Debug.LogError("A GameState Component is required in the scene");
                return false;
            }

            //Check if a GameState save file exists (which is the global save file)
            string fileLocation = Application.persistentDataPath + "/" + GameObject.FindObjectOfType<GameState>().GetUID() + ".save";

            //If it exists then there is saved progress
            if (File.Exists(fileLocation))
            {
                return true;
            }

            return false;
        }

        public static bool HasSaveFilesForScene(string sceneName)
        {
            //Find all files with extension .save in the Application.persistentDataPath path
            DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath);
            FileInfo[] Files = d.GetFiles(sceneName + "_*.save");

            return (Files != null) ? (Files.Length >= 1) : false;
        }

        public static void ClearAllSaves()
        {
            //Find all files with extension .save in the Application.persistentDataPath path
            DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath);
            FileInfo[] Files = d.GetFiles("*.save");

            //For each file, delete it.
            foreach (FileInfo file in Files)
            {

                File.Delete(file.FullName);
            }
        }

        public static void LoadStateFromFile(State state)
        {
            //Find the location of the file (using UID)
            string fileLocation = Application.persistentDataPath + "/" + state.GetUID() + ".save";

            //Make sure the Save file still exists
            if (File.Exists(fileLocation))
            {
                //If it does then read the JSON
                string json = File.ReadAllText(fileLocation);
                state.LoadState(json);
            }
        }

        //Clears all the saves from the current level
        public static void ClearLevelSaves(string sceneName)
        {
            //Find all files with extension .save in the Application.persistentDataPath path
            DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath);
            FileInfo[] Files = d.GetFiles(sceneName + "_*.save");

            //For each file, delete it.
            foreach (FileInfo file in Files)
            {
                Debug.Log(file.FullName);
                File.Delete(file.FullName);
            }
        }

        //Method that Saves all the States in the Scene
        public IEnumerator SaveAllStates()
        {
            //Find all the State objects in the Scene
            foreach (State foundState in GameObject.FindObjectsOfType<State>())
            {
                //If the State says it should save (true by default)
                if (foundState.ShouldSave())
                {
                    yield return new WaitForEndOfFrame();
                    //Get the JSON for that Object from the State
                    string json = foundState.SaveState();

                    WriteFileAsync(Application.persistentDataPath + "/" + foundState.GetUID() + ".save", json);

                    //Do it across multiple frames so going across checkpoints is smooth
                    yield return new WaitForEndOfFrame();

                }
            }
        }

        //Should Save Async so it doesn't affect the Gameplay while the Character is moving
        public async Task WriteFileAsync(string path, string json)
        {
            using (StreamWriter outputFile = new StreamWriter(path))
            {
                await outputFile.WriteAsync(json);
            }
        }


        //Can be called whenever the Game needs to be reloaded
        public void LoadAllStates()
        {
            //Find each state in the Scene
            foreach (State foundState in GameObject.FindObjectsOfType<State>())
            {
                if (foundState.ShouldLoad())
                {
                    //Find the location of the file (using UID)
                    string fileLocation = Application.persistentDataPath + "/" + foundState.GetUID() + ".save";

                    //Make sure the Save file still exists
                    if (File.Exists(fileLocation))
                    {
                        //If it does then read the JSON
                        string json = File.ReadAllText(fileLocation);
                        foundState.LoadState(json);
                    }
                }
            }
        }

        //Static functions for the SavePoints to tell the manager that the Player is in/out of the Save Point
        public static void PlayerStandingOnSavePoint()
        {
            instance.playerIsAtSavePoint = true;
        }

        public static void PlayerLeftSavePoint()
        {
            instance.playerIsAtSavePoint = false;
        }

        public static void CheckpointReached()
        {
            //Auto start saving states
            print("Checkpoint Reached");
            instance.StartCoroutine("SaveAllStates");
        }

    }
}