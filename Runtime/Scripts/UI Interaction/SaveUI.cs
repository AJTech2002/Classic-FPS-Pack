using ClassicFPS.Managers;
using ClassicFPS.Saving_and_Loading;
using ClassicFPS.Saving_and_Loading.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassicFPS.UI_Interaction
{
    public class SaveUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button startGameButton; //Button that will either start a new game or load from existing one
        public Button clearProgressButton; //Button that will clear the existing saves


        public bool HasSaveFile()
        {
            return SaveManager.HasSaveFiles();
        }

        public void DeleteSavedProgress()
        {
            SaveManager.ClearAllSaves();
            RefreshUI();
        }

        public void LoadPlayerCurrentLevel()
        {
            //Load the current GameState which is global (it stores the current level)
            SaveManager.LoadStateFromFile(GameObject.FindObjectOfType<GameState>());

        }

        public void LoadLevel ()
        {
            if (HasSaveFile())
            {
                //Load the current level if there is a Save
                LoadPlayerCurrentLevel();
            }
            else {
                //Load the Start Scene if there is not a Save
                GameManager.LoadStartScene();
            }
        }

        //Refreshes the UI 
        private void RefreshUI()
        {
            if (HasSaveFile())
            {
                //Text if there is a save file (Load Game)
                startGameButton.GetComponentInChildren<Text>().text = "Load Game";
                clearProgressButton.enabled = true;
            }
            else
            {
                //Text if there is no save files (New Game)
                startGameButton.GetComponentInChildren<Text>().text = "New Game";
                clearProgressButton.enabled = false;
            }
        }

        private void Start()
        {
            RefreshUI();
        }

    }
}