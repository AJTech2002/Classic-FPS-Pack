using ClassicFPS.Controller.PlayerState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Pickups
{
    public class HealthPickup : Pickup
    {
        [Header("Pickup Options")]
        public int healthPickupAmount = 1;

        private void OnTriggerEnter(Collider other)
        {
            PlayerStatistics stats = other.GetComponentInChildren<PlayerStatistics>();

            if (!beenUsed && stats != null && stats.playerOptions.health < stats.maxHealth)
            {
                RunSFX();
                WeaponPickupAnimation();
                stats.playerOptions.health = Mathf.Clamp(stats.playerOptions.health + healthPickupAmount, 0, stats.maxHealth);
                stats.UpdateUI();
                beenUsed = true;
                EnableObject(false);

            }
        }
    }
}