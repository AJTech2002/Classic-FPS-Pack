using ClassicFPS.Saving_and_Loading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.UI_Interaction
{
public class EndUI : MonoBehaviour
{
    private void Awake() {
        if (GameObject.FindObjectOfType<SaveManager>() != null)
        {
            Debug.Log("Reached End, Clearing all Saves!");
            SaveManager.ClearAllSaves();
        }
    }
}
}