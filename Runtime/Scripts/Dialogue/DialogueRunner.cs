using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace ClassicFPS.Dialogue
{
    public class DialogueRunner : MonoBehaviour
    {
        [Header("Display Options")]
        public bool autoRunWhenPlayerEnters = false;
        public bool disableAfterLeavingTrigger = false;
        public bool runOnce = false;
        public bool playPlayerReplies = false;

        private bool didRun = false;


        [Header("Callbacks")]

        public UnityEvent DialogueCompletedEvent;
        public UnityEvent<DialogueInteraction> DialogueRunEvent;

        [Header("References")]
        public int defaultRunDialogue = 0;
        public List<Dialogue> runnableDialogues = new List<Dialogue>();

        [Header("Input")]
        public bool overrideDialogueInputExecution;
        public InputAction dialogueRunInput;

        [HideInInspector]
        public bool inTrigger = false;

        [Header("Audio")]
        public AudioSource source;

        private void Awake()
        {
            if (!overrideDialogueInputExecution)
            {
                dialogueRunInput.Enable();
                dialogueRunInput.performed += RunDialogue;
            }
        }

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

            DialogueRunEvent.Invoke(dialogue);
        }

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

        private void OnTriggerExit(Collider col)
        {
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