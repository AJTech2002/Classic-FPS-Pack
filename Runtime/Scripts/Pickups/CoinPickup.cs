using ClassicFPS.Controller.PlayerState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Pickups
{
    public class CoinPickup : Pickup
    {
        [Header("Pickup Options")]
        public int pickupAmount = 1;

        private void OnTriggerEnter(Collider other)
        {
            PlayerStatistics stats = other.GetComponentInChildren<PlayerStatistics>();

            if (!beenUsed && stats != null)
            {
                RunSFX();
                WeaponPickupAnimation();
                stats.playerOptions.coins += pickupAmount;
                stats.UpdateUI();
                beenUsed = true;
                EnableObject(false);
            }
        }
    }

}