using ClassicFPS.Controller.PlayerState;
using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ClassicFPS.Enemy
{

    public class SampleEnemy : Enemy
    {
        [Header("Player Injury Options")]
        public float attackRadius;
        public float attackDelay;
        public float damageByProximity;
        [SerializeField] private bool usesHurtBox = false;
        private float attackTimeout;

        [Header("Projectile Options")]
        public bool shootProjectiles;
        public Transform projectilePrefab;
        public Transform gunModel;
        public Transform projectileSpawnPoint;
        public float projectileSpeed;

        [Header("Hearing")]
        [SerializeField] float hearingPower = 5;
        Vector3 boxColliderCenterOrig;
        Vector3 boxColliderSizeOrig;
        [SerializeField]BoxCollider boxCollider;


        [Header("Other")]
        [SerializeField] bool rotationHandledByGroundfitter = false; //If this is true, rotation is not handled in this script

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
            boxColliderSizeOrig = boxCollider.size;
            boxColliderCenterOrig = boxCollider.center;
        }

        private void Update()
        {

            if (GameManager.PlayerController.isShooting)
            {
                boxCollider.size = boxColliderSizeOrig * hearingPower;
                boxCollider.center = Vector3.zero;
            }
            else
            {
                boxCollider.size = boxColliderSizeOrig;
                boxCollider.center = boxColliderCenterOrig;
            }

           
            if ((currentState == AIState.Following || currentState == AIState.Patrolling) && controller != null)
            {
                float dist = Vector3.Distance(transform.position, controller.transform.position);

                if (agent.velocity.magnitude > 0.05f && dist > attackRadius)
                {
                    animator.SetBool("walking", true);
                }
                else
                {
                    animator.SetBool("walking", false);
                }

                //Vector3 lookAt = new Vector3(targetTransform.position.x, transform.position.y, targetTransform.transform.position.z);

                //transform.LookAt(Vector3.Lerp(transform.forward, lookAt, aimSpeed));

                if (!rotationHandledByGroundfitter)
                {
                    Vector3 dir = targetTransform.position - transform.position + transform.forward;
                    dir.y = 0;//This allows the object to only rotate on its y axis
                    Quaternion rot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rot, aimSpeed * Time.deltaTime);
                }
                

             
                if (dist >= attackRadius - 0.5f)
                {

                    agent.isStopped = false;

                    agent.destination = targetTransform.position;

                    //Patrolling
                    if (currentState == AIState.Patrolling)
                    {
                        Patrol();
                    } else if (currentState == AIState.Following)
                    {
                        targetTransform = controller.transform;
                    }

                    if (shootProjectiles)
                    {
                        gunModel.LookAt(Vector3.Lerp(gunModel.transform.forward, controller.transform.position, aimSpeed));
                    }

                    if (attackTimeout >= attackDelay)
                    {
                        if (shootProjectiles)
                        {
                            Debug.DrawRay(projectileSpawnPoint.position, projectileSpawnPoint.forward * 10, Color.red, 0.1f);

                            Ray ray = new Ray(projectileSpawnPoint.position, projectileSpawnPoint.forward);
                            RaycastHit hit;

                            if (Physics.Raycast(ray, out hit))
                            {
                                CreateProjectile(hit.point - projectileSpawnPoint.position, hit.point);
                            }
                            else
                            {
                                CreateProjectile(ray.GetPoint(100) - projectileSpawnPoint.position, ray.GetPoint(100));
                            }
                        }

                        attackTimeout = 0f;
                    }

                    attackTimeout += Time.deltaTime;

                }
                else
                {
                    agent.isStopped = true;
                    agent.destination = transform.position;
                }

                if (dist <= attackRadius)
                {
                    animator.SetBool("attacking", true);
                    if (!usesHurtBox) GameObject.FindObjectOfType<PlayerStatistics>().TakeDamage(damageByProximity);
                    attackTimeout = 0;
                }
                else
                {
                    animator.SetBool("attacking", false);
                }


            }


        }

        private void CreateProjectile(Vector3 directionMove, Vector3 end)
        {
            Transform t = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

            t.GetComponent<Rigidbody>().velocity = directionMove.normalized * projectileSpeed;
            t.LookAt(end);


        }

        private void OnDrawGizmos()
        {
            if (currentState != AIState.Dead)
            {

                if (currentState == AIState.Idle) Gizmos.color = Color.green;
                else if (currentState == AIState.Following) Gizmos.color = Color.red;

                Gizmos.DrawWireSphere(transform.position, attackRadius); //Radius of attack
            }
        }

    }
}