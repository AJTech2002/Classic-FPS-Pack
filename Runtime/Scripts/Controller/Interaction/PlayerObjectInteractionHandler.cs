using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using ClassicFPS.Controller.Movement;
using ClassicFPS.Pushable;

namespace ClassicFPS.Controller.Interaction
{
    public class PlayerObjectInteractionHandler : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera; //Reference to the Player Camera
        private PlayerPhysics physics;
        private PlayerController controller;
        private PlayerWeaponController weaponController;

        [Header("Selection Options")]
        public float selectionRadius; //The radius from the player for which an Object can be selected
        public float selectionMoveSpeed = 10; //The movement of the selected object when it is intersecting with an object (how quickly it comes out)
        public float distanceFromCamera;

        [Header("Throwing Options")]
        public float throwSpeed; //Speed of the throw

        //Store the current state
        private Transform currentlyPickedObject; //Transform of the current object
        private PushableObject currentlyPickedObjectComponent; //The component on the current object
        private int originalLayer = 0; //The original layer of the Object
        private Transform originalParent; //The original parent of the Object 

        private Transform crosshair;

        //Initialize
        private void Start()
        {
            weaponController = GetComponent<PlayerWeaponController>();
            physics = GetComponent<PlayerPhysics>();
            controller = GetComponent<PlayerController>();
            crosshair = GameObject.FindGameObjectWithTag("Crosshair").transform;
        }

        //Return whether or not the Player is currently holding an object
        public bool hasObject()
        {
            return (currentlyPickedObject != null);
        }

        //On Pickup Button Pressed
        public void GetPickupRequest(InputAction.CallbackContext callback)
        {
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
                        //Store the temporary state variables
                        currentlyPickedObject = hitInfo.transform;
                        currentlyPickedObjectComponent = currentlyPickedObject.GetComponent<PushableObject>();
                        originalLayer = currentlyPickedObject.gameObject.layer;
                        originalParent = currentlyPickedObject.parent;

                        if (currentlyPickedObjectComponent.precalculateBounds)
                        {
                            //Center Vertically
                            currentlyPickedObjectComponent.objectHoldingOffset.y = 0;
                            currentlyPickedObjectComponent.objectHoldingOffset.z = Mathf.Clamp(currentlyPickedObject.GetComponent<Collider>().bounds.extents.magnitude * currentlyPickedObject.lossyScale.magnitude * distanceFromCamera, 3, 30);
                          
                        }
                        //Assign a new layer to prevent Ray intersections
                        currentlyPickedObject.gameObject.layer = LayerMask.NameToLayer("Player");

                        //Set its parent to the Camera so it doesn't jitter on movement
                        currentlyPickedObject.SetParent(playerCamera.transform);

                        //Set to a trigger so that it doesn't affect collisions
                        currentlyPickedObject.GetComponent<Collider>().isTrigger = true;

                        //Make sure gravity/other doesn't affect it
                        currentlyPickedObject.GetComponent<Rigidbody>().isKinematic = true;

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

                //Check where the Object would end up due to physics if Player was to let the Object go
                Vector3 newLoc = PreventCollision(currentlyPickedObject.position, currentlyPickedObject.GetComponent<Collider>(), currentlyPickedObject);

                //If the position has been modified then there was a collision
                if (newLoc != currentlyPickedObject.position)
                {
                    //Don't allow the object to be dropped
                    return null;
                }

                //Set to the new location
                currentlyPickedObject.transform.position = newLoc;

                //Set to original values using the temporary storage
                currentlyPickedObject.gameObject.layer = originalLayer;
                currentlyPickedObject.SetParent(originalParent);
                currentlyPickedObject.GetComponent<Collider>().isTrigger = false;

                Rigidbody temp = currentlyPickedObject.GetComponent<Rigidbody>();

                //Remove kinematic state
                temp.isKinematic = false;

                //Set to null so hasObject is false
                currentlyPickedObject = null;
                currentlyPickedObjectComponent = null;

                //Make sure you can shoot weapons now
                weaponController.EquipWeapon(0.3f);

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
                returnedBody.velocity = playerCamera.transform.forward * throwSpeed;
            }
        }

        private void Update()
        {
            //Only Update if Player has an Object
            if (hasObject())
            {
                //Check if the position was modified
                bool wasModified = false;

                //Get the offset defined in the PushableObject component
                Vector3 objectHoldingOffset = currentlyPickedObjectComponent.objectHoldingOffset;

                //Two intersection test have to be done in order to always show the object to the player

                //1: Sphere Cast to check if the Object is currently intersecting with a wall, if so then we can use the hitInfo.distance to move the Object closer
                var (hitInfo, didCollide) = physics.SphereCast(new Ray(playerCamera.transform.position, currentlyPickedObject.forward), 0.1f, objectHoldingOffset.z);

                //2: Ensure that the Player can see the Object, this situation is important if the holding distance is far away
                var (seeObjectHit, foundObject) = physics.Raycast(new Ray(playerCamera.transform.position, currentlyPickedObject.transform.position - playerCamera.transform.position), Mathf.Infinity);

                //Current holding distance
                float holdingDistance = objectHoldingOffset.z;

                //If the Object is colliding with a wall
                if (didCollide)
                {
                    //Move the object closer based on hitInfo.distance (only if it is less than the objectHoldingOffset.z)
                    holdingDistance = Mathf.Min(hitInfo.distance, objectHoldingOffset.z);
                    wasModified = true;
                }

                //In the case we can't see the object, move it to the point where we can see the Object along the same path
                if ((foundObject && seeObjectHit.transform != currentlyPickedObject.transform) && seeObjectHit.distance < objectHoldingOffset.z)
                {
                    holdingDistance = seeObjectHit.distance;
                    wasModified = true;
                }

                //Where the object should end up
                Vector3 predictedPosition = playerCamera.transform.position + playerCamera.transform.forward * holdingDistance + playerCamera.transform.up * objectHoldingOffset.y + playerCamera.transform.right * objectHoldingOffset.x;

                if (wasModified)
                {
                    //If we are treating an intersection then this shouldn't be done instantly as this could cause jiterring motions
                    currentlyPickedObject.transform.position = Vector3.Lerp(currentlyPickedObject.transform.position, predictedPosition, selectionMoveSpeed * Time.deltaTime);
                }
                else
                {
                    //If the Object is not intersecting anything then we can simply set the position
                    currentlyPickedObject.transform.position = predictedPosition;
                }

                //Ensure that the Object also rotates with the Camera so that it is always oriented in the correct direction + rotation extra
                currentlyPickedObject.forward = playerCamera.transform.forward;

                crosshair.localScale = Vector3.Lerp(crosshair.localScale, new Vector3(1f, 1f, 0), Time.deltaTime * 8);

            }
            else if (weaponController.GetActiveWeapon() != null && !weaponController.GetActiveWeapon().overridePicking)
            {
                //Convert the center of the screen to a Ray
                Ray selectionRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                //Do a Sphere Cast outwards (Raycast is too specific and can miss small objects)
                var (hitInfo, didHit) = physics.Raycast(selectionRay, selectionRadius);



                if (didHit && ((hitInfo.transform.GetComponent<PushableObject>() != null && hitInfo.transform.GetComponent<PushableObject>().canBePickedUp) || hitInfo.transform.CompareTag("Interactable")))
                {
                    //Scale up crosshair to indicate something that can be interacted with

                    if (weaponController.IsCurrentWeaponEquipped())
                        crosshair.localScale = Vector3.Lerp(crosshair.localScale, weaponController.GetCurrentWeapon().crosshairSize * weaponController.GetCurrentWeapon().crosshairScaleOnTarget, Time.deltaTime * 8);
                    else
                        crosshair.localScale = Vector3.Lerp(crosshair.localScale, new Vector3(0.5f, 0.5f, 0), Time.deltaTime * 8);

                    crosshair.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(crosshair.GetComponent<UnityEngine.UI.Image>().color, Color.white, Time.deltaTime * 15);
                }
                else
                {
                    if (weaponController.IsCurrentWeaponEquipped())
                        crosshair.localScale = Vector3.Lerp(crosshair.localScale, weaponController.GetCurrentWeapon().crosshairSize, Time.deltaTime * 8);
                    else
                        crosshair.localScale = Vector3.Lerp(crosshair.localScale, new Vector3(0.4f, 0.4f, 0), Time.deltaTime * 8);
                    crosshair.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(crosshair.GetComponent<UnityEngine.UI.Image>().color, new Color(0.7f, 0.7f, 0.7f, 1f), Time.deltaTime * 15);
                }
            }
        }

        //Function that is able to determine before hand whether or not the object is going to intersect with anything (does not require Physics)
        public Vector3 PreventCollision(Vector3 newPosition, Collider collider, Transform transform)
        {
            Vector3 tempPosition = newPosition;

            //Find all the colliders in the sphere near the Player
            Collider[] c = Physics.OverlapSphere(newPosition, 20);

            //Run a loop through each collider nearby the Player
            foreach (Collider col in c)
            {
                if (col.isTrigger)
                {
                    continue;
                }

                //Foreach collider detect how much the Player is penetrating that collider using Physics.ComputePenetration
                Vector3 penDir = new Vector3();
                float penDist = 0f;
                bool d = Physics.ComputePenetration(col, col.transform.position, col.transform.rotation, collider, tempPosition, transform.rotation, out penDir, out penDist);

                //If the Player is intersecting any nearby colliders (for example the Wall)
                if (d == true)
                {
                    //Push the player out of the wall
                    tempPosition += (-penDir.normalized * penDist);
                }
            }

            return tempPosition;
        }

    }
}