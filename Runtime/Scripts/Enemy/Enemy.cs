using ClassicFPS.Controller.Movement;
using ClassicFPS.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* This class can be extended */
namespace ClassicFPS.Enemy
{
    [RequireComponent(typeof(SphereCollider))]
    public class Enemy : DamageableEntity
    {
        [Header("Trigger Collider")]
        public SphereCollider trigger;

        [Header("Physics Damage")]
        public float damageByThrownObjectsMultiplier = 1;

        [Header("Navigation")]
        public NavMeshAgent agent;

        [Header("Animations")]
        public Animator animator;

        [Space(10)]
        public AIState currentState;

        protected PlayerController controller;


        private void Start()
        {
            //The sphere collider should be a trigger
            trigger.isTrigger = true;
            controller = GameManager.PlayerController;

            if (controller == null)
                controller = GameObject.FindObjectOfType<PlayerController>();
        }

        public virtual void OnStart()
        {

        }

        public override void Die()
        {
            currentState = AIState.Dead;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
            }
            agent.enabled = false;
            animator.enabled = false;
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

        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.transform.CompareTag("Player"))
            {

                if (currentState == AIState.Idle)
                {
                    currentState = AIState.Following;
                }

            }
        }

        private void OnCollisionEnter(Collision col)
        {
            Rigidbody rBody = col.transform.GetComponent<Rigidbody>();

            if (rBody != null)
            {

                if (rBody.velocity.magnitude >= 20)
                {
                    float val = Mathf.Clamp((rBody.velocity.magnitude * damageByThrownObjectsMultiplier), 0f, health);
                    TakeDamage(val);
                }

            }
        }

        public enum AIState
        {
            Idle,
            Following,
            WaitingForPath,
            Dead
        }

    }
}