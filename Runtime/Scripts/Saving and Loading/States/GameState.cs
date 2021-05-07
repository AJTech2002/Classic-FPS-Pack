using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassicFPS.Saving_and_Loading.States
{
    //This state simply stores what scene the Player is currently in
    public class GameState : State
    {

        //Creating a storage format for the transform component (must be marked Serializable)
        [System.Serializable]
        public struct LevelState
        {
            public string currentScene;
        }

        //The current Saved State
        [Header("Saved State")]
        public LevelState savedState = new LevelState();

        public override string SaveState()
        {
            savedState.currentScene = SceneManager.GetActiveScene().name;

            //Creating and returning JSON (SaveUtils provided)
            return SaveUtils.ReturnJSONFromObject<LevelState>(savedState);
        }

        public override void LoadState(string loadedJSON)
        {
            //Getting back the struct through JSON
            savedState = SaveUtils.ReturnStateFromJSON<LevelState>(loadedJSON);
            if (savedState.currentScene != "" && savedState.currentScene != SceneManager.GetActiveScene().name)
            {
                //When the game is loaded, you need to load the scene the player was in before
                Debug.Log("Got Saved Scene Info : " + savedState.currentScene);
                GameManager.LoadScene(savedState.currentScene, true);
            }
        }


        public override string GetUID()
        {
            //Always be the same UID no matter the Scene
            return "GameManager";
        }

        public override bool ShouldSave()
        {
            //Remove from global save format
            return true;
        }

        public override bool ShouldLoad()
        {
            //Remove from global load format
            return false;
        }
    }
}