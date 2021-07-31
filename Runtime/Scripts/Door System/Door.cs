using ClassicFPS.Audio;
using ClassicFPS.Dialogue;
using ClassicFPS.Managers;
using ClassicFPS.Saving_and_Loading;
using ClassicFPS.Saving_and_Loading.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClassicFPS.Door_System
{
    public class Door : State
    {   
        //What key can open this door
        [Header("Door Options")]
        public KeyReference keyReference;
        public bool locked = true;

        //The sound effect options for the door
        [Header("SFX")]
        public Sound doorOpen;
        [Range(0f, 10f)]
        public float volume = 0.5f;

        [Header("Animation")]
        public Animation doorAnimation; 
        public string doorAnimationName = "Door_Open"; //Run an animation to open the door

        [Header("Dialogue")]
        public DialogueRunner dialogueRunner; //You can attach a dialogue to this, when the door is opened

        [Header("Input")]
        public InputAction openDoor; // The key to open doors when you are in the trigger

        private bool inTrigger = false;

        private bool hasBeenOpened = false;

        [System.Serializable]
        public struct EnabledState
        {
            public bool enabled;
        }

        private EnabledState savedState = new EnabledState();

        public override string SaveState()
        {
            savedState.enabled = !hasBeenOpened;

            return SaveUtils.ReturnJSONFromObject<EnabledState>(savedState);
        }

        //Used State is loaded
        public override void LoadState(string loadedJSON)
        {
            savedState = SaveUtils.ReturnStateFromJSON<EnabledState>(loadedJSON);

            hasBeenOpened = !savedState.enabled;

            if (hasBeenOpened)
            {
                doorAnimation.Play(doorAnimationName);
                hasBeenOpened = true;
            }

        }


        private void Start()
        {
            openDoor.Enable();
            openDoor.performed += OnOpenDoorAttempt;
        }

        //Make sure the door is opened only when you have a key that fits the door
        private void OnOpenDoorAttempt(InputAction.CallbackContext callback)
        {
            if (inTrigger)
            {
                if (GameManager.PlayerStatistics.HasKey(keyReference.keyReference) || !locked)
                {
                    if (GameManager.keySettings[keyReference.keyReference].consumable && locked)
                    {
                        int index = GameManager.PlayerStatistics.keys.IndexOf(keyReference.keyReference);
                        GameManager.PlayerStatistics.keys.RemoveAt(index);
                        GameManager.PlayerStatistics.keyUI.UpdateUI(GameManager.PlayerStatistics.keys);
                    }

                    Open();
                }
                else if (!hasBeenOpened)
                {
                    if (dialogueRunner != null)
                        dialogueRunner.ExecuteDialogue(0);
                }
            }
        }

        //Open the Door
        private void Open()
        {
            if (!hasBeenOpened)
            {
                doorOpen.PlayAt(transform.position, volume);
                doorAnimation.Play(doorAnimationName);
                hasBeenOpened = true;
            }
        }

        //Use the trigger in the object to detect when a Player enters  
        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Player"))
            {
                inTrigger = true;
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.CompareTag("Player"))
            {
                inTrigger = false;
            }
        }

    }
}