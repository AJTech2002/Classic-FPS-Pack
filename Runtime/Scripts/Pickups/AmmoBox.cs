using ClassicFPS.Controller.Interaction;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Guns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Pickups
{
    //Ammo box is used to add more ammo to the gun that the Player is currently holding
    public class AmmoBox : Pickup
    {
        [Header("Collection Options")]
        [WeaponID]
        public string ID;
        [Space(10)]
        public bool isUniversalAmmo = false;

        [Header("Ammo Amount")]
        public int pickupAmount;

        private void OnTriggerEnter(Collider other)
        {
            if (!beenUsed && other.GetComponentInChildren<PlayerController>() != null)
            {
                PlayerWeaponController c = other.transform.GetComponent<PlayerWeaponController>();

                if (c != null)
                {

                    //Only pickup ammo if the Player has the weapon collected
                    if (c.collectedWeapons.Contains(ID) && !isUniversalAmmo)
                    {
                        beenUsed = true;

                        RunSFX();
                        EnableObject(false);

                        c.GetWeaponReference(ID).State.ammoRemaining += pickupAmount;


                        c.UpdateUI();

                    }

                    if (isUniversalAmmo)
                    {
                        if (c.GetActiveWeapon() != null && c.GetActiveWeapon().requiresAmmo)
                        {
                            beenUsed = true;
                            RunSFX();
                            EnableObject(false);

                            c.GetCurrentWeapon().State.ammoRemaining += pickupAmount;
                            c.UpdateUI();
                        }
                    }

                }
            }
        }

    }
}