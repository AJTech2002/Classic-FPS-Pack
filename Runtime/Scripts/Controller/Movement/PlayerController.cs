using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ClassicFPS.Controller.SFX;
using ClassicFPS.Audio;
using UnityEngine.InputSystem;
using ClassicFPS.Managers;

namespace ClassicFPS.Controller.Movement
{
    public class PlayerController : MonoBehaviour
    {
        //References

        [Header("References")]
        [Tooltip("A Transform Indicating the Current Direction of the Player")]
        public Transform forwardDirection;
        public Transform camera;

        [HideInInspector]
        public CharacterController controller;
        [HideInInspector]
        public PlayerPhysics physics; // Physics Manager
        [HideInInspector]
        public PlayerInputManager inputManager; // Input Manager
        [HideInInspector]
        public PlayerCameraController playerCameraController;
        [SerializeField] GameObject gunMountPosition;

        public PlayerSFX playerSFX;

        //Movement Options
        [Header("Movement Options")]
        public float walkSpeed = 3f; // The walk speed
        public float sprintSpeed = 5f; // The sprint speed
        [SerializeField] float sprintTimeMax = 5;
        bool sprintTimeout = false; //If set to true, require the sprint timer to recover back to 0
        float sprintTime = 0;
        public float jumpSpeed = 5f; // The acceleration imparted on Player when Jump is performed
        public bool sprinting = false; // Are we sprinting? Not if the input "sprint" is true, but if we're actually sprinting! 


        [Header("Slope Options")]
        public float slopeSlipSpeed = 8f; // The acceleration at which a character slides down a slope
        private float slopeSlipThreshold = 0.5f; // If the slope fall distance is too short then don't slip (prevents jittering)
        private float slopeGroundingThreshold = -0.1f; // Distance character can be when a slope is detected

        [Header("Terrain Smoothing")]
        public bool flatGroundOptimised = false;
        [Range(0f, 1f)]
        public float smoothingAmount = 1f;

        [Header("Other")]
        public int enemiesFollowing = 0;

        [HideInInspector]
        public float characterSpeed; // The current character speed (can toggle between walkSpeed and spritnSpeed)
        [HideInInspector]
        public bool grounded; // Grounding State
        [HideInInspector]
        public bool hasJumped; // If the PlayerController is currently in the middle of a jump
        public RaycastHit groundHit; // The RayCastHit containing the properties of the current ground
        public bool isShooting; //Checks to see if the player is shooting. Enemies hear using this!

        //Movement States
        private float verticalForce; // The current vertical force of the player (includes gravity and jumping force)
        private Vector3 acceleration; // The current acceleration of the player 
        [HideInInspector]
        public Vector3 lastAcceleration; // The last known acceleration of the player
        [HideInInspector]
        public bool hasBeenSlipping; // True if the player was slipping on the last frame
        private Vector3 slopeForce; // The current force exerted on the player due to a slope limit exceeded
        private Vector3 input; // The current input vector given to the PlayerController
        private Vector3 externallyAppliedForce; // Any force exerted upon the player

        [HideInInspector]
        public Vector3 lastVelocity;
        private float airTime = 0f;
        private Vector3 lastPosition;

        private bool isTeleporting = false; //Whether or not the player is currently teleporting (used for saving)
        private Vector3 teleportingPosition; //Where to teleport

        private bool wasGrounded = false;

        [Header("Character State")]
        public bool characterIsEnabled = true; // Disable the Player Controller


        //Initialisation 
        private void Awake()
        {
            characterSpeed = walkSpeed;
            controller = GetComponent<CharacterController>();
            playerCameraController = GetComponent<PlayerCameraController>();
            physics = GetComponent<PlayerPhysics>();
            inputManager = GetComponent<PlayerInputManager>();

        }

        //This will provide an approximation for the grounding status of the player (should be used for quick detections)
        public bool isApproximatelyGrounded()
        {
            //If accurately grounded or if the player is close to the ground
            return (grounded || (groundHit.transform != null && groundHit.distance < physics.baseRadius * (1 + physics.groundStickiness) && !isStandingOnSlope()));
        }

        //This is for very accurate grounding detections
        public bool isStrictlyGrounded()
        {
            //Simply if player is grounded 
            return grounded;
        }

        //Whenever a vertical force has to be applied use this
        public void ApplyVerticalForce(float force)
        {
            verticalForce += force;
        }

        //Whenever a vertical force has to be applied use this
        public void SetVerticalForce(float force)
        {
            verticalForce = force;
        }


        //Applies force in direction
        public void ApplyForce(Vector3 forceDirection)
        {
            externallyAppliedForce += forceDirection;
        }

        //Applies immediate force in direction
        public void ApplyImpulseForce(Vector3 forceDirection)
        {
            input = forceDirection;
        }

        //Enables the Player
        public void EnablePlayer()
        {
            characterIsEnabled = true;
        }

        //Disables the Player

        public void DisablePlayer()
        {
            characterIsEnabled = false;
        }

        //Teleports the Player to newPosition (Important to use this function with immediate movements)
        public void TeleportPlayer(Vector3 newPosition)
        {
            isTeleporting = true;
            teleportingPosition = newPosition;
        }

        private void Update()
        {
            if (characterIsEnabled && !isTeleporting)
            {
                //Get and Process Inputs 
                inputManager.ProcessInputs();

                //Generate a Relative Movement Direction from Inputs
                inputManager.GenerateMovementVector();

                //Assign Locally
                input = inputManager.movementVector;

                //Detect if Player is on a slope, pass in 'input' to the function
                SlopeSlip(ref input);

                //Only change speed if the player is grounded
                //transform.right.magnitude




                //controller.velocity.y
                //strafe equation (-inputManager.inputData.x * 5)
                Vector3 mouseSpeed = Mouse.current.delta.ReadValue();
                float controllerFallVelocity = controller.velocity.y;
                if (controller.isGrounded) controllerFallVelocity = 0;

                Quaternion gunMountPositionRotation = Quaternion.Euler((-mouseSpeed.y / 4) + controllerFallVelocity/2f, mouseSpeed.x/4, 90);
                gunMountPosition.transform.localRotation = Quaternion.Lerp(gunMountPosition.transform.localRotation, gunMountPositionRotation, 6f*Time.deltaTime);

                if (!grounded) airTime += Time.deltaTime;

                //Sprinting

                airTime = 0f;
                //Toggle Character Speed based on whether Sprinting action performed


                if (sprinting || sprintTimeout)
                {
                    GameManager.PlayerStatistics.recoveryCircle.enabled = true;
                    GameManager.PlayerStatistics.recoveryCircle.fillAmount = 1 - (sprintTime / sprintTimeMax);
                    GameManager.PlayerStatistics.recoveryCircle.GetComponent<Animator>().SetBool("recovering", sprintTimeout);
                }
                else
                {
                    GameManager.PlayerStatistics.recoveryCircle.enabled = false;
                }

                if (inputManager.sprinting && sprintTime < sprintTimeMax && !sprintTimeout)
                {
                    playerCameraController.virtualCameraNoise.m_AmplitudeGain = playerCameraController.cameraNoiseAmplitudeAndFrequency[1].x;
                    playerCameraController.virtualCameraNoise.m_FrequencyGain = playerCameraController.cameraNoiseAmplitudeAndFrequency[1].y;
                    characterSpeed = sprintSpeed;
                    sprintTime += Time.deltaTime;
                    sprinting = true;
                }
                else
                {

                    if (Mathf.Abs(controller.velocity.x) > 0 || Mathf.Abs(controller.velocity.z) > 0)
                    {
                        playerCameraController.virtualCameraNoise.m_AmplitudeGain = playerCameraController.cameraNoiseAmplitudeAndFrequency[0].x;
                        playerCameraController.virtualCameraNoise.m_FrequencyGain = playerCameraController.cameraNoiseAmplitudeAndFrequency[0].y;
                    }

                    characterSpeed = walkSpeed;
                    sprinting = false;

                    if (sprintTime >= sprintTimeMax) sprintTimeout = true;

                    if (sprintTime >= 0)
                    {
                        sprintTime -= Time.deltaTime / 2;
                    }
                    else
                    {
                        sprintTime = 0;
                        sprintTimeout = false;
                    }

                }


                lastVelocity = (transform.position - lastPosition) / Time.deltaTime;

                //If the Vertical Force (Gravity is increasing but the actual movement does not match this)
                if (verticalForce <= Physics.gravity.y && Mathf.Abs(lastVelocity.y) < 0.05f)
                {
                    verticalForce = Mathf.Clamp(verticalForce, Physics.gravity.y, 0);
                }

                //Only update the movement when there is a movement vector (optimisation)
                if (input != Vector3.zero || verticalForce != 0 || slopeForce.magnitude != 0)
                {
                    //F = ma where m = 1 hence Force = Acceleration in this context

                    //Rotate the forwardDirection transform in the current direction of the camera without tilting object
                    forwardDirection.eulerAngles = new Vector3(forwardDirection.eulerAngles.x, camera.eulerAngles.y, forwardDirection.eulerAngles.z);

                    //Add acceleration from input, slopeForce, externallyAppliedForce, verticalForce all multiplied by Time.deltaTIme for consistency
                    acceleration = input * Time.deltaTime;


                    acceleration += slopeForce * Time.deltaTime;

                    //If has jumped then slope force and input force in y-direction should be eliminated
                    if (hasJumped) acceleration.y = 0;

                    acceleration += externallyAppliedForce * Time.deltaTime;

                    acceleration.y += verticalForce * Time.deltaTime;

                    lastAcceleration = acceleration;




                    lastPosition = transform.position;
                    //Using this function ensures good collision detection that doesn't impact velocity
                    controller.Move(acceleration);

                }

                //Externally Applied Forces are removed
                externallyAppliedForce = Vector3.zero;
            }
            else
            {
                //Stops the update loop if teleporting
                if (isTeleporting)
                {
                    //Changes position
                    transform.position = teleportingPosition;
                    isTeleporting = false;
                }
            }
        }

        public void ClampToGround()
        {
            if (groundHit.transform != null)
                if (groundHit.transform.CompareTag("Terrain") && isStrictlyGrounded() && lastVelocity.magnitude <= 0.1f && inputManager.inputData.magnitude >= 0)
                {
                    var (directGroundingCheck, isStandingOnSlope) = physics.Raycast(new Ray(transform.position, Vector3.down));

                    float v = Mathf.Clamp(-(directGroundingCheck.point.y - (transform.position + Vector3.down * controller.height / 2).y), 0.1f, 1f);

                    ApplyVerticalForce(v);
                }
        }

        //Handles the gravity of the player (in charge of realistic jumping and just falling)
        private void Gravity()
        {
            /*Physics.gravity.y is a negative constant (-9.81), so this condition is basically reducing the vertical force until
              it exceeds 'Physics.gravity.y * physics.gravityScale * 3' . This is to ensure that the gravity doesn't keep rising 
              infinitely which can cause issues. However, it does provide gravity that increases exponentially and not linearly. */

            if (verticalForce > Physics.gravity.y * physics.gravityScale * 3 && !grounded)
            {
                //Time.fixedDeltaTime is used as this function is called in FixedUpdate()
                verticalForce += Physics.gravity.y * physics.gravityScale * Time.fixedDeltaTime;
            }

            //Only play ground hit sound if you were in the air for longer than 0.5f seconds
            if (wasGrounded == false && isApproximatelyGrounded() && airTime > 0.5f)
            {
                if (playerSFX.jumpLandAudioSource.isPlaying == false)
                    SFXManager.PlayClipFromSource(playerSFX.landSound, playerSFX.jumpLandAudioSource);
            }

            wasGrounded = isApproximatelyGrounded();

            //If the player becomes grounded (only when the player is falling)
            if (grounded && verticalForce < 0f)
            {
                //Set verticalForce = 0 meaning the dropping has stopped
                verticalForce = 0f;

                //Also if the character has landed then it is no longer jumping
                hasJumped = false;
            };

            //Approximate grounding is enough to untoggle the jumped parameter to allow for the next jump
            if (isApproximatelyGrounded() && verticalForce < 0f)
            {
                hasJumped = false;
            }



        }

        public void Jump()
        {
            //Uses the SphereCast helper function to check upwards
            var (hit, didHit) = physics.SphereCast(new Ray(transform.position, Vector3.up), physics.baseRadius);

            if (!hasJumped) if (playerSFX != null)
                    SFXManager.PlayClipFromSource(playerSFX.jumpSound, playerSFX.jumpLandAudioSource);

            /* Only allow the jump if the distance to the roof above the player is 
            greater than the height of the player * 1.5, or if there is no roof */

            if ((didHit && hit.distance > controller.height * 1.5f) || !didHit)
            {
                //Can't jump if already jumped
                if (!hasJumped)
                {
                    //Reset Vertical Force
                    verticalForce = 0f;
                    //Apply a Vertical Force of jumpSpeed
                    ApplyVerticalForce(jumpSpeed);
                    //Tell the controller it is jumping now
                    hasJumped = true;
                }
            }
        }

        //Physics based items should be processed in FixedUpdate for consistency across devices
        private void FixedUpdate()
        {
            if (characterIsEnabled)
            {
                //Set the current grounding state

                grounded = physics.isGrounded(out groundHit, Vector3.zero);



                //Apply gravity
                Gravity();

                //On every fixed update check if the player is stuck to a wall
                physics.PreventWallSticking(lastAcceleration, input, lastVelocity);


            }
        }

        public bool isStandingOnSlope()
        {
            //If the angle between the up vector and direction of the ground face exceeds limit then return true
            return (Vector3.Angle(Vector3.up, groundHit.normal) > controller.slopeLimit);
        }

        public Vector3 bottomOfPlayerController()
        {
            //Return the position at the PlayerController's base
            return transform.position + Vector3.down * controller.height / 4;
        }

        float timeOnSlope = 0f;

        public void AddEnemiesFollowing(int amount)
        {
            if (amount >= 0 || (amount < 0 && enemiesFollowing > 0))
            {
                enemiesFollowing += amount;
            }
        }

        private void SlopeSlip(ref Vector3 input)
        {
            if (!grounded)
            {

                bool isCloseToTheGround = (groundHit.distance < physics.baseRadius - slopeGroundingThreshold);

                if (isStandingOnSlope() && isCloseToTheGround)
                {

                    //Generates a small vector pointing towards the slope
                    Vector3 dirToWall = (groundHit.point - transform.TransformPoint(physics.basePoint));
                    dirToWall.y = 0f;
                    dirToWall = dirToWall.normalized;

                    //Create a ray checking from the player's step offset in the direction of the slope
                    Ray stepCheckRay = new Ray(bottomOfPlayerController() + Vector3.up * controller.stepOffset, dirToWall);

                    //Check if any obstacles are below the player's step offset threshold 
                    var (stepHeightCheck, isAnythingBlocking) = physics.Raycast(stepCheckRay, physics.baseRadius * 2);

                    //Check the ground directly below the Player Controller, this is different to the grounding because that utilises SphereCast while this uses a RayCast
                    var (directGroundingCheck, isStandingOnSlope) = physics.Raycast(new Ray(transform.position, Vector3.down));

                    /*
                        In order for the PlayerController to slip down a slope **one** of the following conditions have to be met: 
                            1. There should be no obstacle below the step height of the player, this ensures that the player does not slip down small items like pebbles
                            2. The player was slipping in the last frame
                            3. The ground directly below the player exceeds the slope limit
                    */

                    timeOnSlope += Time.deltaTime;



                    if (isAnythingBlocking || hasBeenSlipping)
                    {

                        //Generate a point on the slope at the same height as the player
                        Vector3 reproject = new Vector3(groundHit.point.x, transform.position.y, groundHit.point.z);

                        //Generate the dot product between the input and the direction towards the slope
                        float dot = Vector3.Dot(reproject - transform.position, new Vector3(input.x, 0, input.z));

                        //Only slide if the input is oppositing the slope direction or no input is given
                        if ((input.magnitude > 0 && dot > -1) || input.magnitude == 0)
                        {
                            //Find a Vector that points down the slope using ProjectOnPlane
                            Vector3 dir = Vector3.ProjectOnPlane(Vector3.down, groundHit.normal).normalized;

                            //Cast a ray in the direction down the slope to see how far the Player has remaining to slide
                            var (collisionCheck, didCollide) = physics.Raycast(new Ray(groundHit.point + groundHit.normal * 0.01f, dir));

                            //In the case that the player is on a ground that is not a slope but they are attemping to climb up a slope, generate a vector along the base of the slope    
                            Vector3 projectSlide = (new Vector3(transform.position.x, groundHit.point.y, transform.position.z) - groundHit.point).normalized;
                            Vector3 projectedMovement = Vector3.ProjectOnPlane(inputManager.movementVector, projectSlide);

                            //Debug.DrawRay(transform.position,projectedMovement*2,Color.red,0.1f);

                            //If the distance to slip is very small then just don't slip, because otherwise it will cause some jiterring motions
                            float slippingDistance = Vector3.Distance(collisionCheck.point, transform.TransformPoint(physics.basePoint));

                            if (slippingDistance < slopeSlipThreshold && !hasBeenSlipping)
                            {
                                //Change the input to be along the base of the slope, the 0.7 slows down the player a little bit for the effect of friction
                                input = (projectedMovement * smoothingAmount);

                                return;
                            }
                            else if (slippingDistance > slopeSlipThreshold)
                            {
                                //hasBeenSlipping determined whether or not the player should go down the slope while keeping track of the slope sliding state
                                hasBeenSlipping = true;
                            }

                            // If hasBeenSlipping then conduct the slipping behaviour
                            if (hasBeenSlipping)
                            {


                                //dir is the Vector that points down the slope and multiplied by the slopeSlipSpeed determines the direction & speed at which the player should slip
                                Vector3 tempSlopeForce = dir * slopeSlipSpeed;

                                //Raycast from the grounding point in the direction of the slope
                                var (slopeCheck, hasFoundCheckPoint) = physics.Raycast(new Ray(groundHit.point, tempSlopeForce));

                                /* The following conditions will make the controller slip:
                                    - If the distance between the player and the projected ground point is greater than the radius of the player, if it is not then the player will clip through the ground
                                    - If there is no projected ground point, meaning the character will fall off a slope
                                    - Or the character has negative vertical force
                                */

                                if ((hasFoundCheckPoint && slopeCheck.distance > physics.baseRadius) || !hasFoundCheckPoint || verticalForce < 0)
                                {
                                    //In the cases that the player has not jumped, then set the input to Vector3.zero, this will result in disabling the input while on a slope
                                    if (!hasJumped || (hasJumped && verticalForce < 0 && verticalForce > -86))
                                    {
                                        input = flatGroundOptimised ? Vector3.zero : (projectedMovement * smoothingAmount);

                                    }

                                    //Change the slope force so that it will affect the controller

                                    slopeForce = tempSlopeForce * Mathf.Clamp(timeOnSlope, 1f, 2);

                                }
                                else
                                {
                                    //Move the character along the base of the slope if the conditions were not met
                                    input = (projectedMovement * smoothingAmount);

                                }

                                return;
                            }
                        }

                    }
                }
            }

            //If the player is not on a slope then the force = 0 and hasBeenSlipping = false
            slopeForce = Vector3.zero;
            hasBeenSlipping = false;
            timeOnSlope = 0f;
        }

    }
}