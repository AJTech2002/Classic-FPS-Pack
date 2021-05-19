using ClassicFPS.Audio;
using ClassicFPS.Controller.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClassicFPS.Controller.Movement;

namespace ClassicFPS.Guns
{
    //Base Class to be attached to all weapon instances
    public class Weapon : MonoBehaviour
    {

        protected string UID;

        [Header("SFX")]
        [HideInInspector]
        public AudioSource weaponSoundsSource;
        public List<ListSound> shootSounds;
        [Space(10)]
        public Sound emptyAmmoSound;

        [Header("Ammunition")]
        //Whether or not this is a gun that requires ammo (ex. knife won't pickup ammo boxes)
        public bool requiresAmmo = true;

        [Header("Animations")]
        public Animator weaponAnimatorController;
        public AnimatorOverrideController overrideController;
        public float disableDelay = 0.3f;
        public float enableDelay = 0.3f;

        [Header("Positioning")]
        //Used to reposition the gun (usually can be set at 0,0,0)
        public Vector3 relativeToPlayer;

        [Header("Overrides")]
        public bool overridePicking;

        //Finds the Animation in the Gun 
        protected Animation animationComponent;

        private PlayerWeaponController foundWeaponController;

        private PlayerInputManager foundPlayerInputManager;

        protected PlayerInputManager inputManager
        {
            get
            {
                if (foundPlayerInputManager != null) return foundPlayerInputManager;
                foundPlayerInputManager = GameObject.FindObjectOfType<PlayerInputManager>();
                return foundPlayerInputManager;
            }
        }

        protected PlayerWeaponController weaponController
        {
            get
            {
                if (foundWeaponController != null) return foundWeaponController;
                {
                    foundWeaponController = GameObject.FindObjectOfType<PlayerWeaponController>();
                    return foundWeaponController;
                }
            }
        }


        //Equipping procedures
        public virtual void Equip(string UID)
        {

            this.UID = UID;

            //Set the position and rotation of Gun
            transform.position = weaponController.weaponMount.TransformPoint(relativeToPlayer);
            transform.forward = weaponController.weaponMount.forward;

            if (overrideController != null) this.weaponAnimatorController.runtimeAnimatorController = overrideController;


            //Set the Parent to Weapon Mount
            transform.SetParent(weaponController.weaponMount);

            //Sound Source 
            weaponSoundsSource = weaponController.weaponSoundSource;


            if (weaponAnimatorController != null)
            {

                weaponAnimatorController.SetTrigger("enable");

            }

            //This function can be overriden in child classes to do any other setup functionality
            StartCoroutine(EquipAfter(enableDelay));
        }

        IEnumerator EquipAfter (float delay)
        {
            yield return new WaitForSeconds(delay);
            OnGunEquipped();
        }    

        //Should override this
        public virtual void OnGunEquipped()
        {

        }

        public virtual void Unequip()
        {
            if (weaponAnimatorController != null)
            {

                weaponAnimatorController.SetTrigger("disable");

            }
            //Change the crosshair back
            weaponController.RevertCrosshair();

            //Run any other unequipping behaviour
            OnGunUnequipped();

            //Destroy this GameOBject
            if (transform.gameObject != null)
                GameObject.Destroy(transform.gameObject, disableDelay);

        }

        //Should override this
        public virtual void OnGunUnequipped()
        {

        }

        public WeaponState GetState()
        {
            return weaponController.GetCurrentWeapon().State;
        }

        public void SetState(WeaponState newState)
        {
            weaponController.GetWeaponReference(UID).State = newState;
            weaponController.UpdateUI();
        }

        //Animation helpers that find and trigger the animation
        public virtual void RunShootAnimation()
        {
            if (weaponAnimatorController != null)
            {
                weaponAnimatorController.SetBool("hasAmmo", true);
                weaponAnimatorController.SetTrigger("shooting");
            }
            
            if (shootSounds.Count > 0)
            SFXManager.PlayClipFromSource(shootSounds[Random.Range(0,shootSounds.Count)].sound, this.weaponSoundsSource);
        }

        public virtual void RunNoAmmoAnimation()
        {
            if (weaponAnimatorController != null)
            {
                weaponAnimatorController.SetBool("hasAmmo", false);
                weaponAnimatorController.SetTrigger("shooting");
            }

        }

        public virtual void HandlePlayerAnimate ()
        {
            Vector3 inputData = inputManager.inputData;
            PlayerController controller = inputManager.controller;

            if (inputData.magnitude == 0 || controller.hasJumped || !controller.grounded)
            {
               weaponAnimatorController.SetBool("walking", false);
            }
            else
            {
               weaponAnimatorController.SetBool("walking", true);
            }

            if (inputManager.sprinting)
            {
               weaponAnimatorController.SetFloat("walkSpeed", 1.5f);
            }
            else
            {
               weaponAnimatorController.SetFloat("walkSpeed", 1);
            }


        }

        public virtual void HandlePlayerPickup ()
        {
            if (weaponAnimatorController != null)
            {

                weaponAnimatorController.SetTrigger("pickedUp");

            }

        }




    }
}