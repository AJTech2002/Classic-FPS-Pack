using ClassicFPS.Audio;
using ClassicFPS.Guns;
using ClassicFPS.Controller.Interaction;
using ClassicFPS.Saving_and_Loading;
using ClassicFPS.Saving_and_Loading.States;
using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ClassicFPS.Pickups;

namespace ClassicFPS.Controller.PlayerState
{
    //Stores the stats of the Player and is able to Save and Load them
    public class PlayerStatistics : State
    {
        [Header("Player Options")]
        public float maxHealth;

        [Header("UI")]
        public Text healthText;
        public Text coinText;
        public KeyUI keyUI;
        public Animator whiteFlashAnimator;
        public Animator levelTitle;

        [Header("Saving Options")]
        public bool resetHealthOnLoad = true;

        [Header("SFX")]
        public AudioSource damageAudioSource;
        public Sound onTakeDamage;
        public Sound onDeath;

        public List<string> keys = new List<string>();

        //Creating a storage format for the transform component (must be marked Serializable)
        [System.Serializable]
        public struct Stats
        {
            //Keeps track of health
            public float health;

            //Keeps track of coins collected
            public int coins;

            //Keeps track of the states of the guns you have
            public WeaponState[] gunStatistics;

            //Keeps track of collected keys
            public string[] collectedKeys;
        }

        //The current Saved State
        [Header("Player Default & Current State")]
        public Stats playerOptions = new Stats();

        private PlayerWeaponController weaponController;

        private void Awake()
        {
            weaponController = GetComponent<PlayerWeaponController>();
            weaponController.collectedWeapons.Clear();
            UpdateUI();
        }

        public void CollectKey(string keyID)
        {
            keys.Add(keyID);
            keyUI.UpdateUI(keys);
        }

        public bool HasKey(string keyID)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (keyID == keys[i]) return true;
            }

            return false;
        }

        public override string SaveState()
        {
            //Find the weapons controller and the weapons that have currently been collected

            List<string> collectedWeapons = weaponController.collectedWeapons;

            List<WeaponReference> weapons = new List<WeaponReference>();

            foreach (string s in collectedWeapons)
            {
                if (s == "") continue;
                //Map all the collect weapons to some real Weapon
                weapons.Add(weaponController.GetWeaponReference(s));


            }

            //Save the weapons list into a static GunStats array
            playerOptions.gunStatistics = new WeaponState[weapons.Count];

            playerOptions.collectedKeys = keys.ToArray();

            //Find the state of that gun and save it
            for (int i = 0; i < weapons.Count; i++)
            {
                playerOptions.gunStatistics[i] = weapons[i].State;
            }

            return SaveUtils.ReturnJSONFromObject<Stats>(playerOptions);
        }

        public override void LoadState(string loadedJSON)
        {
            //Getting back the struct through JSON
            playerOptions = SaveUtils.ReturnStateFromJSON<Stats>(loadedJSON);

            weaponController.UnequipWeapon();

            weaponController.collectedWeapons.Clear();
            keys.Clear();

            //Set the value of each gun
            foreach (WeaponState gunStat in playerOptions.gunStatistics)
            {
                //Add the gun to the currently collected guns and set the State of that gun
                weaponController.GetWeaponReference(gunStat.gunName).SetState(gunStat);
                weaponController.collectedWeapons.Add(gunStat.gunName);
            }

            if (weaponController.collectedWeapons.Count > 0)
            {
                weaponController.currentlyHoldingGunIndex = 0;
                weaponController.DirectEquip();
            }

            StartCoroutine("WeaponPickupsDisable");

            if (playerOptions.health <= 0 || resetHealthOnLoad)
            {
                playerOptions.health = maxHealth;
            }

            //Remove all the key pickups that share ID's of the keys you already have
            foreach (string s in playerOptions.collectedKeys)
            {
                CollectKey(s);
            }

            UpdateUI();

        }

        //Update the Health and Coin UI
        public void UpdateUI()
        {
            healthText.text = Mathf.RoundToInt(playerOptions.health).ToString();
            coinText.text = playerOptions.coins.ToString();
        }

        //This is where the Enemy calls the damage function of the Player 
        public void TakeDamage(float damage)
        {   
            //Remove health
            playerOptions.health -= damage;

            //Play the damage audio
            onTakeDamage.PlayFromSource(damageAudioSource, 0f);

            playerOptions.health = Mathf.Clamp(playerOptions.health, 0, 1000);

            print("Player took " + damage.ToString() + " damage");

            UpdateUI();

            if (playerOptions.health <= 0)
            {
                onDeath.PlayFromSource(damageAudioSource, 0f);
                Death();
            }

        }

        //Kill the Player, this should be handled how you see fit
        public void Death()
        {
            print("The Player Died, Reloading Scene from last Save Point!");
            if (SaveManager.HasSaveFilesForScene(SceneManager.GetActiveScene().name))
            {
                GameManager.RespawnFromLastSave();
            }
            else
                print("This scene doesn't have a Save file to return to, implement some other logic here.");
        }

        // Disable all the weapon pickups that have already been picked up (duplicate weapons can't be picked up)
        IEnumerator WeaponPickupsDisable()
        {
            yield return new WaitForEndOfFrame();

            //Disable All Pickups
            foreach (WeaponPickup pickup in GameObject.FindObjectsOfType<WeaponPickup>())
            {

                if (weaponController.collectedWeapons.Contains(pickup.GunID))
                {

                    pickup.beenUsed = true;
                    pickup.EnableObject(false);
                }
            }
        }

        public override string GetUID()
        {
            //Global State
            return "PlayerStats";
        }

    }

}