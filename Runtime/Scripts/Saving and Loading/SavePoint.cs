using ClassicFPS.Saving_and_Loading.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Saving_and_Loading
{
    /*
     * This script is for a point where a Player can approach and save the game; this can either be a Save Point or a Checkpoint
        * Save Point: Player has to go up to the Box and press E while near it to save the game (can be used multiple times)
        * Checkpoint: Player has to collide with it, then it saves; can be disabled afterwards
    */

    //Extends from the State class because it saves information on whether or not it is disabled
    public class SavePoint : State
    {
        [Header("Options")]
        //Whether or not this SavePoint is a checkpoint
        public bool isCheckpoint = false;

        [Header("Checkpoint Options [If Checkpoint]")]
        //Should the checkpoint be disabled after it has been used
        public bool disableCheckpointOnUse = true;

        //Whether or not the checkpoint is currently disabled
        private bool checkpointDisabled = false;

        [Header("Appearance")]
        //The material to switch to when it disables
        public Material disabledMaterial;

        [System.Serializable]
        public struct SavePointState
        {
            public bool checkpointDisabled;
        }

        [Header("Saved State")]
        public SavePointState savedState;

        //Logic to Save the state
        public override string SaveState()
        {
            savedState.checkpointDisabled = checkpointDisabled;
            return SaveUtils.ReturnJSONFromObject<SavePointState>(savedState);
        }
    
        //Logic to Load the state
        public override void LoadState(string loadedJSON)
        {
            savedState = SaveUtils.ReturnStateFromJSON<SavePointState>(loadedJSON);
            checkpointDisabled = savedState.checkpointDisabled;

            UpdateAppearance();
        }

        //Updates the appearance of the Checkpoint based on its enabled status
        private void UpdateAppearance()
        {
            if (checkpointDisabled)
            {
                this.transform.GetComponentInChildren<MeshRenderer>().material = disabledMaterial;
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            //Check if the Collider that entered the Trigger was a Player
            if (LayerMask.LayerToName(other.gameObject.layer) == "Player")
            {
                //If it is a Checkpoint then Save without prompt, if it isn't then request input
                if (!isCheckpoint)
                    SaveManager.PlayerStandingOnSavePoint();
                else
                {
                    if (!checkpointDisabled)
                    {
                        SaveManager.CheckpointReached();
                    }

                    //Checkpoint can't be used anymore
                    if (disableCheckpointOnUse) checkpointDisabled = true;
                    UpdateAppearance();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //Check if the Collider that entered the Trigger was a Player
            if (LayerMask.LayerToName(other.gameObject.layer) == "Player")
            {
                //In that case allow for saving functionality
                SaveManager.PlayerLeftSavePoint();
            }
        }
    }
}