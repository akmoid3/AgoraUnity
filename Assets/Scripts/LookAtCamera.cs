using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 up = transform.up; // Keep the surface alignment
        Vector3 toCamera = (Camera.main.transform.position - transform.position).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(toCamera, up).normalized;

        transform.rotation = Quaternion.LookRotation(forward, up);
    }
}

