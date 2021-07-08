using System.Collections;
using System.Collections.Generic;
using ClassicFPS.Managers;
using UnityEngine;

namespace ClassicFPS.Enemy
{
    public class SampleDroneEnemy : Enemy
    {
        [Header("Aiming")]
        public float aimSpeed = 5;

        [Header("Follow")]
        public float followSpeed = 2;
        public float stoppingRadius = 5;

        [Header("Shooting")]
        public float injuryDelay;
        public float projectileSpeed;
        [SerializeField] ParticleSystem shootParticles;

        private float injuryTimeout;

        [Header("References")]
        public Transform geometry;
        public SphereCollider hitCollider;
        public Transform projectilePrefab;
        public Transform projectileSpawnPoint;

        [Header("Flying")]
        [SerializeField] Vector3 flyOffset;

        Vector3 lastPlayerPos;

        [Header("Hearing")]
        [SerializeField] float hearingPower = 5;
        Vector3 boxColliderCenterOrig;
        Vector3 boxColliderSizeOrig;
        [SerializeField] BoxCollider boxCollider;

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

            if ((currentState == AIState.Following || currentState == AIState.Patrolling) && controller != null) {
                float dist = Vector3.Distance(transform.position, controller.transform.position);
                animator.SetBool("walking", true);
                agent.baseOffset = flyOffset.y;

                //Vector3 lookAt = new Vector3(targetTransform.position.x, transform.position.y, targetTransform.transform.position.z);

                //transform.LookAt(Vector3.Lerp(transform.forward, lookAt, aimSpeed));

                Vector3 dir = targetTransform.position - transform.position + transform.forward;
                dir.y = 0;//This allows the object to only rotate on its y axis
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, aimSpeed * Time.deltaTime);

                agent.destination = targetTransform.position;

                //Patrolling
                if (currentState == AIState.Patrolling)
                {
                    Patrol();
                } else if (currentState == AIState.Following)
                {
                    targetTransform = controller.transform;
                    Shoot();
                }
            }
        }

        private void CreateProjectile(Vector3 directionMove, Vector3 end)
        {
            Transform t = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            shootParticles.Emit(7);
        }

        public override void Die()
        {
            currentState = AIState.Dead;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
            }

            agent.enabled = false;
            //trigger.enabled = false;

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

        public void Shoot()
        {
            if (injuryTimeout >= injuryDelay)
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

                injuryTimeout = 0f;
            }

            injuryTimeout += Time.deltaTime;
        }
    }
}