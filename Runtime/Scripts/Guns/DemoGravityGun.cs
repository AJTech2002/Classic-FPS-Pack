using ClassicFPS.Audio;
using ClassicFPS.Breakable;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Pushable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClassicFPS.Guns
{
    public class DemoGravityGun : Weapon
    {

        [Header("Physics")]
        public LayerMask discludePlayer;

        [Header("Input")]
        public InputAction pickupAction;
        public InputAction throwAction;

        [Header("Gravity Gun Options")]
        public float pickupSpeed;
        public float throwSpeed;
        public float distanceFromCamera = 1.2f;
        public float maximumDamage;

        [Header("SFX")]
        public Sound releaseSound;

        [Header("UI Options")]
        public Color gravityGunCrosshairColor;

        private float lastShotDelay = 0f;

        private Transform currentlyPickedObject;
        private PushableObject currentlyPickedObjectComponent;
        private PlayerPhysics physics;
        private PlayerController controller;
        private Vector3 predictedPosition;

        private Camera playerCamera
        {
            get
            {
                return weaponController._camera;
            }
        }

        private int originalLayer = 0; //The original layer of the Object
        private Transform originalParent; //The original parent of the Object 

        public override void OnGunEquipped()
        {
            physics = GameObject.FindObjectOfType<PlayerPhysics>();
            controller = GameObject.FindObjectOfType<PlayerController>();

            pickupAction.performed += PickupActionPerformed;



            throwAction.performed += ThrowActionPerformed;

            pickupAction.Enable();
            throwAction.Enable();
        }

        public override void OnGunUnequipped()
        {
            pickupAction.performed -= PickupActionPerformed;


            throwAction.performed -= ThrowActionPerformed;

            pickupAction.Disable();
            throwAction.Disable();


            ResetObject();

        }

        private void Update()
        {
            lastShotDelay += Time.deltaTime;
            if (currentlyPickedObject != null)
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
                predictedPosition = playerCamera.transform.position + playerCamera.transform.forward * holdingDistance + playerCamera.transform.up * objectHoldingOffset.y + playerCamera.transform.right * objectHoldingOffset.x;

                if (wasModified)
                {
                    //If we are treating an intersection then this shouldn't be done instantly as this could cause jiterring motions
                    currentlyPickedObject.transform.position = Vector3.Lerp(currentlyPickedObject.transform.position, predictedPosition, 10 * Time.deltaTime);
                }
                else
                {
                    //If the Object is not intersecting anything then we can simply set the position

                    if (Vector3.Distance(currentlyPickedObject.transform.position, predictedPosition) >= 0.01f)
                    {
                        currentlyPickedObject.transform.position = Vector3.Lerp(currentlyPickedObject.transform.position, predictedPosition, pickupSpeed * Time.deltaTime);
                    }
                    else
                    {
                        currentlyPickedObject.transform.position = predictedPosition;
                    }
                }

            }
            else
            {
                RaycastHit hit;
                Ray ray = weaponController._camera.ViewportPointToRay(new Vector2(0.5f, 0.5f));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, discludePlayer))
                {

                    if (hit.transform.GetComponent<PushableObject>() != null && hit.transform.GetComponent<PushableObject>().canBePickedUp)
                    {
                        weaponController.crosshairUI.color = Color.Lerp(weaponController.crosshairUI.color, gravityGunCrosshairColor, 15 * Time.deltaTime);
                        weaponController.crosshairUI.transform.localScale = Vector3.Lerp(weaponController.crosshairUI.transform.localScale, weaponController.GetCurrentWeapon().crosshairSize * weaponController.GetCurrentWeapon().crosshairScaleOnTarget, Time.deltaTime * 8);
                    }
                    else
                    {
                        weaponController.crosshairUI.color = Color.Lerp(weaponController.crosshairUI.color, Color.white, 15 * Time.deltaTime);
                        weaponController.crosshairUI.transform.localScale = Vector3.Lerp(weaponController.crosshairUI.transform.localScale, weaponController.GetCurrentWeapon().crosshairSize, Time.deltaTime * 8);
                    }
                }
                else
                {
                    weaponController.crosshairUI.color = Color.Lerp(weaponController.crosshairUI.color, Color.white, 15 * Time.deltaTime);
                    weaponController.crosshairUI.transform.localScale = Vector3.Lerp(weaponController.crosshairUI.transform.localScale, weaponController.GetCurrentWeapon().crosshairSize, Time.deltaTime * 8);
                }
            }
        }

        //Pick Up & Throw combined into one action
        private void PickupActionPerformed(InputAction.CallbackContext obj)
        {
            if (currentlyPickedObject == null && lastShotDelay > 0.5f)
            {
                RaycastHit hit;
                Ray ray = weaponController._camera.ViewportPointToRay(new Vector2(0.5f, 0.5f));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, discludePlayer))
                {

                    if (hit.transform.GetComponent<PushableObject>() != null && hit.transform.GetComponent<PushableObject>().canBePickedUp)
                    {
                        Debug.DrawLine(weaponController.weaponMount.position, hit.point, Color.red, 0.1f);

                        SFXManager.PlayClipFromSource(shootSound, weaponController.weaponSoundSource);

                        currentlyPickedObject = hit.transform;

                        originalLayer = currentlyPickedObject.gameObject.layer;
                        originalParent = currentlyPickedObject.parent;

                        //Assign a new layer to prevent Ray intersections
                        currentlyPickedObject.gameObject.layer = LayerMask.NameToLayer("Player");

                        //Set its parent to the Camera so it doesn't jitter on movement
                        currentlyPickedObject.SetParent(weaponController._camera.transform);

                        //Set to a trigger so that it doesn't affect collisions
                        currentlyPickedObject.GetComponent<Collider>().isTrigger = true;

                        //Make sure gravity/other doesn't affect it
                        currentlyPickedObject.GetComponent<Rigidbody>().isKinematic = true;

                        currentlyPickedObjectComponent = currentlyPickedObject.GetComponent<PushableObject>();

                        if (currentlyPickedObjectComponent.precalculateBounds)
                            currentlyPickedObjectComponent.objectHoldingOffset.z = currentlyPickedObject.lossyScale.magnitude * distanceFromCamera;

                    }

                }
            }
            else if (currentlyPickedObject != null && Vector3.Distance(currentlyPickedObject.transform.position, predictedPosition) <= 0.3f)
            {

                Rigidbody returnedBody = ResetObject();

                if (returnedBody != null)
                {
                    if (returnedBody.GetComponent<BreakableObject>() != null)
                    {
                        returnedBody.GetComponent<BreakableObject>().thrown = true;
                    }

                    SFXManager.PlayClipFromSource(releaseSound, weaponController.weaponSoundSource);

                    //Throw the object by setting velocity
                    returnedBody.velocity = playerCamera.transform.forward * throwSpeed;
                }
            }
            else
            {
                ResetObject();
            }


        }

        private Rigidbody ResetObject()
        {
            //Can only be done if Player has an Object
            if (currentlyPickedObject != null)
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


                lastShotDelay = 0f;

                return temp;



            }


            return null;
        }

        //Throw
        private void ThrowActionPerformed(InputAction.CallbackContext obj)
        {
            if (currentlyPickedObject != null && !controller.hasJumped)
            {
                ResetObject();
            }
        }

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