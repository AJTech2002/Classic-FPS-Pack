using ClassicFPS.Controller.Interaction;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Controller.PlayerState;
using ClassicFPS.Door_System;
using ClassicFPS.Saving_and_Loading;
using ClassicFPS.Saving_and_Loading.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassicFPS.Managers
{
    [RequireComponent(typeof(SaveManager))]
    public class GameManager : MonoBehaviour
    {
        [Header("Prefab")]
        //The Prefab of the Player
        public GameObject playerPrefab;

        [Header("Settings")]
        public KeyManager keyManager;

        [Header("Scene Information")]
        public bool isTestingScene = false;
        //The name of the starting scene (that is not the Menu)
        public string startingSceneName;

        public static GameManager instance;

        public static PlayerWeaponController WeaponController;
        public static PlayerController PlayerController;
        public static PlayerStatistics PlayerStatistics;

        //Whether or not the game is loading the next scene [Important]
        public bool awaitingSceneLoad = false;

        //Requires loading
        private bool requiresLoading = true;

        [HideInInspector]
        public static Dictionary<string, KeySettings> keySettings = new Dictionary<string, KeySettings>();

        //Setup References
        private void Awake()
        {
            //Setup the Keys
            if (keyManager != null)
            {
                keySettings.Clear();
                for (int i = 0; i < keyManager.keys.Count; i++)
                {
                    keySettings.Add(keyManager.keys[i].keyID, keyManager.keys[i]);
                }
            }

            if (startingSceneName == "") Debug.LogError("No Starting Scene Attached");
            if (playerPrefab == null) Debug.LogError("No Player Prefab Atached");

            //Destroy all duplicates of GameManager
            GameObject[] objs = GameObject.FindGameObjectsWithTag("GameManager");

            foreach (GameObject go in objs)
            {
                if (go != this.gameObject)
                    Destroy(go);
            }

            DontDestroyOnLoad(this.gameObject);
            instance = this;

            //When the scene is loaded call SceneManager_sceneLoaded
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            //Setup some References to be accessed from anywhere
            WeaponController = GameObject.FindObjectOfType<PlayerWeaponController>();
            PlayerController = GameObject.FindObjectOfType<PlayerController>();
            PlayerStatistics = GameObject.FindObjectOfType<PlayerStatistics>();

            if (isTestingScene)
            {
                startingSceneName = SceneManager.GetActiveScene().name;
                awaitingSceneLoad = true;
            }
        }

        public static void RespawnFromLastSave()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            instance.awaitingSceneLoad = true;
            instance.requiresLoading = true;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            //If the GameManager was expecting a new scene to be loaded, then detect any saves
            if (awaitingSceneLoad && requiresLoading)
            {
                //Only run this code if manually called a new Scene

                Debug.Log("Scene Loaded, Created Save Point at this Scene.");

                //Find a SpawnPoint
                GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

                if (spawnPoint == null) Debug.LogError("No SpawnPoint found in this scene");

                //Create a new Player if it doesn't exist in that scene
                if (GameObject.FindGameObjectWithTag("Player") == null)
                {
                    Debug.Log("Creating new Player");

                    GameObject go = GameObject.Instantiate(instance.playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

                    //Set the rotation of the Camera to face in direction of Spawn Point
                    go.GetComponentInChildren<PlayerCameraController>().SetRotation(spawnPoint.transform.eulerAngles);
                }


                //Setup References
                WeaponController = GameObject.FindObjectOfType<PlayerWeaponController>();
                PlayerController = GameObject.FindObjectOfType<PlayerController>();
                PlayerStatistics = GameObject.FindObjectOfType<PlayerStatistics>();

                //Needs to be Coroutine to give the Level 1 frames to load into memory
                //GameObject.FindObjectOfType<SaveManager>().StartCoroutine("LoadLevelContents");

                SaveManager.LoadLevelContents();

                //Check if there are any existing save files for the loaded scene
                if (!SaveManager.HasSaveFilesForScene(scene.name))
                    //If there isn't then create a Save here, so that the Game knows the Player reached this level
                    SaveManager.SaveAll();

                awaitingSceneLoad = false;
            }
        }

        //Check if there are any saves
        public bool HasSaveFile()
        {
            return SaveManager.HasSaveFiles();
        }

        //Delete any progress
        public void DeleteSavedProgress()
        {
            SaveManager.ClearAllSaves();
        }

        public void LoadPlayerCurrentLevel()
        {
            //Load the current GameState which is global (it stores the current level)
            SaveManager.LoadStateFromFile(this.GetComponent<GameState>());

            //Once the level is loaded then load in all the Player/Environment information
            SaveManager.LoadAll();
        }

        public static void LoadStartScene()
        {
            LoadScene(instance.startingSceneName, true);
        }

        public static void CollectedKey(string keyID)
        {
            //Nothing
        }

        public static void LoadScene(string sceneName, bool requiresLoading)
        {
            SceneManager.LoadScene(sceneName);
            instance.awaitingSceneLoad = true;
            instance.requiresLoading = requiresLoading;
        }

    }
}