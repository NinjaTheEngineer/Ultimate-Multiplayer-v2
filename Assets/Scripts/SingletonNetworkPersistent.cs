using Unity.Netcode;
using UnityEngine;

public class SingletonNetworkPersistent<T> : NetworkBehaviour where T : Component {
    public static T Instance { get; private set; }

    public virtual void Awake() {
        if (Instance == null) {
            Instance = this as T;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }
}
