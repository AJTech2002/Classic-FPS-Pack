using ClassicFPS.Controller.Interaction;
using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassicFPS.Saving_and_Loading
{
public class LevelSwitcher : MonoBehaviour
{
    [Header("Options")]
    public string sceneToSwitchTo; //Name of scene to switch to
    public bool requiresLoading = true; //Do you need the Player on the next level (Game Over scene will not need the Player)
    public bool resetAll = false;

    private void OnTriggerEnter(Collider other)
    {
        //Check if the Collider that entered the Trigger was a Player
        if (LayerMask.LayerToName(other.gameObject.layer) == "Player")
        {
            //Unequip the Current Gun
            GameObject.FindObjectOfType<PlayerWeaponController>().UnequipWeapon();

            if (resetAll) SaveManager.ClearAllSaves();
            else {
                //Save Player Stats
                SaveManager.SavePlayerStatistics();

                //Clear all the saves of the current level, because Player is not going backwards in levels
                SaveManager.ClearLevelSaves(SceneManager.GetActiveScene().name);

                //Clear any previous saves of the level you are about to go to 
                SaveManager.ClearLevelSaves(sceneToSwitchTo);
            }
            

            //Load the Scene
            GameManager.LoadScene(sceneToSwitchTo, requiresLoading);
        }
    }
}
}