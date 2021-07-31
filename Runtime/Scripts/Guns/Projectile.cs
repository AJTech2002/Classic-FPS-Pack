using ClassicFPS.Audio;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Controller.PlayerState;
using ClassicFPS.Enemy;
using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClassicFPS.Guns
{
    /* An example projectile script that can be used to follow the player, or even explode halfway */

    public class Projectile : MonoBehaviour
    {
        [Header("Ammunition Properties")]
        public bool canHarmPlayer = false; //Can it harm the player
        public bool canHarmEnemies = true; //can it harm the enemy

        [Header("Impact Properties")]
        public bool doesExplode = true; //Should it explode
        public float explosionForce; //The force of the explosion
        public float upwardsForce; //how far up do you want the force to be
        public float explosionRadius; //radius of objects affected
        public ParticleSystem explosionParticles; //Particles to run during this 

        [Header("Physics")]
        public float minimumImpactVelocity = 15;

        [Header("General")]
        public float damage;

        [Header("Delay")]
        public bool stickToImpact; 
        public float timeToImpact;

        [Header("Homing")]
        public bool isHomingMissile = false; //Whether or not we want it to follow the Player
        public float destroyAfter = 10; //Destroy the Projectile after a max time
        public float moveSpeed; //Move speed of the Projectile

        [Header("Sounds")]
        public Sound explosionSound; //Sound of the explosion

        private float timer = 0f;

        private Rigidbody rBody;
        private PlayerController controller;

        private void Awake()
        {
            rBody = GameObject.FindObjectOfType<Rigidbody>();

            controller = GameManager.PlayerController;

            if (gameObject != null)
            {
                GameObject.Destroy(this.gameObject, destroyAfter);
            }
            
            //Ensuring all the Colliders are enabled
            if (GetComponentInChildren<Collider>() != null)
                if (stickToImpact && GetComponentInChildren<Collider>().isTrigger == false)
                {
                    Debug.LogError("If you want a projectile to stick make the Colliders a trigger");
                }

            if (GetComponent<Collider>() != null)
                if (stickToImpact && GetComponent<Collider>().isTrigger == false)
                {
                    Debug.LogError("If you want a projectile to stick make the Colliders a trigger");
                }
        }

        private void Update()
        {
            if (isHomingMissile)
            {
                //Follow the player
                rBody.velocity = (controller.transform.position - transform.position).normalized * moveSpeed;
                transform.LookAt(controller.transform.position);
                timer += Time.deltaTime;

                if (timer > destroyAfter - 0.1f)
                {
                    Impact(true, false);
                }

            }

            //If the speed of the projectile drops below a certain speed then kill it
            if (rBody.velocity.magnitude <= 2 && !stickToImpact)
            {
                GameObject.Destroy(this.gameObject, 0f);
            }
        }

        //What to do when the Projectile hits something or the ground
        private void Impact()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            
            //Get all nearby objects and handle based on type
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                DamageableEntity de = hit.GetComponent<DamageableEntity>();

                //if (doesExplode) SFXManager.PlayClipAt(explosionSound, transform.position, 0.2f);
                if (doesExplode) {
                    explosionSound.PlayAt(GameObject.Find("Controller").transform.position, 0.2f);
                    if (explosionParticles != null) {
                        explosionParticles.gameObject.SetActive(true);
                        explosionParticles.transform.parent = null;
                        Destroy(explosionParticles.gameObject, 5f);
                    }
                }

                if (rb != null)
                {
                    rb.velocity += (rb.transform.position - transform.position).normalized * minimumImpactVelocity;
                }

                float r = 1 - (Vector3.Distance(hit.transform.position, transform.position) / explosionRadius);

                if (de != null && canHarmEnemies)
                {

                    de.TakeDamage(r * damage);

                }

            }

            GameObject.Destroy(this.gameObject, 0.0f);
        }

        //An impact function where player = whether or not Player gets hurt, and same for enemy
        public void Impact(bool player, bool enemy)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in colliders)
            {
                //Get the properties of the colliders nearby
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                DamageableEntity de = hit.GetComponent<DamageableEntity>();
                PlayerStatistics stat = hit.GetComponent<PlayerStatistics>();

                if (doesExplode) explosionSound.PlayAt(transform.position,0.2f); 

                //Rigidbody Handle
                if (rb != null)
                {
                    if (doesExplode)
                    {

                        rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsForce);
                    }
                    rb.velocity += (rb.transform.position - transform.position).normalized * minimumImpactVelocity;
                }

                float r = Mathf.Clamp01(1 - (Vector3.Distance(hit.transform.position, transform.position) / explosionRadius));

                //DamageableEntity handle
                if (de != null && enemy)
                {

                    de.TakeDamage(r * damage);

                }

                //Player handle
                if (stat != null && player)
                {
                    stat.TakeDamage(r * damage);
                }


            }

            GameObject.Destroy(this.gameObject, 0.0f);
        }


        private void ImpactPlayer()
        {
            GameObject.FindObjectOfType<PlayerStatistics>().TakeDamage(damage);
            GameObject.Destroy(this.gameObject, 0.0f);
        }

        IEnumerator Stick()
        {

            yield return new WaitForSeconds(timeToImpact);
            Impact();
        }

        private void OnCollisionEnter(Collision col)
        {

            if (!isHomingMissile)
            {
                if (!stickToImpact && col.transform.CompareTag("Player") == false)
                {

                    Impact();
                }

                if (canHarmPlayer && col.transform.CompareTag("Player") == true)
                {
                    ImpactPlayer();
                }
            }
            else
            {
                if (!stickToImpact && col.transform.CompareTag("Enemy") == true && timer >= 1)
                {
                    Impact(false, true);
                }
                else if (canHarmPlayer && col.transform.CompareTag("Player") == true)
                {
                    Impact(true, false);
                }
                else
                {
                    Impact(false, false);
                }
            }

        }

        private void OnTriggerEnter(Collider col)
        {
            if (stickToImpact)
            {
                this.GetComponent<Rigidbody>().isKinematic = true;
                StartCoroutine("Stick");
            }
        }

    }

}