using System;
using UnityEngine;

public class Tail : MonoBehaviour
{
    public Transform networkedOwner;
    public Transform followTransform;

    [SerializeField] float delayTime = 0.1f;
    [SerializeField] float distance = 0.3f;
    [SerializeField] float moveStep = 0.3f;
    [SerializeField] Collider _collider;

    private Vector3 _targetPosition;

    public void IgnoreCollisionWith(Collider other) {
        Physics.IgnoreCollision(_collider, other);
    }

    private void Start() {
        _collider ??= GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
    }
    private void Update() {
        if (followTransform == null) {
            Destroy(gameObject);
            return;
        }
        _targetPosition = followTransform.position - followTransform.forward * distance;
        _targetPosition += (transform.position - _targetPosition) * delayTime;
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * moveStep);
        transform.LookAt(_targetPosition);
    }
}
