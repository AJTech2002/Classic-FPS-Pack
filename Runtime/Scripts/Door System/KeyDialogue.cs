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
    /* A good example of how to use the Dialogue System in a specialised way, running Dialogue under circumstances and with commands */
    public class KeyDialogue : State
    {
        [Header("Options")]
        public KeyReference keyReference;
        public DialogueRunner dialogue;
        public bool onlyGiftOnce;

        [System.Serializable]
        public struct EnabledState
        {
            public bool enabled;
        }

        [Header("Saved State")]
        public EnabledState savedState = new EnabledState();

        //Used State is tracked
        public override string SaveState()
        {
            return SaveUtils.ReturnJSONFromObject<EnabledState>(savedState);
        }

        //Used State is loaded
        public override void LoadState(string loadedJSON)
        {
            savedState = SaveUtils.ReturnStateFromJSON<EnabledState>(loadedJSON);
        }

        private void Start()
        {
            dialogue.dialogueRunInput.Enable();
            dialogue.dialogueRunInput.performed += RunDialogue;
        }

        //Run the 0th dialogue if the key isn't already gifted, 1st if it has been
        private void RunDialogue(InputAction.CallbackContext callbackContext)
        {
            if (savedState.enabled == false || !onlyGiftOnce)
                dialogue.ExecuteDialogue(0, true);
            else
                dialogue.ExecuteDialogue(1, true);
        }

        //Recieve a Dialogue Interaction from the Dialogue Runner
        public void GiftKey(DialogueInteraction interaction)
        {
            //If DialogueInteraction has a command that says "KEY" then recieve a key

            if (interaction.Commands.Contains("KEY"))
            {
                if (savedState.enabled == false || !onlyGiftOnce)
                {
                    savedState.enabled = true;
                    GameManager.PlayerStatistics.CollectKey(keyReference.keyReference);
                }
            }
        }
    }
}