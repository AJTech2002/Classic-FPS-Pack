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
        public Image recoveryCircle;


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

        public void UpdateUI()
        {
            healthText.text = Mathf.RoundToInt(playerOptions.health).ToString();
            coinText.text = playerOptions.coins.ToString();
        }

        public void TakeDamage(float damage)
        {
            playerOptions.health -= damage;

            SFXManager.PlayClipFromSource(onTakeDamage, damageAudioSource, 0f);

            StartCoroutine(GameManager.PlayerController.playerCameraController.ShakeScreen(3f, 7f, .14f));
            GameManager.PlayerStatistics.whiteFlashAnimator.ResetTrigger("Hurt");
            GameManager.PlayerStatistics.whiteFlashAnimator.SetTrigger("Hurt");
            Debug.Log("TakeDamange!!");
            playerOptions.health = Mathf.Clamp(playerOptions.health, 0, 1000);

            UpdateUI();

            if (playerOptions.health <= 0)
            {
                SFXManager.PlayClipFromSource(onDeath, damageAudioSource, 0f);
                Death();
            }

        }

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