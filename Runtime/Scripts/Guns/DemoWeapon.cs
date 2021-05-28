using ClassicFPS.Audio;
using ClassicFPS.Enemy;
using ClassicFPS.Pushable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClassicFPS.Guns
{
    public class DemoWeapon : Weapon
    {
        [Header("Projectile Options")]
        public bool shootsProjectiles;
        public Transform projectilePrefab;
        [Tooltip("Have a Transform that marks where the Projectile starts")]
        public Transform projectileStartPosition;
        public float projectileSpeed;

        [Header("Input Actions")]
        //The input used to trigger the shooting
        public InputAction shootAction;

        [Header("Shooting Options")]
        public bool useSphereCast;
        public float sphereCastRadius;
        public LayerMask hitMask;
        public bool hasLimitedRange = false;
        public float shootDelay = 0.3f;
        public bool holdShoot = false;
        private float lastShotDelay = 0f;
        public float maxShootDistance;

        [Header("Scatter")]
        public bool scatter;
        public int scatterBulletsInEachDirection = 3;

        [Range(0f, 1f)]
        public float scatterRadius;


        [Header("Ray Impact Options")]
        public float forwardForce;
        public float maximumDamage;
        public float minimumDamage;

        [Header("Custom SFX")]
        public Sound onHitObjectSFX;


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