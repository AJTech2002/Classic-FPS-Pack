using UnityEngine;
using System.Collections;
using ClassicFPS.Enemy;

public class Ragdoll : MonoBehaviour
{
    Collider[] rigColliders;
    Rigidbody[] rigRigidbodies;
    [SerializeField] bool disableCollidersToo;
    [SerializeField] bool isRagdoll = false;
    Enemy enemy;

    private void Awake()
    {
        if(GetComponent<Enemy>()) enemy = GetComponent<Enemy>();

        rigColliders = GetComponentsInChildren<Collider>();
        rigRigidbodies = GetComponentsInChildren<Rigidbody>();
        EnableRagdoll(isRagdoll);

    }

    public void EnableRagdoll(bool disable)
    {
        //wait 2-3 seconds.
        if (disableCollidersToo)
        {
            foreach (Collider col in rigColliders)
            {
                col.enabled = disable;
            }
        }

        foreach (Rigidbody rb in rigRigidbodies)
        {
            rb.isKinematic = !disable;
            enemy.animator.enabled = !disable;
        }
    }
}