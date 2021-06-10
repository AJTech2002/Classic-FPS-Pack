using ClassicFPS.Controller.PlayerState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ClassicFPS.Enemy
{

    public class TestEnemy : MonoBehaviour
    {
        public NavMeshAgent agent;
        public Transform targetTransform;

        private void Update()
        {
            agent.destination = targetTransform.position;
        }
    }
}