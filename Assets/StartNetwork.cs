using Unity.Netcode;
using UnityEngine;

public class StartNetwork : MonoBehaviour {
    public void StartServer() {
        NetworkManager.Singleton.StartServer();
        Destroy(gameObject);
    }
    public void StartHost() {
        NetworkManager.Singleton.StartHost();
        Destroy(gameObject);
    }
    public void StartClient() {
        NetworkManager.Singleton.StartClient();
        Destroy(gameObject);
    }
}
