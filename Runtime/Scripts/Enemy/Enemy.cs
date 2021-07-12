using ClassicFPS.Audio;
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
        //public SphereCollider trigger;

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
        [SerializeField] float patrolPauseTime = 5; //How long I pause between each patrol position
        [SerializeField] float patrolPauseTimeTemp; 
        bool pausingInPatrol = false;

        [Header("Animations")]
        public Animator animator;


        [Header("Aiming")]
        public float aimSpeed = 5;
        float aimSpeedOrig = 0;

        [Header("Shooting")]
        public float shootTimer;
        public float shootDelay = 7;
        public float projectileSpeed;

        [Header("Effects")]
        public GameObject graphics;
        [SerializeField] GameObject deathParticles;
        public AudioClip awakenSound;
        [SerializeField] AudioSource audioSource;
        [SerializeField] GameObject preservedObjectAfterDeath;
        [HideInInspector] public float randomPitchVariation;

        [Header("Ragdoll Effects")]
        [SerializeField] bool isRagdoll;
        [SerializeField] Ragdoll ragdoll;


        [Space(10)]
        public AIState currentState;

        [SerializeField] protected PlayerController controller;


        private void Start()
        {
            patrolPauseTime += Random.Range(-4f, 4f);
            aimSpeedOrig = aimSpeed;
            if (patrollingDestinations.Length != 0) SetUpPatrolDestinations();
            //The sphere collider should be a trigger
            //trigger.isTrigger = true;
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
            //trigger.enabled = false;

            if (preservedObjectAfterDeath) preservedObjectAfterDeath.transform.parent = null;

            SpawnDrops();
            controller.AddEnemiesFollowing(-1);

            if (deathParticles)
            {
                deathParticles.transform.parent = null;
                deathParticles.SetActive(true);
            }

            if (isRagdoll)
            {
                ragdoll.EnableRagdoll(true);
            }
            else
            {
                if (graphics) Destroy(graphics);

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
            }

            Destroy(gameObject);
            Time.timeScale = 1;
            
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
                Debug.Log("Follow the player!");
                if (currentState == AIState.Idle || currentState == AIState.Patrolling)
                {
                    currentState = AIState.Following;
                    if(headLookAt) headLookAt.target = GameManager.PlayerController.transform;
                    aimSpeed = aimSpeedOrig;
                    controller.AddEnemiesFollowing(1);
                    //Play awake SFX
                    if(audioSource) audioSource.PlayOneShot(awakenSound, 1.2f);
                }
            }
            else
            {
                if (currentState == AIState.Following)
                {
                    controller.AddEnemiesFollowing(-1);
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
            if (!agent.pathPending && agent.remainingDistance < 0.5f && !pausingInPatrol)
                StartCoroutine(GoToNextPoint());
        }

        IEnumerator GoToNextPoint()
        {
            pausingInPatrol = true;
            aimSpeed = 0;
            yield return new WaitForSeconds(patrolPauseTimeTemp);

            targetTransform = patrollingDestinations[patrolPoint];

            // Returns if no points have been set up
            if (patrollingDestinations.Length == 0)
                yield break;

            // Set the agent to go to the currently selected destination.
            agent.destination = patrollingDestinations[patrolPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            patrolPoint = (patrolPoint + 1) % patrollingDestinations.Length;
            patrolPauseTimeTemp = patrolPauseTime;
            pausingInPatrol = false;
            aimSpeed = aimSpeedOrig;
        }



        void SetUpPatrolDestinations()
        {

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

                    agent.CalculatePath(patrollingDestinations[i].position, path);
                    Debug.Log(path.status);

                    if (path.status == NavMeshPathStatus.PathPartial || PathLength(path) > walkRadius)
                    {
                        pathWasInvalid = true;
                        Debug.Log("Path was invalid!");
                        continue;
                    }
                }
                
            }

        }

        float PathLength(NavMeshPath path)
        {
            if (path.corners.Length < 2)
                return 0;

            Vector3 previousCorner = path.corners[0];
            float lengthSoFar = 0.0F;
            int i = 1;
            while (i < path.corners.Length)
            {
                Vector3 currentCorner = path.corners[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }
            return lengthSoFar;
        }

    }
}