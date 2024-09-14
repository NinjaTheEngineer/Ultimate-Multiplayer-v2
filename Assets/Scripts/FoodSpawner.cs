using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    public Vector2 spawnPositionRange;
    public float spawnHeight;
    public float MaxFoodCount = 5;

    private void Start() {
        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;
    }

    private void SpawnFoodStart() {
        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart;
        for(int i = 0; i < MaxFoodCount; i++) {
            SpawnFood();
        }
        StartCoroutine(SpawnOverTime());
    }
    private Vector3 GetRandomPositionOnMap() {
        var x = UnityEngine.Random.Range(-spawnPositionRange.x, spawnPositionRange.x);
        var y = UnityEngine.Random.Range(-spawnPositionRange.y, spawnPositionRange.y);

        return new Vector3(x, spawnHeight, y);
    }

    private void SpawnFood() {
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GetRandomPositionOnMap(), Quaternion.identity);
        obj.GetComponent<Food>().prefab = prefab;
        if (!obj.IsSpawned) {
            obj.Spawn(true);
            obj.gameObject.SetActive(true);
        }
    }

    private IEnumerator SpawnOverTime() {
        while(NetworkManager.Singleton.ConnectedClients.Count > 0) {
            yield return new WaitForSeconds(2f);

            if (NetworkObjectPool.Singleton.GetCurrentPrefabCount(prefab) < MaxFoodCount) {
                SpawnFood();
            }
        }
    }
}
