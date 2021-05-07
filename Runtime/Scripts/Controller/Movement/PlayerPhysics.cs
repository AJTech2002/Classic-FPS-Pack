using ClassicFPS.Pushable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassicFPS.Controller.Movement
{
    public class PlayerPhysics : MonoBehaviour
    {
        //References
        private CapsuleCollider capsuleCollider;
        private CharacterController _controller;
        private PlayerController controller;

        [Header("Grounding and Gravity")]
        public float gravityScale; //How powerful the gravity is compared to the default Unity Gravity
        [Tooltip("A LayerMask that contains all Layers except the player layer")]
        public LayerMask discludePlayer; //A layermask that discludes the player, this allows to make raycasts that are not blocked by the player itself
        public LayerMask dontStepLayer; //A layermask that prevents stepping onto objects

        [Header("Pushing")]
        public float pushPower = 100f; //Power in the push (additional to the characterSpeed)
        public float pushableObjectVelocityMax = 100f; //Maximum Velocity for Pushable Object

        [Header("Reference Shape")]
        [Tooltip("Where is the base of the player controller")]
        public Vector3 referenceBasePoint = new Vector3(0, -0.5f, 0); //The reference base point that can be tweaked in the editor

        //The real base point takes into account the stepOffset of the character controller
        public Vector3 basePoint
        {
            get
            {
                return referenceBasePoint + Vector3.down * baseRadius + Vector3.up * _controller.stepOffset;
            }
        }

        [Tooltip("What is the radius of the player controller")]
        public float baseRadius = 0.5f; //The radius of the player controller, usually 0.5

        private float movementTolerance = 0.05f; //The minimum movement before the script detects that you are 'stuck' in a wall

        [HideInInspector]
        public float groundStickiness = 0.4f; //How close to the ground the player can be to be grounded

        //Radius modifiers to SphereCast in/out of the default radius
        private float radiusTolerance = 0f;
        private float outterRadiusTolerance = 0.1f;

        //Initialise references
        private void Start()
        {
            controller = GetComponent<PlayerController>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            _controller = controller.controller;
        }

        //Helper Raycast function that takes in simple parameters and returns the Raycast Hit Information and whether or not it hit anything 
        public (RaycastHit, bool) Raycast(Ray ray, float minDist = Mathf.Infinity)
        {
            RaycastHit hit;

            //Utilises the discludePlayer LayerMask
            if (Physics.Raycast(ray, out hit, minDist, discludePlayer))
            {
                return (hit, true);
            }

            return (hit, false);
        }

        //Similar to Raycast but uses a SphereCast
        public (RaycastHit, bool) SphereCast(Ray ray, float radius, float minDist = Mathf.Infinity)
        {
            RaycastHit hit;
            if (Physics.SphereCast(ray.origin, radius, ray.direction, out hit, minDist, discludePlayer))
            {
                return (hit, true);
            }

            return (hit, false);
        }

        //Draw a Simple Gizmos to help position the base point and radius
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.TransformPoint(referenceBasePoint), baseRadius);
            if (_controller != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.TransformPoint(basePoint), baseRadius);
            }
        }

        //A sophisticated grounding function for the player
        public bool isGrounded(out RaycastHit groundHit, Vector3 extrapolate)
        {

            RaycastHit hit;

            //Pass out the groundHit variable from the function
            groundHit = new RaycastHit();

            //Conduct a SphereCast from the basePoint downwards
            if (Physics.SphereCast(transform.TransformPoint(referenceBasePoint) + extrapolate, baseRadius - radiusTolerance * radiusTolerance, Vector3.down, out hit, Mathf.Infinity, discludePlayer))
            {
                groundHit = hit;

                //Using the found normal, conduct a raycast from the basePoint towards the surface
                RaycastHit checkHit;
                Ray checkRay = new Ray(transform.TransformPoint(referenceBasePoint) + extrapolate, -hit.normal);

                //This Raycast will find the closest point to the basePoint on the surface of the mesh
                if (Physics.Raycast(checkRay, out checkHit, Mathf.Infinity, discludePlayer))
                {
                    //If the distance to that point is lower than the radius + tolerance then it is grounded
                    if (checkHit.distance < baseRadius + outterRadiusTolerance)
                    {
                        //It can only be grounded if it is not standing on a slope
                        if (Vector3.Angle(checkHit.normal, Vector3.up) < _controller.slopeLimit)
                        {
                            groundHit = checkHit;
                            return true;
                        }

                    }
                }
            }

            //If the conditions fail then Player is not grounded
            return false;
        }

        public void Clamp(Vector3 extrapolate)
        {

            RaycastHit hit;


            //Conduct a SphereCast from the basePoint downwards
            if (Physics.SphereCast(transform.TransformPoint(referenceBasePoint) + extrapolate, baseRadius - radiusTolerance * radiusTolerance, Vector3.down, out hit, Mathf.Infinity, discludePlayer))
            {

                //Using the found normal, conduct a raycast from the basePoint towards the surface
                RaycastHit checkHit;
                Ray checkRay = new Ray(transform.TransformPoint(referenceBasePoint) + extrapolate, -hit.normal);

                //This Raycast will find the closest point to the basePoint on the surface of the mesh
                if (Physics.Raycast(checkRay, out checkHit, Mathf.Infinity, discludePlayer))
                {
                    //If the distance to that point is lower than the radius + tolerance then it is grounded
                    if (checkHit.distance < baseRadius + outterRadiusTolerance)
                    {
                        Vector3 offset = (checkHit.point + hit.normal * baseRadius) - transform.TransformPoint(referenceBasePoint);
                        transform.position += offset;
                    }
                }
            }

        }


        //Pass in the currentVelocity of the Player
        public void PreventWallSticking(Vector3 currentVelocity, Vector3 input, Vector3 velocity)
        {
            //The Player can only be stuck in the wall if the controller is in the air and the velocity's magnitude is really small (meaning it is not moving)

            if ((!controller.isApproximatelyGrounded() && currentVelocity.magnitude <= movementTolerance))
            {
                Vector3 added = currentVelocity;

                //Find all the colliders in the sphere near the Player

                Collider[] c = Physics.OverlapSphere(transform.position, 10, discludePlayer);

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
                    bool d = Physics.ComputePenetration(col, col.transform.position, col.transform.rotation, capsuleCollider, transform.position + added, transform.rotation, out penDir, out penDist);

                    //If the Player is intersecting any nearby colliders (for example the Wall)
                    if (d == true)
                    {
                        //Push the player out of the wall
                        transform.position += (-penDir.normalized * penDist);

                    }
                }
            }
        }



        //A Collision Detection System provided by the CharacterController
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            // no rigidbody, kinematic or the tag isn't pushable
            if (body == null || body.isKinematic || body.GetComponent<PushableObject>() == null || body.velocity.magnitude > pushableObjectVelocityMax || body.angularVelocity.magnitude > pushableObjectVelocityMax)
            {
                return;
            }

            // Calculate push direction from move direction,
            // we only push objects to the sides never up and down
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            // Apply the push, also multiplied by the characterSpeed as a percentage 
            Vector3 force = (pushDir + controller.lastAcceleration).normalized * pushPower * (controller.characterSpeed / controller.sprintSpeed);

            //Clamp the force to ensure no large forces are imparted
            force = Vector3.ClampMagnitude(force, pushPower);

            //Push the rigidbody
            body.AddForceAtPosition(force, hit.point, ForceMode.Force);

            //Tell the PushableObject that it is being moved
            body.GetComponent<PushableObject>().BeingPushed();

        }


    }
}
