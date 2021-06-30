using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Enemy
{
    /* Slight variation on the normal enemy, it is lifted into the air and following the player like a drone */
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

        private float injuryTimeout;

        [Header("References")]
        public Transform geometry;
        public SphereCollider hitCollider;
        public Transform projectilePrefab;
        public Transform projectileSpawnPoint;

        Vector3 lastPlayerPos;

        private void Update()
        {
            if (currentState == AIState.Following && controller != null)
            {
                float dist = Vector3.Distance(transform.position, controller.transform.position);
                Vector3 lookAt = controller.transform.position + Vector3.up * 0.4f;

                geometry.forward = Vector3.Lerp(geometry.forward, lookAt - transform.position, aimSpeed * Time.deltaTime);

               

                hitCollider.center = transform.InverseTransformPoint(geometry.position);
                geometry.transform.position = Vector3.Lerp(geometry.transform.position, new Vector3(geometry.transform.position.x, controller.transform.position.y + 1, geometry.transform.position.z), followSpeed * Time.deltaTime);

                if (dist <= stoppingRadius)
                {
                    agent.isStopped = true;

                }
                else
                {
                    agent.isStopped = false;
                    agent.destination = controller.transform.position;

                    if (injuryTimeout >= injuryDelay)
                    {
                        //Debug.DrawRay(projectileSpawnPoint.position, projectileSpawnPoint.forward * 10, Color.red, 0.1f);

                        Ray ray = new Ray(projectileSpawnPoint.position, projectileSpawnPoint.forward);
                        RaycastHit hit;

                        //Shoot projectiles towards the Player

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

        //The Projectile has a script that handles its movement
        private void CreateProjectile(Vector3 directionMove, Vector3 end)
        {
            Transform t = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        }

        public override void Die()
        {
            currentState = AIState.Dead;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
            }

            agent.enabled = false;
            trigger.enabled = false;

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


    }
}