using ClassicFPS.Audio;
using ClassicFPS.Controller.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        //Name of the Animation used for Firing
        public string fireAnimationName;

        //Name of the Animation usd for indicating no Ammo
        public string noAmmoAnimationName;

        //Breathing animation
        public string breathingAnimationName;

        [Header("Positioning")]
        //Used to reposition the gun (usually can be set at 0,0,0)
        public Vector3 relativeToPlayer;

        [Header("Overrides")]
        public bool overridePicking;

        //Finds the Animation in the Gun 
        protected Animation animationComponent;

        private PlayerWeaponController foundWeaponController;

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

            //Find Animation
            animationComponent = GetComponentInChildren<Animation>();

            //Set the Parent to Weapon Mount
            transform.SetParent(weaponController.weaponMount);

            //Sound Source 
            weaponSoundsSource = weaponController.weaponSoundSource;

            //This function can be overriden in child classes to do any other setup functionality
            OnGunEquipped();
        }

        //Should override this
        public virtual void OnGunEquipped()
        {

        }

        public virtual void Unequip()
        {
            //Change the crosshair back
            weaponController.RevertCrosshair();

            //Run any other unequipping behaviour
            OnGunUnequipped();

            //Destroy this GameOBject
            if (transform.gameObject != null)
                GameObject.Destroy(transform.gameObject, 0.0f);

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
            if (animationComponent != null)
            {
                if (animationComponent.GetClip(fireAnimationName) != null)
                {
                    animationComponent.Stop();
                    animationComponent.wrapMode = WrapMode.Once;
                    animationComponent.Play(fireAnimationName);
                }
            }

            if (shootSounds.Count > 0)
            SFXManager.PlayClipFromSource(shootSounds[Random.Range(0,shootSounds.Count)].sound, this.weaponSoundsSource);
        }

        public virtual void RunNoAmmoAnimation()
        {
            if (animationComponent != null)
            {
                if (animationComponent.GetClip(noAmmoAnimationName) != null)
                {
                    animationComponent.Stop();
                    animationComponent.wrapMode = WrapMode.Once;
                    animationComponent.Play(noAmmoAnimationName);

                    
                }
            }

        }


        public virtual void BeginBreathing()
        {
            if (animationComponent != null)
            {
                if (animationComponent.GetClip(breathingAnimationName) != null)
                {
                    animationComponent.Stop();
                    animationComponent.Play(breathingAnimationName);
                    Debug.Log(breathingAnimationName);
                }
            }
        }


    }
}