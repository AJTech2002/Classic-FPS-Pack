using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple MonoBehaviour class that prevents an object from dying when the level changes
namespace ClassicFPS.Controller.PlayerState
{ 
    public class DontDestroyGameObject : MonoBehaviour
    {
        [Header("Options")]
        public string _tag;
        public bool dontDestroyOnLoad = true;


        private void Awake()
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(_tag);

            foreach (GameObject go in objs)
            {
                if (go != this.gameObject)
                    Destroy(go);
            }
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
        }
    }
}