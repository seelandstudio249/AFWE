using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToUser : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start() {
        mainCameraTransform = Camera.main.transform;
    }

    void Update() {
        transform.LookAt(mainCameraTransform);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f); // Lock rotation to Y-axis
    }
}
