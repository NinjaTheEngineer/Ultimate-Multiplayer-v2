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
    bool serverRunning;
    private void Start() {
        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
    }

    private void OnServerStopped(bool isStopped) {
        Debug.Log("OnServerStopped=" + isStopped);
        serverRunning = false;
    }

    private void SpawnFoodStart() {
        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart;
        serverRunning = true;
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
        NetworkObject obj;
        try {
            obj = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GetRandomPositionOnMap(), Quaternion.identity);
        } catch(Exception e) {
            Debug.LogWarning("SpawnFood: Exception=" + e.Message);
            return;
        }
        if(obj==null) {
            Debug.LogWarning("Object is null => no-op");
            return;
        }
        obj.GetComponent<Food>().prefab = prefab;
        if (!obj.IsSpawned) {
            //Debug.Log("Spawned Food correctly!");
            obj.Spawn(true);
            obj.gameObject.SetActive(true);
        }
    }
    private IEnumerator SpawnOverTime() {
        while(serverRunning) {
            yield return new WaitForSeconds(2f);
            var foodCount = NetworkObjectPool.Singleton.GetCurrentPrefabCount(prefab);
            //Debug.Log("FoodCount=" + foodCount);
            if (foodCount < MaxFoodCount) {
                SpawnFood();
            }
        }
    }
}
