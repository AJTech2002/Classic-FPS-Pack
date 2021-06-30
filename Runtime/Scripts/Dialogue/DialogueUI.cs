using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace ClassicFPS.Dialogue
{
    //Dialogue UI will handle the actual display of the options and text
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI instance;

        [Header("UI")]
        public Text speakerText; //The text indiating who is speaking 
        public Text text; //The text of the dialogue
        public Button optionA; //The button of the first option
        public Text optionAText; //The text within the option
        public Button optionB; //The button of the second option
        public Text optionBText; //The text within the option

        [HideInInspector]
        public int currentIndex = 0;

        [Header("Input")]
        public InputAction OptionAInput; //You can assign a button to automatically press option A
        public InputAction OptionBInput; //You can assign a button to automatically press option B

        [Header("Current Dialogue")]
        public string NPCName;
        public string PlayerName;
        public Dialogue dialogue;
        public float waitTime;
        public System.Action<DialogueInteraction> OnDialogueStarted;
        public System.Action OnDialogueCompleted;
        public bool playPlayerReplies;



        private void Awake()
        {
            if (instance == null)
            {
                DialogueUI.instance = this;
                OptionAInput.Enable();
                OptionBInput.Enable();
                
                OptionAInput.performed += OptionASelected;
                OptionBInput.performed += OptionBSelected;
            }
        }

        private void OptionASelected(InputAction.CallbackContext obj)
        {
            if (instance.dialogue != null)
                if (!instance.dialogue.interactions[currentIndex].isPlayer && instance.dialogue.interactions[currentIndex].childAIndex != -1)
                    ChildASelected();
        }


        private void OptionBSelected(InputAction.CallbackContext obj)
        {
            if (instance.dialogue != null)
                if (!instance.dialogue.interactions[currentIndex].isPlayer && instance.dialogue.interactions[currentIndex].childBIndex != -1)
                    ChildBSelected();
        }

        //This is called by the UI Button
        public void ChildASelected()
        {
            if (dialogue != null)
            {
                //Move the dialogue forward
                if (playPlayerReplies)
                    MoveToIndex(dialogue.interactions[currentIndex].childAIndex);
                else
                    MoveToIndex(dialogue.interactions[dialogue.interactions[currentIndex].childAIndex].childAIndex);
            }
        }

        //This is called by the UI Button
        public void ChildBSelected()
        {
            if (dialogue != null)
            {
                //Move the Dialogue Forward
                if (playPlayerReplies)
                    MoveToIndex(dialogue.interactions[currentIndex].childBIndex);
                else
                    MoveToIndex(dialogue.interactions[dialogue.interactions[currentIndex].childBIndex].childAIndex);
            }
        }

        //Move to Index moves the dialogue to a point within the DialogueInteractions
        private void MoveToIndex(int index)
        {
            if (dialogue != null && dialogue.ValidIndex(index))
            {
                currentIndex = index;
                ApplyIndex();
            }
            else
            {
                //Finish the Dialogue if nothing is left
                instance.optionB.gameObject.SetActive(false);
                instance.optionA.gameObject.SetActive(false);
                instance.Clear();

                instance.OnDialogueCompleted();
            }
        }

        //ApplyIndex decides how the UI should be rendered and handled for the next iteration
        private void ApplyIndex()
        {
            if (dialogue.ValidIndex(instance.currentIndex))
            {
                instance.speakerText.gameObject.SetActive(true);
                instance.text.gameObject.SetActive(true);

                instance.speakerText.text = dialogue.At(instance.currentIndex).isPlayer ? instance.PlayerName : NPCName;
                instance.text.text = dialogue.At(instance.currentIndex).Line;

                instance.waitTime = dialogue.At(instance.currentIndex).waitTime;

                instance.OnDialogueStarted(dialogue.At(instance.currentIndex));

                instance.optionA.gameObject.SetActive(false);
                instance.optionB.gameObject.SetActive(false);

                if (dialogue.At(instance.currentIndex).isPlayer)
                {
                    instance.optionB.gameObject.SetActive(false);
                    instance.optionA.gameObject.SetActive(false);
                    instance.StartCoroutine(instance.WaitForNext(dialogue.At(instance.currentIndex).childAIndex, waitTime));
                    return;
                }


                if (dialogue.At(instance.currentIndex).childAIndex != -1 && dialogue.At(instance.currentIndex).childBIndex != -1 && !dialogue.At(instance.currentIndex).isPlayer)
                {
                    instance.optionA.gameObject.SetActive(true);
                    instance.optionAText.text = dialogue.At(dialogue.At(instance.currentIndex).childAIndex).Line + " [ X ] ";

                    instance.optionB.gameObject.SetActive(true);
                    instance.optionBText.text = dialogue.At(dialogue.At(instance.currentIndex).childBIndex).Line + " [ Z ] ";
                }
                else if (dialogue.At(instance.currentIndex).childAIndex != -1 && !dialogue.At(instance.currentIndex).isPlayer)
                {
                    instance.optionA.gameObject.SetActive(false);
                    instance.optionB.gameObject.SetActive(false);
                    instance.StartCoroutine(instance.WaitForNext(dialogue.At(instance.currentIndex).childAIndex, waitTime));
                }
                else if (dialogue.At(instance.currentIndex).childAIndex == -1 && !dialogue.At(instance.currentIndex).isPlayer)
                {
                    instance.optionA.gameObject.SetActive(false);
                    instance.optionB.gameObject.SetActive(false);
                    instance.StartCoroutine(DisableIn(waitTime));
                    instance.OnDialogueCompleted();
                }
            }
        }

        //Few Helper functions to Clear, Disable, Wait etc. with time constraints
        private IEnumerator DisableIn(float wait)
        {
            yield return new WaitForSeconds(wait);
            instance.optionA.gameObject.SetActive(false);
            instance.optionA.gameObject.SetActive(false);
            instance.text.gameObject.SetActive(false);
            instance.speakerText.gameObject.SetActive(false);
        }

        private IEnumerator WaitForNext(int next, float wait)
        {
            yield return new WaitForSeconds(wait);
            MoveToIndex(next);
        }

        public void Clear()
        {
            instance.speakerText.gameObject.SetActive(false);
            instance.text.gameObject.SetActive(false);
            instance.optionB.gameObject.SetActive(false);
            instance.optionA.gameObject.SetActive(false);
        }

        public static void Disable()
        {
            instance.dialogue = null;
            instance.currentIndex = -1;
            instance.NPCName = "";
            instance.PlayerName = "";
            instance.waitTime = 0;

            instance.Clear();
        }

        //It starts here, the dialogue objet is set with a few of options
        public static void SetDialogueInteractions(Dialogue dialogue, string NPCName, string PlayerName, float waitTime, bool playPlayerReplies, System.Action<DialogueInteraction> OnDialogueStarted, System.Action OnInteractionCompleted)
        {
            if (dialogue.ValidIndex(0))
            {

                instance.currentIndex = 0;
                instance.dialogue = dialogue;
                instance.playPlayerReplies = playPlayerReplies;
                instance.NPCName = NPCName;
                instance.PlayerName = PlayerName;
                instance.waitTime = dialogue.At(instance.currentIndex).waitTime;

                instance.OnDialogueStarted = OnDialogueStarted;
                instance.OnDialogueCompleted = OnInteractionCompleted;

                //You can call an action when the dialouge is started
                OnDialogueStarted(dialogue.At(instance.currentIndex));

                instance.speakerText.gameObject.SetActive(true);
                instance.text.gameObject.SetActive(true);

                bool isPlayer = dialogue.At(instance.currentIndex).isPlayer;

                instance.speakerText.text = isPlayer ? instance.PlayerName : NPCName;
                instance.text.text = dialogue.At(instance.currentIndex).Line;

                instance.optionB.gameObject.SetActive(false);
                instance.optionB.gameObject.SetActive(false);

                if (dialogue.At(instance.currentIndex).childAIndex != -1 && dialogue.At(instance.currentIndex).childBIndex != -1 && !isPlayer)
                {
                    //Providing options for selection
                    instance.optionA.gameObject.SetActive(true);
                    instance.optionAText.text = dialogue.At(dialogue.At(instance.currentIndex).childAIndex).Line + " [ X ] ";

                    instance.optionB.gameObject.SetActive(true);
                    instance.optionBText.text = dialogue.At(dialogue.At(instance.currentIndex).childBIndex).Line + " [ Z ] ";
                }
                else if (dialogue.At(instance.currentIndex).childAIndex != -1 && !isPlayer)
                {
                    instance.StartCoroutine(instance.WaitForNext(dialogue.At(instance.currentIndex).childAIndex, instance.waitTime));
                    instance.optionA.gameObject.SetActive(false);
                    instance.optionB.gameObject.SetActive(false);
                }
                else if (!isPlayer)
                {
                    instance.StartCoroutine(instance.DisableIn(instance.waitTime));

                    //You can call an action when the Dialogue is completed
                    instance.OnDialogueCompleted();

                }

            }
        }

    }
}