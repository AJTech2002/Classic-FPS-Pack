using ClassicFPS.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassicFPS.Pushable
{
    public class PushableObject : MonoBehaviour
    {
        [Header("Movement Options")]
        [Tooltip("How much to scale velocity every frame after contact loss")]
        [Range(0, 100)]
        public float xAxisDrag; //Drag amount on the X Axis
        [Range(0, 100)]
        public float yAxisDrag; //Drag amount on the Y Axis
        [Range(0, 100)]
        public float zAxisDrag; //Drag amount on the Z Axis

        [Header("Safety Options")]
        [Range(0, 100)]
        public float maximumVelocity = 20; //Maximum velocity of the object after the Player interacts with it

        [Header("Pickup Options")]
        public bool canBePickedUp = false; //Whether or not this object can be picked up by the Player
        public Vector3 objectHoldingOffset = new Vector3(0.0f, 0.0f, 1.5f); //If it is to be picked up, where will it be held
        public bool precalculateBounds = true; //Whether or not the given objectHoldingOffset z is used

        [Header("SFX")]
        public Sound impactSound;
        public Sound onShootSound;
        public float waitBeforePlayingSFXAgain = 0.4f;

        private bool inPushLoop = false;

        //True when the Player is in Contact
        private bool beingPushed = true;

        //Rigidbody Reference
        private Rigidbody rigidbody;

        //Coroutine that runs only when the Player is pushing the object (this is so that every Pushable object doesn't use Update() which cab become slow)

        int frames = 0;

        IEnumerator PushLoop()
        {
            inPushLoop = true;
            frames = 0;
            //If the object is in motion
            while ((beingPushed || rigidbody.velocity.magnitude > 0.1) && frames < 100)
            {
                yield return new WaitForEndOfFrame();
                beingPushed = false;
                yield return new WaitForEndOfFrame();
                //Detect if the Player has stopped pushing
                StopMovement();
                frames += 1;
            }
            inPushLoop = false;
        }

        private void StopMovement()
        {
            //If Player has stopped pushing
            if (beingPushed == false)
            {
                //Reduce the velocity of the object over time
                rigidbody.velocity = new Vector3(rigidbody.velocity.x * (1 - xAxisDrag / 100), rigidbody.velocity.y * (1 - yAxisDrag / 100), rigidbody.velocity.z * (1 - zAxisDrag / 100));

                //Clamping the velocity of the object
                rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maximumVelocity);
                rigidbody.angularVelocity = Vector3.ClampMagnitude(rigidbody.angularVelocity, maximumVelocity);
            }
        }

        //Called by Player on Collision
        public void BeingPushed()
        {
            beingPushed = true;

            if (rigidbody == null)
                rigidbody = GetComponent<Rigidbody>();

            if (!inPushLoop)
                StartCoroutine("PushLoop");
        }

        private bool waitingSFX = false;

        IEnumerator WaitForSFX (float seconds)
        {
            waitingSFX = true;
            yield return new WaitForSeconds(seconds);
            waitingSFX = false;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (rigidbody == null)
                rigidbody = GetComponent<Rigidbody>();


            if (rigidbody.velocity.magnitude >= 0f && canBePickedUp && !waitingSFX)
            {
                SFXManager.PlayClipAt(impactSound, transform.position);
                WaitForSFX(waitBeforePlayingSFXAgain);
            }
        }

    }
}