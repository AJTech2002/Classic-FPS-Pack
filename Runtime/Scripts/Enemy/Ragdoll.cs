using UnityEngine;
using System.Collections;


public class Ragdoll : MonoBehaviour
{
    Collider[] rigColliders;
    Rigidbody[] rigRigidbodies;
    [SerializeField] bool disableCollidersToo;

    private void Awake()
    {
        rigColliders = GetComponentsInChildren<Collider>();
        rigRigidbodies = GetComponentsInChildren<Rigidbody>();
        EnableRagdoll(false);
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
        }
    }
}