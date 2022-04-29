using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    private Transform mainCamera;
    // Start is called before the first frame update
    private void Start()
    {
        mainCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.rotation * Vector3.forward, //quaternion * vector3 = vector3
                        mainCamera.rotation * Vector3.up); // face camera
    }
}
