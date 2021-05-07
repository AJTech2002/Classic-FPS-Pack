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

        //[HideInInspector]
        //What is the player currently holding
        public int currentlyHoldingGunIndex = -1;

        //[HideInInspector]
        //Names of the collected weapons from the 'List of All Weapons'
        public List<string> collectedWeapons = new List<string>();

        [Header("References")]
        //Location where the gun should be mounted
        public Transform weaponMount;
        //Reference to the Camera
        public Camera _camera;

        [Header("UI Elements")]
        //Reference to the CrosshairUI
        public Image crosshairUI;

        //Reference to the image of the default crosshair
        public Sprite defaultCrosshair;

        //Reference to Ammo Counter
        public Text ammoCount;

        //Image of the Gun
        public Image gunImage;

        [Header("Input Options")]
        public InputAction numberKeys;
        public InputAction scrollWheel;

        private void Start()
        {
            weaponManager.Setup();

            numberKeys.Enable();

            numberKeys.performed += PerformedNumberPress;

            scrollWheel.Enable();

            scrollWheel.performed += PerformedScrollWheel;

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

        private void PerformedNumberPress(InputAction.CallbackContext callback)
        {
            int val = (int)(callback.ReadValue<float>());
            int selected = val;

            if (weaponManager.keybindIndex.ContainsKey(selected))
            {
                if (collectedWeapons.Contains(weaponManager.keybindIndex[selected]))
                {
                    ChangeWeapon(collectedWeapons.IndexOf(weaponManager.keybindIndex[selected]));
                }
            }


        }

        float scrollAccumulator = 0f;

        private void PerformedScrollWheel(InputAction.CallbackContext callback)
        {
            float val = (callback.ReadValue<float>()) / 120;

            scrollAccumulator += val;

            if (scrollAccumulator > 0.5f)
            {
                ChangeWeapon(nextAvailableIndex(currentlyHoldingGunIndex));
                scrollAccumulator = 0;
            }
            else if (scrollAccumulator < -0.5f)
            {
                ChangeWeapon(previousAvailableIndex(currentlyHoldingGunIndex));
                scrollAccumulator = 0;
            }

        }

        int nextAvailableIndex(int from)
        {
            for (int i = from + 1; i < collectedWeapons.Count; i++)
            {
                if (collectedWeapons[i] != "")
                    return i;
            }

            return -1;
        }

        int previousAvailableIndex(int from)
        {
            for (int i = from - 1; i > -1; i--)
            {
                if (collectedWeapons[i] != "")
                    return i;
            }

            return -1;
        }

        public void ChangeWeapon(int selected)
        {
            if (selected >= 0 && selected <= collectedWeapons.Count - 1 && selected != currentlyHoldingGunIndex)
            {
                if (objectInteractionHandler != null && objectInteractionHandler.hasObject())
                    return;

                UnequipWeapon();
                currentlyHoldingGunIndex = selected;
                DirectEquip();
            }
        }


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

        //Collect Weapon
        public bool CollectWeapon(string ID)
        {
            if (collectedWeapons.Contains(ID))
            {
                return false;
            }

            //Unequip the Current Weapon
            UnequipWeapon();

            // collectedWeapons.Add(ID);

            // int index = weaponManager.GetWeaponReference(ID).index;

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

            GetCurrentWeapon().State = GetCurrentWeapon().Default;

            UpdateUI();

            return true;
        }

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

        private WeaponReference CurrentWeapon;
        private int lastCurrentIndex;
        private int lastCount = 0;

        //Return the currently holding gun
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