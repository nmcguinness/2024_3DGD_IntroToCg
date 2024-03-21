using UnityEngine;

public class RotateBehaviour : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotateAxis;

    [SerializeField]
    private Space relativeToSpace;

    private void Update()
    {
        transform.Rotate(rotateAxis, relativeToSpace);
    }
}