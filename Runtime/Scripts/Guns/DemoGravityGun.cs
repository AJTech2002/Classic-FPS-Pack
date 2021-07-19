using ClassicFPS.Audio;
using ClassicFPS.Breakable;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Pushable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ClassicFPS.Utils;
namespace ClassicFPS.Guns
{
    /* A Gravity Gun example on how to use the Weapon base class */
    public class DemoGravityGun : Weapon
    {

        [Header("Options")]
        public PickupUtils gravityGun = new PickupUtils();

        [Header("Input")]
        public InputAction pickupAction; //The key binds used to pickup the objects
        public InputAction throwAction; //The key bind to throw the object

        
        [Header("SFX")]
        public Sound releaseSound; //Sound when releasing the object
        public Sound cantPickupOrThrowSound;

        private Camera playerCamera
        {
            get
            {
                return weaponController._camera;
            }
        }
 

        //When equipped get some references and enable the inputs
        public override void OnGunEquipped()
        {
            pickupAction.performed += PickupActionPerformed;

            throwAction.performed += ThrowActionPerformed;

            pickupAction.Enable();
            throwAction.Enable();

            gravityGun.Setup(weaponController._camera);
        }
        
        //Make sure to remove the inputs when finished so it doesn't continue to play
        public override void OnGunUnequipped()
        {
            pickupAction.performed -= PickupActionPerformed;

            throwAction.performed -= ThrowActionPerformed;

            pickupAction.Disable();
            throwAction.Disable();

            gravityGun.ResetObject();

        }

        

        //During the update function you have to center the object that is being picked and ensure that it doesn't collide with objects
        private void Update()
        {
            gravityGun.UpdateLoop();

            HandlePlayerAnimate();
        }

        //Pick Up & Throw combined into one action
        private void PickupActionPerformed(InputAction.CallbackContext obj)
        {
            
            if (gravityGun.currentlyPickedObject == null && gravityGun.lastShotDelay > 0.5f)
            {
                RaycastHit hit;
                Ray ray = weaponController._camera.ViewportPointToRay(new Vector2(0.5f, 0.5f));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, gravityGun.discludePlayer))
                {

                    if (hit.transform.GetComponent<PushableObject>() != null && hit.transform.GetComponent<PushableObject>().canBePickedUp)
                    {
                        Debug.DrawLine(weaponController.weaponMount.position, hit.point, Color.red, 0.1f);

                        if (shootSounds.Count > 0)
                        SFXManager.PlayClipFromSource(shootSounds[Random.Range(0,shootSounds.Count)].sound, weaponController.weaponSoundSource);
                        gravityGun.OnPickup(hit.transform, this);

                    }
                    else {
                        //Can't Pickup
                        SFXManager.PlayClipFromSource(cantPickupOrThrowSound, weaponController.weaponSoundSource);
                    }

                }
            }
            else if (gravityGun.currentlyPickedObject != null && Vector3.Distance(gravityGun.currentlyPickedObject.transform.position, gravityGun.predictedPosition) <= gravityGun.dropFromExpectedPositionLerp)
            {

                Rigidbody returnedBody = gravityGun.ResetObject();

                if (returnedBody != null)
                {
                    //Set 'thrown' to true when let go, so it has more tendency to break
                    if (returnedBody.GetComponent<BreakableObject>() != null)
                    {
                        returnedBody.GetComponent<BreakableObject>().thrown = true;
                    }

                    SFXManager.PlayClipFromSource(releaseSound, weaponController.weaponSoundSource);

                    //Throw the object by setting velocity
                    returnedBody.velocity = playerCamera.transform.forward * gravityGun.throwSpeed;
                }
                else {
                    //Unable to throw
                    SFXManager.PlayClipFromSource(cantPickupOrThrowSound, weaponController.weaponSoundSource);
                }
            }
            else
            {
                //Reset the object
                gravityGun.ResetObject();
            }

        }

        

        //Throw
        private void ThrowActionPerformed(InputAction.CallbackContext obj)
        {
            if (gravityGun.currentlyPickedObject != null && !gravityGun.controller.hasJumped)
            {
                var a = gravityGun.ResetObject();
                if ( a == null ) SFXManager.PlayClipFromSource(cantPickupOrThrowSound, weaponController.weaponSoundSource);
            }
        }

        

    }
}