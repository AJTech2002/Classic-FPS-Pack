using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ClassicFPS.Enemy;


namespace ClassicFPS.Breakable
{
    public class BreakableObject : DamageableEntity
    {
        [Header("Damage Options")]
        public float damageVelocityMultiplier = 10;
        public float minimumVelocityForImpact = 10;

        [HideInInspector]
        public bool thrown;

        private Rigidbody rBody;

        //If a BreakableObject is thrown with force it will collide and break (or at least take damage)
        private void OnCollisionEnter(Collision col)
        {
            if (rBody == null) rBody = GetComponent<Rigidbody>();

            if (rBody != null)
            {
                if (rBody.velocity.magnitude >= minimumVelocityForImpact && !thrown)
                {
                    TakeDamage(rBody.velocity.magnitude * damageVelocityMultiplier, 0f);
                }

                //If the object is thrown then you know the player is intending on breaking it
                if (thrown)
                {
                    TakeDamage(10 * damageVelocityMultiplier, 0f);
                }

            }

            thrown = false;
        }

    }
}
