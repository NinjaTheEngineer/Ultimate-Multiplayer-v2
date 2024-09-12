using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLength : NetworkBehaviour
{
    public NetworkVariable<ushort> _length = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private List<GameObject> _tails;
    private Transform _lastTail;
    [SerializeField] Collider _collider;
    [SerializeField] GameObject _tailPrefab;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        _tails = new List<GameObject>();
        _lastTail = transform;
        _collider ??= GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        if (!IsServer) _length.OnValueChanged += LengthChanged;
        var length = _length.Value;
        for (int i = 0; i < _length.Value; i++) {
            IncrementTail();
        }
    }

    [ContextMenu("Add Length")]
    private void AddLength() {
        _length.Value += 1;
        IncrementTail();
    }

    private void LengthChanged(ushort oldValue, ushort newValue) {
        Debug.Log("LengthChanged - Callback");
        IncrementTail();
    }

    private void IncrementTail() {
        GameObject tailGameObject = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        
        if(!tailGameObject.TryGetComponent(out Tail tail)) {
            Debug.LogError("Tail Prefab doesn't have Tail component => no-op");
            return;
        }

        tail.networkedOwner = transform;
        tail.followTransform = _lastTail;
        _lastTail = tailGameObject.transform;
        tail.IgnoreCollisionWith(_collider);


        _tails.Add(tailGameObject);
    }
}
