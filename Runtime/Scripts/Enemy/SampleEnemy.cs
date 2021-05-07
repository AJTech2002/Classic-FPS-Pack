using ClassicFPS.Controller.PlayerState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ClassicFPS.Enemy
{

    public class SampleEnemy : Enemy
    {
        [Header("Player Injury Options")]
        public float injuryRadius;
        public float injuryDelay;
        public float damageByProximity;

        private float injuryTimeout;

        [Header("Aiming")]
        public float aimSpeed = 5;

        [Header("Projectile Options")]
        public bool shootProjectiles;
        public Transform projectilePrefab;
        public Transform gunModel;
        public Transform projectileSpawnPoint;
        public float projectileSpeed;

        private void Update()
        {

            if (currentState == AIState.Following && controller != null)
            {
                float dist = Vector3.Distance(transform.position, controller.transform.position);

                if (agent.velocity.magnitude > 0.05f && dist > injuryRadius)
                {
                    animator.SetBool("walking", true);
                }
                else
                {
                    animator.SetBool("walking", false);
                }

                Vector3 lookAt = new Vector3(controller.transform.position.x, transform.position.y, controller.transform.position.z);

                transform.LookAt(Vector3.Lerp(transform.forward, lookAt, aimSpeed));

                if (dist >= injuryRadius - 0.5f)
                {

                    agent.isStopped = false;
                    agent.destination = controller.transform.position;

                    if (shootProjectiles)
                    {
                        gunModel.LookAt(Vector3.Lerp(gunModel.transform.forward, controller.transform.position, aimSpeed));
                    }

                    if (injuryTimeout >= injuryDelay)
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

                        injuryTimeout = 0f;
                    }

                    injuryTimeout += Time.deltaTime;

                }
                else
                {
                    agent.isStopped = true;
                    agent.destination = transform.position;
                }

                if (dist <= injuryRadius)
                {
                    if (injuryTimeout >= injuryDelay)
                    {
                        animator.SetBool("attacking", true);
                        GameObject.FindObjectOfType<PlayerStatistics>().TakeDamage(damageByProximity);
                        injuryTimeout = 0;
                    }

                    injuryTimeout += Time.deltaTime;
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

                Gizmos.DrawWireSphere(transform.position, injuryRadius); //Radius of injury
            }
        }
    }
}