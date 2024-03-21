using UnityEngine;

public class RotateController : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(new Vector3(45f, 30f, 20f) * Time.deltaTime);
    }
}