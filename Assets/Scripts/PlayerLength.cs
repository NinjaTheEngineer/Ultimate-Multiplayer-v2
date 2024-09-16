using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLength : NetworkBehaviour
{
    public NetworkVariable<ushort> length = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [CanBeNull] public static event System.Action<ushort> ChangedLengthEvent;

    private List<GameObject> _tails;
    private Transform _lastTail;
    [SerializeField] Collider _collider;
    [SerializeField] GameObject _tailPrefab;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        _tails = new List<GameObject>();
        _lastTail = transform;
        _collider ??= GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        if (!IsServer) this.length.OnValueChanged += LengthChangedEvent;
        var length = this.length.Value;
        for (int i = 0; i < this.length.Value; i++) {
            IncrementTail();
        }
    }

    public void AddLength() {
        length.Value += 1;
        LengthChanged(); //For host
    }

    private void LengthChangedEvent(ushort oldValue, ushort newValue) {
        Debug.Log("LengthChanged - Callback");
        LengthChanged();
    }
    private void LengthChanged() {
        IncrementTail();

        if (!IsOwner) return;
        ChangedLengthEvent?.Invoke(length.Value);
        ClientMusicPlayer.Instance.PlayPickUpAudioClip();
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

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        DestroyTails();
    }
    private void DestroyTails() {
        while(_tails.Count != 0) {
            GameObject tail = _tails[0];
            _tails.RemoveAt(0);
            Destroy(tail);
        }
    }
}