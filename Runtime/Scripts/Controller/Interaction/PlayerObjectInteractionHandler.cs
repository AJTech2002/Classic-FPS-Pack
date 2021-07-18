using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ClassicFPS.Utils;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Pushable;

// This script is used mainly used to handle the picking up of objects

namespace ClassicFPS.Controller.Interaction
{
    public class PlayerObjectInteractionHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera; //Reference to the Player Camera
        private PlayerPhysics physics;
        private PlayerController controller;
        private PlayerWeaponController weaponController;

        [Header("Pickup Options")]
        public PickupUtils pickup;

        [Header("Selection Options")]
        [SerializeField] private float selectionRadius; //The radius from the player for which an Object can be selected
     
        //Initialize
        private void Start()
        {
            weaponController = GetComponent<PlayerWeaponController>();
            physics = GetComponent<PlayerPhysics>();
            controller = GetComponent<PlayerController>();
            pickup.Setup(playerCamera);
        }

        //Return whether or not the Player is currently holding an object
        public bool hasObject()
        {
            return (pickup.currentlyPickedObject != null);
        }

        //On Pickup Button Pressed
        public void GetPickupRequest(InputAction.CallbackContext callback)
        {
            //Ensure that the Player doesn't have an object
            if (!hasObject() && weaponController.GetActiveWeapon() != null && !weaponController.GetActiveWeapon().overridePicking)
            {
                //Convert the center of the screen to a Ray
                Ray selectionRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                //Do a Sphere Cast outwards (Raycast is too specific and can miss small objects)
                var (hitInfo, didHit) = physics.SphereCast(selectionRay, 0.1f, selectionRadius);

                if (didHit)
                {
                    //If there was a hit, check if there is a PushableObject and whether or not it is allowed to be picked up
                    if (hitInfo.transform.GetComponent<PushableObject>() != null && hitInfo.transform.GetComponent<PushableObject>().canBePickedUp)
                    {
                        pickup.OnPickup(hitInfo.transform, this);

                        //Make sure you can't shoot weapons now
                        weaponController.UnequipWeapon();
                    }
                }

            }
        }

        //Function to Reset the object to its former state
        private Rigidbody ResetObject()
        {
            //Can only be done if Player has an Object
            if (hasObject())
            {
                Rigidbody temp = pickup.ResetObject();
                if (temp != null)
                {
                    //Make sure you can shoot weapons now
                    weaponController.EquipWeapon(0.3f);
                }

                return temp;
            }

            return null;
        }

        //On Drop Button Pressed
        public void GetDropRequest(InputAction.CallbackContext callback)
        {
            if (!controller.hasJumped)
                ResetObject();
        }

        //On Throw Button Pressed
        public void GetThrowRequest(InputAction.CallbackContext callback)
        {
            Rigidbody returnedBody = ResetObject();

            if (returnedBody != null)
            {
                //Throw the object by setting velocity
                returnedBody.velocity = playerCamera.transform.forward * pickup.throwSpeed;
            }
        }

        private void Update()
        {
            //Only Update if Player has an Object
            if (hasObject())
            {
                pickup.CallibrateObjectPosition();

            }
            else if (weaponController.GetActiveWeapon() != null && !weaponController.GetActiveWeapon().overridePicking)
            {
                pickup.ModifyCrosshair();
            }
        }

        

    }
}