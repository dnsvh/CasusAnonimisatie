using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Range(0.01f, 0.5f)] private float smoothTime = 0.12f;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) target = p.transform;
        }
    }

    private void LateUpdate()
    {
        if (!target) return;
        var desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
