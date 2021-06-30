using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

using ClassicFPS.Guns;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Pickups;

namespace ClassicFPS.Controller.Interaction
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [Header("Weapon Manager")]
        public WeaponManager weaponManager;
        [HideInInspector]
        public PlayerController controller;
        private PlayerObjectInteractionHandler objectInteractionHandler;

        [Header("Audio")]
        public AudioSource weaponSoundSource;

        [Header("Default Weapons")]
        public List<string> defaultWeapons = new List<string>();

        [HideInInspector]
        public int currentlyHoldingGunIndex = -1;

        [HideInInspector]
        public List<string> collectedWeapons = new List<string>();

        [Header("References")]
        public Transform weaponMount; //Location where the gun should be mounted
        public Camera _camera;  //Reference to the Camera

        [Header("UI Elements")]
        public Image crosshairUI; //Reference to the CrosshairUI
        public Sprite defaultCrosshair; //Reference to the image of the default crosshair
        public Text ammoCount; //Reference to Ammo Counter
        public Image gunImage; //Image of the Gun

        [Header("Input Options")]
        public InputAction numberKeys;
        public InputAction scrollWheel;

        private void Start()
        {
            //Setup the WeaponManager 
            weaponManager.Setup();

            //Ensure the NumberKeys Inputs are setup
            numberKeys.Enable();

            //Map it to a Function
            numberKeys.performed += PerformedNumberPress;
            
            //Enable the ScrollWheel
            scrollWheel.Enable();

            //Map it to a function
            scrollWheel.performed += PerformedScrollWheel;

            //Make sure the user has all of the Default Weapons
            foreach (string s in defaultWeapons)
            {
                if (!collectedWeapons.Contains(s))
                {
                    CollectWeapon(s);
                }
            }

            UpdateUI();

            objectInteractionHandler = GetComponent<PlayerObjectInteractionHandler>();
        }

        private void OnDestroy()
        {
            numberKeys.Disable();
            scrollWheel.Disable();

        }

        //When the user presses a number acll this function
        private void PerformedNumberPress(InputAction.CallbackContext callback)
        {
            int val = (int)(callback.ReadValue<float>());
            int selected = val;

            //Find the Weapon in the WeaponManager linked to this keyBindIndex
            if (weaponManager.keybindIndex.ContainsKey(selected))
            {
                if (collectedWeapons.Contains(weaponManager.keybindIndex[selected]))
                {
                    //Call ChangeWeapon
                    ChangeWeapon(collectedWeapons.IndexOf(weaponManager.keybindIndex[selected]));
                }
            }
        }

        private float scrollAccumulator = 0f;
        private void PerformedScrollWheel(InputAction.CallbackContext callback)
        {
            float val = (callback.ReadValue<float>()) / 120;

            scrollAccumulator += val;


            // Scroll between the weapons when the threshold exceeds a certain value
            if (scrollAccumulator > 0.5f)
            {
                // Change the Weapon to the nextAvailableIndex
                ChangeWeapon(nextAvailableIndex(currentlyHoldingGunIndex));
                scrollAccumulator = 0;
            }
            else if (scrollAccumulator < -0.5f)
            {
                ChangeWeapon(previousAvailableIndex(currentlyHoldingGunIndex));
                scrollAccumulator = 0;
            }

        }

        // Find the next available index in the slots with a weapon
        int nextAvailableIndex(int from)
        {
            for (int i = from + 1; i < collectedWeapons.Count; i++)
            {
                if (collectedWeapons[i] != "")
                    return i;
            }

            return -1;
        }

        // Find the last index without a weapon 
        int previousAvailableIndex(int from)
        {
            for (int i = from - 1; i > -1; i--)
            {
                if (collectedWeapons[i] != "")
                    return i;
            }

            return -1;
        }

        //Switch the weapon over from the last weapon
        public void ChangeWeapon(int selected)
        {
            if (selected >= 0 && selected <= collectedWeapons.Count - 1 && selected != currentlyHoldingGunIndex)
            {
                if (objectInteractionHandler != null && objectInteractionHandler.hasObject())
                    return;

                //Unequp the Current Weapon
                UnequipWeapon();
                currentlyHoldingGunIndex = selected;

                //Equip the new weapon
                DirectEquip();
            }
        }

        //Update the Weapons UI (Ammo and Gun Image)
        public void UpdateUI()
        {
            if (ammoCount != null && GetCurrentWeapon() != null && GetActiveWeapon() != null && GetActiveWeapon().requiresAmmo)
            {
                gunImage.enabled = true;
                ammoCount.text = GetCurrentWeapon().State.ammoRemaining.ToString();
                gunImage.sprite = GetCurrentWeapon().thumbnail;
            }
            else if (ammoCount != null)
            {
                ammoCount.text = "-";
                gunImage.enabled = false;
            }
        }

        //Collect Weapon from ID
        public bool CollectWeapon(string ID)
        {
            if (collectedWeapons.Contains(ID))
            {
                return false;
            }

            //Unequip the Current Weapon
            UnequipWeapon();

            //Add to the list of collected weapons
            collectedWeapons.Add(ID);

            //Disable All Pickups
            foreach (WeaponPickup pickup in GameObject.FindObjectsOfType<WeaponPickup>())
            {
                if (pickup.GunID == ID)
                {
                    pickup.beenUsed = true;
                    pickup.EnableObject(false);
                }
            }

            collectedWeapons = collectedWeapons.OrderBy(o => weaponManager.GetWeaponReference(o).index).ToList<string>();

            currentlyHoldingGunIndex = collectedWeapons.IndexOf(ID);

            //Equip the current gun
            DirectEquip();
            
            //Reset the state of the Gun (Ammo) - The Default is set in the WeaponManager
            GetCurrentWeapon().State = GetCurrentWeapon().Default;

            //Make sure to update UI
            UpdateUI();

            return true;
        }
        
        //Get the ID of the current weapon
        public string CurrentHoldingID()
        {
            if (currentlyHoldingGunIndex >= 0 && currentlyHoldingGunIndex < collectedWeapons.Count)
            {
                return collectedWeapons[currentlyHoldingGunIndex];
            }
            else
            {
                return "";
            }
        }

        //The WeaponReference of the current weapon (WeaopnReference holds ammo and name)
        private WeaponReference CurrentWeapon;
        private int lastCurrentIndex;
        private int lastCount = 0;

        //Return the currently holding gun as a WeaponReference
        public WeaponReference GetCurrentWeapon()
        {
            if (currentlyHoldingGunIndex == lastCurrentIndex && CurrentWeapon != null && lastCount == collectedWeapons.Count) return CurrentWeapon;

            if (currentlyHoldingGunIndex != -1)
            {
                if (currentlyHoldingGunIndex <= collectedWeapons.Count - 1)
                {
                    CurrentWeapon = weaponManager.GetWeaponReference(collectedWeapons[currentlyHoldingGunIndex]);
                    lastCurrentIndex = currentlyHoldingGunIndex;
                    lastCount = collectedWeapons.Count;
                    return CurrentWeapon;
                }
            }

            return null;
        }

        /* A Few Helper Functions to Equip, Get Information, Unequip, Changing Crosshair */
        public WeaponReference GetWeaponReference(string UID)
        {
            return weaponManager.GetWeaponReference(UID);
        }

        public Weapon GetActiveWeapon()
        {
            return weaponManager.spawnedWeapon;
        }

        //Equipping and Unequipping Helpers
        public bool IsCurrentWeaponEquipped()
        {
            if (GetCurrentWeapon() == null)
                return false;

            return GetCurrentWeapon().isEquipped;
        }

        public void UnequipWeapon()
        {
            if (GetCurrentWeapon() != null)
                weaponManager.Unequip(CurrentHoldingID());

            UpdateUI();
        }

        public void DirectEquip()
        {
            weaponManager.Equip(CurrentHoldingID());
            UpdateUI();
        }

        public void EquipWeapon(float delay)
        {
            StartCoroutine(EquipWeaponDelayed(delay));
        }

        IEnumerator EquipWeaponDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (GetCurrentWeapon() != null)
            {

                weaponManager.Equip(CurrentHoldingID());
                UpdateUI();

            }
        }

        //Crosshair Helper Functions
        public void ChangeCrosshair(Sprite crosshair)
        {
            crosshairUI.sprite = crosshair;
        }

        public void RevertCrosshair()
        {
            crosshairUI.sprite = defaultCrosshair;
        }


    }
}