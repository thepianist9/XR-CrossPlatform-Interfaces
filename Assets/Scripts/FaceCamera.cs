using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Public toggles
    public bool lookAtCamera = true; // If true, the object looks at the camera


    void Update()
    {
        // Reference to the main camera
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found. Ensure there is a camera tagged as 'MainCamera'.");
            return;
        }

        // Look at the camera
        if (lookAtCamera)
        {
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0);
        }
    }
}
