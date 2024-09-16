using UnityEngine;

public class Tail : MonoBehaviour
{
    public Transform networkedOwner;
    public Transform followTransform;

    [SerializeField] float delayTime = 0.1f;
    [SerializeField] float distance = 0.3f;
    [SerializeField] float moveStep = 0.3f;
    [SerializeField] Collider _collider;
    float stopMovingDistance;

    private Vector3 _targetPosition;

    public void IgnoreCollisionWith(Collider other) {
        Physics.IgnoreCollision(_collider, other);
    }

    private void Start() {
        _collider ??= GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        stopMovingDistance = distance * 0.80f;
    }
    private void Update() {
        if (followTransform == null || (transform.position - followTransform.position).magnitude <= stopMovingDistance) {
            return;
        }
        _targetPosition = followTransform.position - followTransform.forward * distance;
        _targetPosition += (transform.position - _targetPosition) * delayTime; 
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * moveStep);
        RotateToTarget(_targetPosition);
    }

    private void RotateToTarget(Vector3 targetPosition) {
        transform.LookAt(targetPosition);
        var currentRotation = transform.rotation;
        currentRotation.x = 0f;
        currentRotation.z = 0f;
        transform.rotation = currentRotation;

    }
}
