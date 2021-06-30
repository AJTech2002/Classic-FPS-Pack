using ClassicFPS.Audio;
using ClassicFPS.Enemy;
using ClassicFPS.Pushable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClassicFPS.Guns
{
     /* A Demo Weapon class which can be repurposed into a Shotgun, Rifle, Pistol, Knife etc. */
    public class DemoWeapon : Weapon
    {
        [Header("Projectile Options")]
        public bool shootsProjectiles; //Whether or not this is based on rays or projectiles
        public Transform projectilePrefab; //What projectile to use if it is projectile based 
        [Tooltip("Have a Transform that marks where the Projectile starts")]
        public Transform projectileStartPosition; //Where to shoot projectiles from
        public float projectileSpeed; //Speed to shoot projectile

        [Header("Input Actions")]
        //The input used to trigger the shooting
        public InputAction shootAction;

        [Header("Shooting Options")]
        public bool useSphereCast; //Whether or not a sphere cast should be used
        public float sphereCastRadius; //Radius of sphere case
        public LayerMask hitMask; //The layermask of what can be hit with this
        public bool hasLimitedRange = false; //Whether or not there is a range to this weapon
        public float maxShootDistance; //How far can you shoot 
        public float shootDelay = 0.3f; //The delay between each bullet/attack
        public bool holdShoot = false; //Can you hold while shooting this weapon or do you have to release each time
        private float lastShotDelay = 0f;
        

        [Header("Scatter")]
        public bool scatter; //Is there a scatter effect of bullets (random like shotgun)
        public int scatterBulletsInEachDirection = 3; //How many scatter bullets

        [Range(0f, 1f)]
        public float scatterRadius; //How far apart the scatter should be


        [Header("Ray Impact Options")]
        public float forwardForce; //The physics effect of the bullets on rigidbodies
        public float maximumDamage; //Maximum damage when the distance is close
        public float minimumDamage; //Minimum damage when the distance is far

        [Header("Custom SFX")]
        public Sound onHitObjectSFX; //What to play when the bullet hits an object


        private bool holding = false; 

        private List<Transform> itemsHit = new List<Transform>();

        //Pistol shooting will be Ray based
        public void Attack(Camera camera, Vector2 crosshairPosition)
        {

            WeaponState tempState = GetState();

            itemsHit.Clear();

            //Check if the Gun has ammo
            if ((tempState.ammoRemaining > 0 || !requiresAmmo) && lastShotDelay > shootDelay)
            {
                lastShotDelay = 0f;

                //Run the animation
                RunShootAnimation();

                //Reduce ammo

                if (requiresAmmo)
                    tempState.ammoRemaining -= 1;

                SetState(tempState);

                Vector3 direction = Vector3.zero;

                if (!scatter) SingleShot(camera, crosshairPosition, Vector3.zero);
                else
                {
                    SingleShot(camera, crosshairPosition, Vector3.zero);
                    for (int i = 0; i < scatterBulletsInEachDirection; i++)
                    {
                        SingleShot(camera, crosshairPosition, ((camera.transform.right * scatterRadius) / scatterBulletsInEachDirection) * i);
                        SingleShot(camera, crosshairPosition, ((-camera.transform.right * scatterRadius) / scatterBulletsInEachDirection) * i);
                    }
                }

            }
            else if (tempState.ammoRemaining <= 0)
                RunNoAmmoAnimation();

        }

        //Shooting Ammendments
        private void SingleShot(Camera camera, Vector2 crosshairPosition, Vector3 addedDirection)
        {
            //Create a Ray
            RaycastHit hit;
            Ray ray = camera.ViewportPointToRay(crosshairPosition);
            ray.direction += addedDirection;

            Debug.DrawRay(weaponController.weaponMount.position, ray.direction * maxShootDistance, Color.black, 0.1f);

            if (!useSphereCast)
            {

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitMask))
                {
                    ShotRay(ray, hit);
                }
                else
                {
                    if (shootsProjectiles)
                    {
                        CreateProjectile(ray.GetPoint(100) - projectileStartPosition.position, ray.GetPoint(100));
                    }
                }
            }
            else
            {

                if (Physics.SphereCast(ray, sphereCastRadius, out hit, Mathf.Infinity, hitMask))
                {
                    ShotRay(ray, hit);
                }
                else
                {
                    if (shootsProjectiles)
                    {
                        CreateProjectile(ray.GetPoint(100) - projectileStartPosition.position, ray.GetPoint(100));
                    }
                }
            }

        }

        private void ShotRay(Ray ray, RaycastHit hit)
        {
            if (!itemsHit.Contains(hit.transform) && !shootsProjectiles)
            {
                float percImpact = 1 - (hit.distance / maxShootDistance);

                if (!hasLimitedRange)
                    percImpact = Mathf.Clamp(percImpact, 0.01f, 1f);
                else
                    percImpact = Mathf.Clamp01(percImpact);

                if (percImpact > 0f)
                    Debug.DrawLine(weaponController.weaponMount.position, hit.point, Color.Lerp(Color.black, Color.red, percImpact), 0.1f);

                //If there is a Rigidbody then Apply a Velocity to it
                if (hit.transform.GetComponent<Rigidbody>() != null)
                {
                    hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(weaponController.weaponMount.forward * forwardForce * percImpact, hit.point, ForceMode.Impulse);

                }

                PushableObject pushableObj = hit.transform.GetComponent<PushableObject>();

                if (hit.transform.CompareTag("Enemy") == false && percImpact > 0.2f && (pushableObj == null || ((pushableObj != null && pushableObj.onShootSound.clipName == ""))))
                {
                    //Run a SFX for just object hits

                    SFXManager.PlayClipAt(onHitObjectSFX, hit.point, 0.5f, 0.1f);
                }

                if (pushableObj != null && pushableObj.onShootSound.clipName != "")
                {

                    SFXManager.PlayClipAt(pushableObj.onShootSound, hit.point, 1f);
                }


                //If it hits an Enemy or Breakable Object 
                if (hit.transform.GetComponent<DamageableEntity>() != null)
                {
                    hit.transform.GetComponent<DamageableEntity>().TakeDamage(Mathf.Clamp(maximumDamage * percImpact, minimumDamage, maximumDamage));
                }

                Projectile proj = hit.transform.GetComponent<Projectile>();

                if (proj != null)
                {
                    if (proj.isHomingMissile)
                    {
                        //Blow up Missile in air if shot
                        proj.Impact(true, true);
                    }
                }

                itemsHit.Add(hit.transform);
            }

            if (shootsProjectiles)
            {
                Debug.DrawLine(weaponController.weaponMount.position, hit.point, Color.magenta, 0.5f);
                CreateProjectile(hit.point - projectileStartPosition.position, hit.point);
            }
        }

        private void CreateProjectile(Vector3 directionMove, Vector3 end)
        {
            Transform t = Instantiate(projectilePrefab, projectileStartPosition.position, Quaternion.identity);

            t.GetComponent<Rigidbody>().velocity = directionMove.normalized * projectileSpeed;
            t.LookAt(end);


        }

        private void Update()
        {
            lastShotDelay += Time.deltaTime;

            if (holdShoot && holding)
            {
                if (lastShotDelay > shootDelay)
                {
                    Attack(weaponController._camera, new Vector2(0.5f, 0.5f));
                }
            }

            HandlePlayerAnimate();

        }

        //Awake method for the guns
        public override void OnGunEquipped()
        {
            lastShotDelay = shootDelay + 0.01f;
            //Link the Input
            shootAction.performed += ShootAction_performed;
            shootAction.canceled += ShootAction_canceled;
            shootAction.Enable();
        }

        private void ShootAction_canceled(InputAction.CallbackContext obj)
        {
            if (holdShoot)
                weaponAnimatorController.SetBool("holding", false);
            holding = false;
        }

        private void ShootAction_performed(InputAction.CallbackContext obj)
        {
            if (holdShoot)
                weaponAnimatorController.SetBool("holding",true);

            WeaponState tempState = GetState();
            if (tempState.ammoRemaining <= 0 && requiresAmmo)
            {
                if (weaponSoundsSource != null)
                    SFXManager.PlayClipFromSource(emptyAmmoSound, this.weaponSoundsSource);
            }

            Attack(weaponController._camera, new Vector2(0.5f, 0.5f));
            holding = true;
        }

        public override void OnGunUnequipped()
        {
            //Unlink the Input
            shootAction.performed -= ShootAction_performed;
            shootAction.canceled -= ShootAction_canceled;
            shootAction.Disable();
        }

        void OnDestroy()
        {
            OnGunUnequipped();
        }

    }

}