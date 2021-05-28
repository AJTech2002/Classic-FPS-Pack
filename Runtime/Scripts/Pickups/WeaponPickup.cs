using ClassicFPS.Controller.Interaction;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Guns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Pickups
{
    //Ammo box is used to add more ammo to the gun that the Player is currently holding
    public class WeaponPickup : Pickup
    {
    
        [WeaponID]
        public string GunID;
   

        private void OnTriggerEnter(Collider other)
        {
            if (!beenUsed && other.GetComponentInChildren<PlayerController>() != null)
            {
                PlayerWeaponController c = other.transform.GetComponent<PlayerWeaponController>();

                //Only pickup the Ammo if the Player has a weapon and it requires ammo
                if (c.CollectWeapon(GunID))
                {
                    beenUsed = true;
                    RunSFX();
                    WeaponPickupAnimation();
                    EnableObject(false);
                }
            }
        }

    }
}