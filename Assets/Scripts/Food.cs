using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{
    public GameObject prefab;

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        Debug.Log("OnNetworkDespawn", this);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        
        if (!NetworkManager.Singleton.IsServer) return;

        if (!other.CompareTag("Player")) return;
        
        if(other.TryGetComponent(out PlayerLength playerLength)) {
            playerLength.AddLength();
        }
        NetworkObject.Despawn();
    }

    private void OnEnable() {
        //Debug.Log("OnEnable");
    }
    private void OnDisable() {
        //Debug.Log("OnDisable");
    }
}
