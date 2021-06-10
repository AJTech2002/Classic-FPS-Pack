using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClassicFPS.Controller.Movement;
using ClassicFPS.Managers;

public class LookAtObject : MonoBehaviour
{
    public Transform target;
    public float damping;
    private Quaternion initialRotation;
    [SerializeField] GameObject parentObject;

    private void Start()
    {
        initialRotation = transform.rotation;
        if (parentObject == null) parentObject = transform.parent.gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        initialRotation = parentObject.transform.rotation;
        Vector3 lookPos = parentObject.transform.forward;

        if (target)
        {
          lookPos = target.position - parentObject.transform.position;
        }
        
        lookPos.y = 0;
        float angle = Vector3.SignedAngle(initialRotation * Vector3.forward, lookPos, Vector3.up);
        Quaternion rotation = initialRotation * Quaternion.AngleAxis(Mathf.Clamp(angle, -90, 90), Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);

    }
}
