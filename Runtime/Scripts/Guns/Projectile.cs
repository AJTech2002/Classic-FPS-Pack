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
    public class Projectile : MonoBehaviour
    {
        [Header("Ammunition Properties")]
        public bool canHarmPlayer = false;
        public bool canHarmEnemies = true;

        [Header("Impact Properties")]
        public bool doesExplode = true;
        public float explosionForce;
        public float upwardsForce;
        public float explosionRadius;
        public ParticleSystem explosionParticles;
        public ParticleSystem expireParticles;

        [Header("Physics")]
        public float minimumImpactVelocity = 15;

        [Header("General")]
        public float damage;

        [Header("Delay")]
        public bool stickToImpact;
        public float timeToImpact;
        [SerializeField] float delayActiveCollider = 0f;
        [SerializeField] Collider collider;


        [Header("Homing")]
        public bool isHomingMissile = false;
        public float destroyAfter = 10;
        public bool explodesAfterTimer = false; //Does the projectile explode after the destroyAfter timer?
        public float moveSpeed;
        [SerializeField] Vector3 targetOffset;

        [Header("Sounds")]
        public Sound explosionSound;

        private float timer = 0f;

        private Rigidbody rBody;
        private PlayerController controller;

        private void Awake()
        {
            if(delayActiveCollider > 0)
            {
                if (!collider) collider = GetComponentInChildren<Collider>();
                collider.enabled = false;
                StartCoroutine(ActivateCollider());
            }

            rBody = GameObject.FindObjectOfType<Rigidbody>();

            controller = GameManager.PlayerController;
            

            if (gameObject != null)
            {
                GameObject.Destroy(this.gameObject, destroyAfter);
            }

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
                Vector3 targetPosition = new Vector3(controller.transform.position.x, controller.transform.position.y + targetOffset.y, controller.transform.position.z);
                rBody.velocity = (targetPosition - transform.position).normalized * moveSpeed;
                transform.LookAt(targetPosition);
                timer += Time.deltaTime;

                if (timer > destroyAfter - 0.1f)
                {
                    if (explodesAfterTimer) { 
                        Impact(true, false);
                    }
                    else
                    {
                        expireParticles.transform.parent = null;
                        expireParticles.gameObject.SetActive(true);
                        Destroy(expireParticles, 3f);
                        GameObject.Destroy(this.gameObject, 0f);

                    }
                }

            }

            //If the speed of the projectile drops below a certain speed then kill it
            if (rBody.velocity.magnitude <= 2 && !stickToImpact)
            {
                GameObject.Destroy(this.gameObject, 0f);
            }
        }

        private void Impact()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                DamageableEntity de = hit.GetComponent<DamageableEntity>();

                //if (doesExplode) SFXManager.PlayClipAt(explosionSound, transform.position, 0.2f);
                if (doesExplode) {
                    ExplosionEffect();
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

        public void Impact(bool player, bool enemy)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                DamageableEntity de = hit.GetComponent<DamageableEntity>();
                PlayerStatistics stat = hit.GetComponent<PlayerStatistics>();

                if (doesExplode) ExplosionEffect();

                if (rb != null)
                {
                    if (doesExplode)
                    {

                        rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsForce);

                    }
                    rb.velocity += (rb.transform.position - transform.position).normalized * minimumImpactVelocity;
                }

                float r = Mathf.Clamp01(1 - (Vector3.Distance(hit.transform.position, transform.position) / explosionRadius));


                if (de != null && enemy)
                {

                    de.TakeDamage(r * damage);

                }

                if (stat != null && player)
                {
                    stat.TakeDamage(r * damage);
                }


            }

            GameObject.Destroy(this.gameObject, 0.0f);
        }

        private void ExplosionEffect()
        {
            SFXManager.PlayClipAt(explosionSound, GameObject.Find("Controller").transform.position, .6f);
            explosionParticles.gameObject.SetActive(true);
            explosionParticles.transform.parent = null;
            Destroy(explosionParticles.gameObject, 5f);
            Debug.Log("Explode!" + gameObject.name);
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
                Debug.Log("Collider with player");
                if (!stickToImpact && col.transform.CompareTag("Enemy") == true && timer >= 1)
                {
                    Impact(false, true);
                }
                else if (canHarmPlayer && col.transform.CompareTag("Player") == true)
                {
                    Impact(true, false);
                    Debug.Log("Hit the palyer!");
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

        IEnumerator ActivateCollider()
        {
            yield return new WaitForSeconds(delayActiveCollider);
            collider.enabled = true;
        }


    }

}