using ClassicFPS.Controller.PlayerState;
using ClassicFPS.Managers;
using ClassicFPS.Pickups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Door_System
{
    public class KeyPickup : Pickup
    {
        [Header("Key Options")]
        public KeyReference keyReference;


        private void OnTriggerEnter(Collider other)
        {
            PlayerStatistics stats = other.GetComponentInChildren<PlayerStatistics>();

            if (!beenUsed && stats != null && (!stats.keys.Contains(keyReference.keyReference) || GameManager.keySettings[keyReference.keyReference].allowMultiplePickups))
            {
                RunSFX();
                stats.CollectKey(keyReference.keyReference);
                beenUsed = true;
                EnableObject(false);

                //GameManager.CollectedKey(keyReference.keyReference);
            }
        }

    }
}