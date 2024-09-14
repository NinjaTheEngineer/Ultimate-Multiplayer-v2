using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{
    public GameObject prefab;
    private void OnTriggerEnter(Collider other) {
        
        if (!NetworkManager.Singleton.IsServer) return;

        if (!other.CompareTag("Player")) return;
        
        if(other.TryGetComponent(out PlayerLength playerLength)) {
            playerLength.AddLength();
        }
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        NetworkObject.Despawn();
    }
}
