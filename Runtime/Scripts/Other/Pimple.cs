using System.Collections;
using System.Collections.Generic;
using ClassicFPS.Managers;
using UnityEngine;

public class Pimple : MonoBehaviour
{

    [SerializeField] Animator animator;
    [SerializeField] float bounceForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.transform.CompareTag("Player") == true)
        {
            animator.SetTrigger("bounce");
            GameManager.PlayerController.SetVerticalForce(bounceForce);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.transform.CompareTag("Player") == true)
        {
            animator.ResetTrigger("bounce");
        }
    }
}
