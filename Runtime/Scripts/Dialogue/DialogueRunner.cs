using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace ClassicFPS.Dialogue
{
    //The script that will be placed on a Monobehaviour to begin the dialogue
    public class DialogueRunner : MonoBehaviour
    {
        [Header("Display Options")]
        public bool autoRunWhenPlayerEnters = false; //Play the dialogue when the Player enters a trigger in the GameObject
        public bool disableAfterLeavingTrigger = false; //Disable the Dialogue when the Player leaves the trigger
        public bool runOnce = false; //Only allow it to be run once
        public bool playPlayerReplies = false; //Whether or not to play the Player's replies

        private bool didRun = false;


        [Header("Callbacks")]

        public UnityEvent DialogueCompletedEvent; //You can call an event when the Dialogue is completed
        public UnityEvent<DialogueInteraction> DialogueRunEvent; //Everytime a Dialogue is run it can call a function

        [Header("References")]
        public int defaultRunDialogue = 0; // You can run multiple dialogues from the same script, you can change it based on the stage of the game for example
        public List<Dialogue> runnableDialogues = new List<Dialogue>(); //List of the dialogues that can be played

        [Header("Input")]
        public bool overrideDialogueInputExecution; //Don't just run the dialogue when the input is pressed
        public InputAction dialogueRunInput; //The input needed to run the dialogue

        [HideInInspector]
        public bool inTrigger = false;

        [Header("Audio")]
        public AudioSource source; //The AudioSource to play the Dialogue from

        private void Awake()
        {
            if (!overrideDialogueInputExecution)
            {
                dialogueRunInput.Enable();
                dialogueRunInput.performed += RunDialogue;
            }
        }

        //Executes the Dialogue only if certain parameters are met
        public void ExecuteDialogue(int index, bool requirePlayerToBeInTrigger = false)
        {
            if ((inTrigger && requirePlayerToBeInTrigger) || requirePlayerToBeInTrigger == false)
            {
                defaultRunDialogue = index;
                StartDialogue();
            }
        }

        //Run Dialogue through Input
        private void RunDialogue(InputAction.CallbackContext callbackContext)
        {
            if (inTrigger)
                StartDialogue();
        }

        private void DialogueCompleted()
        {
            if (DialogueCompletedEvent != null)
                DialogueCompletedEvent.Invoke();
        }

        private void DialogueStarted(DialogueInteraction dialogue)
        {
            if (dialogue.sfx != null)
            {
                print(dialogue.Line + " has SFX");
                source.clip = dialogue.sfx;
                source.Play();
            }

            if (DialogueRunEvent != null)
                DialogueRunEvent.Invoke(dialogue);
        }

        //Begins the dialogue by interacting with the DialogueUI script
        private void StartDialogue()
        {
            if (DialogueUI.instance != null)
            {
                if ((runOnce && !didRun) || !runOnce)
                {
                    Dialogue dialogue = runnableDialogues[defaultRunDialogue];
                    DialogueUI.SetDialogueInteractions(dialogue, dialogue.NPCName, dialogue.PlayerName, 0, playPlayerReplies, DialogueStarted, DialogueCompleted);
                    didRun = true;
                }
            }
        }

        //Begin dialogue when the Player enters only if autoRunWhenplayerEnters is true
        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Player"))
            {
                inTrigger = true;

                if (autoRunWhenPlayerEnters)
                {
                    StartDialogue();
                }
            }
        }

        //Requires a trigger to begin the work 
        private void OnTriggerExit(Collider col)
        {
            //Only if the Player enters
            if (col.CompareTag("Player"))
            {
                inTrigger = false;

                if (disableAfterLeavingTrigger)
                {
                    DialogueUI.Disable();
                }
            }
        }

    }
}