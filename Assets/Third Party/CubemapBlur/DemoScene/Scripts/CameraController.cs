using UnityEngine;

public class CameraController : MonoBehaviour
{
    void Update()
    {
        transform.position = Quaternion.AngleAxis(Time.time * 20f, Vector3.up) * new Vector3(0f, 1f, -4f);
        transform.LookAt(Vector3.zero, Vector3.up);
    }
}