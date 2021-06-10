using ClassicFPS.Controller.Movement;
using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* This class can be extended */
namespace ClassicFPS.Enemy
{
    [RequireComponent(typeof(SphereCollider))]
    public class Enemy : DamageableEntity
    {
        [Header("Trigger Collider")]
        public SphereCollider trigger;

        [Header("Physics Damage")]
        public float damageByThrownObjectsMultiplier = 1;

        [Header("Navigation")]
        public NavMeshAgent agent;
        public Transform targetTransform;
        public Transform[] patrollingDestinations;
        [SerializeField] int patrolPoint = 0;
        [SerializeField] float patrolLocationScatterMultiplier = 1;
        [SerializeField] float walkRadius;
        [SerializeField] LookAtObject headLookAt;

        [Header("Animations")]
        public Animator animator;

        [Header("Graphics")]
        public GameObject graphics;
        [SerializeField] GameObject deathParticles;


        [Space(10)]
        public AIState currentState;

        [SerializeField] protected PlayerController controller;


        private void Start()
        {
            if(patrollingDestinations.Length != 0) SetUpPatrolDestinations();
            //The sphere collider should be a trigger
            trigger.isTrigger = true;
            controller = GameManager.PlayerController;

            if (controller == null)
                controller = GameObject.FindObjectOfType<PlayerController>();

            if(!targetTransform) targetTransform = controller.transform;


        }

        public override void Die()
        {
            currentState = AIState.Dead;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
            }
            agent.enabled = false;
            animator.enabled = false;
            trigger.enabled = false;

            if (graphics) Destroy(graphics);

            if (deathParticles)
            {
                deathParticles.transform.parent = null;
                deathParticles.SetActive(true);
            }

            foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
            {
                r.enabled = false;
            }

            if (GetComponent<MeshRenderer>() != null)
            {
                GetComponent<MeshRenderer>().enabled = false;
            }

            foreach (Collider r in GetComponentsInChildren<Collider>())
            {
                r.enabled = false;
            }

            if (GetComponent<Collider>() != null)
            {
                GetComponent<Collider>().enabled = false;
            }
            SpawnDrops();
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.transform.CompareTag("Player"))
            {

                FollowPlayer(true);

            }
        }

        private void OnCollisionEnter(Collision col)
        {
            Rigidbody rBody = col.transform.GetComponent<Rigidbody>();

            if (rBody != null)
            {

                if (rBody.velocity.magnitude >= 20)
                {
                    float val = Mathf.Clamp((rBody.velocity.magnitude * damageByThrownObjectsMultiplier), 0f, health);
                    TakeDamage(val);
                }

            }
        }

    

        public void FollowPlayer(bool followPlayer = true)
        {
            if (followPlayer)
            {
                if (currentState == AIState.Idle || currentState == AIState.Patrolling)
                {
                    currentState = AIState.Following;
                    if(headLookAt) headLookAt.target = GameManager.PlayerController.transform;
                }
            }
            else
            {
                if (currentState == AIState.Following)
                {
                    currentState = AIState.Idle;
                }
            }
        }

        public enum AIState
        {
            Idle,
            Patrolling,
            Following,
            WaitingForPath,
            Dead
        }

        public void Patrol()
        {

            if (!agent.pathPending && agent.remainingDistance < 0.5f)

                GoToNextPoint();
        }

        public void GoToNextPoint()
        {
            targetTransform = patrollingDestinations[patrolPoint];

            
            // Returns if no points have been set up
            if (patrollingDestinations.Length == 0)
                return;

            // Set the agent to go to the currently selected destination.
            agent.destination = patrollingDestinations[patrolPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            patrolPoint = (patrolPoint + 1) % patrollingDestinations.Length;
            

           

        }

        void SetUpPatrolDestinations()
        {
            /*
            Transform patrollingDestinationsParent = patrollingDestinations[0].transform.parent.transform;

            patrollingDestinationsParent.parent = null;
            for (int i = 0; i < patrollingDestinations.Length; i++)
            {
                patrollingDestinationsParent.Rotate(Vector3.up, 360 / patrollingDestinations.Length);
                patrollingDestinations[i].transform.Translate(patrollingDestinationsParent.forward * patrolLocationScatterMultiplier);
            }
            */

            Transform patrollingDestinationsParent = patrollingDestinations[0].transform.parent.transform;
            bool pathWasInvalid = false;

            patrollingDestinationsParent.parent = null;
            for (int i = 0; i < patrollingDestinations.Length + 1; i++)
            {
                if (pathWasInvalid)
                {
                    i--;
                    pathWasInvalid = false;
                }

                if (i < patrollingDestinations.Length)
                {

                    NavMeshPath path = new NavMeshPath();

                    Vector3 randomDirection = Random.insideUnitSphere * walkRadius;

                    randomDirection += transform.position;
                    NavMeshHit hit;
                    NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
                    patrollingDestinations[i].position = hit.position;

                    //patrollingDestinations[i].position += Vector3.down * 4;
                    agent.CalculatePath(patrollingDestinations[i].position, path);
                    Debug.Log(path.status);

                    if (path.status == NavMeshPathStatus.PathPartial)
                    {
                        pathWasInvalid = true;
                        Debug.Log("Path was invalid!");
                        continue;
                    }
                }
                
            }

        }

    }
}