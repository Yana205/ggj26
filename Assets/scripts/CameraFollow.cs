using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothTime = 5f;

    float zPosition;

    void Start()
    {
        zPosition = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, zPosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothTime * Time.deltaTime);
    }
}
