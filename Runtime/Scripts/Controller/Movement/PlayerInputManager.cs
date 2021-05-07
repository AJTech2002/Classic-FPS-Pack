using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace ClassicFPS.Controller.Movement
{
    public class PlayerInputManager : MonoBehaviour
    {
        //References to external scripts
        private PlayerController controller;
        private CharacterController _controller;
        private PlayerPhysics physics;

        public Animator playerAnimator;

        //The gravity and sensitivity of the input keys
        private float gravity = 15f; //How quickly the values of inputs fall ('slippiness')
        private float sensitivity = 15f; //How quickly the values of inputs rise ('snapiness')

        //The input after it has been processed
        private float processedX, processedZ;

        //The input data given by the keyboard
        [HideInInspector]
        public Vector2 inputData;
        private Vector3 generatedMovementData;


        [HideInInspector]
        public bool sprinting = false;

        public Vector2 processedInputs
        {
            get
            {
                return new Vector2(processedX, processedZ);
            }
        }

        //The output movement vector
        public Vector3 movementVector
        {
            get
            {
                return generatedMovementData;
            }
        }

        //Initialise references
        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            _controller = GetComponent<CharacterController>();
            physics = GetComponent<PlayerPhysics>();

        }

        //On WASD Pressed, this is given out
        public void GetMoveRequest(InputAction.CallbackContext callback)
        {
            //Input data is given as a raw Vector2 as either -1,0,1 for both the x and y axis
            inputData = callback.ReadValue<Vector2>();
        }

        //On Sprint Button Pressed
        public void GetSprintRequest(InputAction.CallbackContext callback)
        {
            sprinting = callback.ReadValueAsButton();
        }

        //On Jump Button Pressed
        public void GetJumpRequest(InputAction.CallbackContext callback)
        {
            //Only jump if the controller is approximately grounded
            if (controller.isApproximatelyGrounded() && callback.ReadValueAsButton() == true)
            {
                controller.Jump();
            }

        }

        //Input Processing where raw inputs are converted into smoothly interpolated inputs (between -1 and 1)
        public void ProcessInputs()
        {
            //Notice how the processedX and Y interpolate using Time.deltaTime

            if (inputData.x == 0 && !Mathf.Approximately(processedX, 0))
            {
                processedX = Mathf.Lerp(processedX, 0, Time.deltaTime * gravity);
            }
            if (inputData.y == 0 && !Mathf.Approximately(processedZ, 0))
            {
                processedZ = Mathf.Lerp(processedZ, 0, Time.deltaTime * gravity);
            }
            if (inputData.x != 0 || inputData.y != 0)
            {

                processedX = Mathf.Clamp(processedX + (inputData.x * Time.deltaTime * sensitivity), -1, 1);
                processedZ = Mathf.Clamp(processedZ + (inputData.y * Time.deltaTime * sensitivity), -1, 1);
            }

            if (inputData.magnitude == 0 || controller.hasJumped || !controller.grounded)
            {
                playerAnimator.SetBool("walking", false);
            }
            else
            {
                playerAnimator.SetBool("walking", true);
            }

            if (sprinting)
            {
                playerAnimator.SetFloat("speed", 1.5f);
            }
            else
            {
                playerAnimator.SetFloat("speed", 1);
            }

        }

        //Where the input vector is processed
        public Vector3 GenerateMovementVector()
        {
            generatedMovementData = new Vector3(processedX, 0, processedZ);
            generatedMovementData = Vector3.ClampMagnitude(generatedMovementData, 1) * controller.characterSpeed;

            //Make the input vector relative to the forwardDirection (which is the Camera forward direction)
            generatedMovementData = controller.forwardDirection.TransformDirection(generatedMovementData);

            //Set input to 0 so that Camera up doesn't affect character
            generatedMovementData.y = 0;

            //If the character is grounded we should move the character along the ground
            if (controller.grounded || controller.groundHit.distance < physics.baseRadius * (1 + physics.groundStickiness))
            {
                //Adjusting the inputs to move along the slope that it is currently on


                RaycastHit groundHit = controller.groundHit;

                var (hasGoodGround, dir) = ReturnAverageDirection(generatedMovementData);

                if (hasGoodGround)
                {
                    //Check if there is a block in the input direction
                    Ray collisionCheckRay = new Ray(controller.bottomOfPlayerController() + Vector3.up * _controller.stepOffset, dir);

                    var (collisionCheck, didCollide) = physics.Raycast(collisionCheckRay);

                    Ray bottom = new Ray(controller.transform.position, Vector3.down);

                    var (bottomCheck, bottomDidCollide) = physics.Raycast(bottom);

                    float movementToleranceValue = physics.baseRadius * 2;

                    //If there isn't a meaningful collision then the Player can move along the surface
                    if (!didCollide || (didCollide && collisionCheck.distance > movementToleranceValue))
                    {
                        generatedMovementData = dir * generatedMovementData.magnitude;
                    }
                    else
                    {
                        //Revert back to no vertical movement
                        generatedMovementData.y *= 0f;
                    }

                    if (bottomCheck.transform != null)
                        if (bottomCheck.transform.CompareTag("Stairs")) { generatedMovementData.y = 0; }

                }
            }

            controller.ClampToGround();
            return generatedMovementData;
        }

        //Simple function that determines how to best move along an inclined surface without falling off
        public (bool, Vector3) ReturnAverageDirection(Vector3 inputDir, int overridePrefer = -1)
        {
            RaycastHit hit = controller.groundHit;

            //Slightly off the ground
            Vector3 origin = hit.point + hit.normal * 0.01f;

            //Conduct one Raycast slightly infront of the Player
            var (hitInfo1, hit1) = physics.Raycast(new Ray(origin + inputDir * Time.deltaTime * 0.01f, Vector3.down), Mathf.Infinity);
            if (Vector3.Angle(Vector3.up, hitInfo1.normal) > _controller.slopeLimit)
            {
                hit1 = false;
            }

            //Conduct one Raycast right where the Player is
            var (hitInfo2, hit2) = physics.Raycast(new Ray(origin, Vector3.down), Mathf.Infinity);

            if (Vector3.Angle(Vector3.up, hitInfo2.normal) > _controller.slopeLimit)
            {
                hit2 = false;
            }

            bool hasGrounding = hit1 && hit2;
            bool hitDistancesClose = Vector3.Distance(hitInfo1.point, hitInfo2.point) < _controller.radius;
            bool notOnVerticalSurface = Vector3.Angle((hitInfo1.point - hitInfo2.point).normalized, Vector3.down) > 10;

            //If both the hits were good, the distances between the hits were close and if the hit angle wasn't too extreme, then we can move along this direciton

            if (hasGrounding && hitDistancesClose && notOnVerticalSurface)
            {
                //Get the direction of the movement by comparing the two hit positions and subtracting them, this provides an input vector along surface
                if (hitInfo2.point.y < transform.position.y && hitInfo1.point.y < transform.position.y)
                    return (true, (hitInfo1.point - hitInfo2.point).normalized);
            }


            return (false, Vector3.zero);

        }

    }
}