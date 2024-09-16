using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour {
    [SerializeField] bool serverMovement = false;
    private Camera _mainCamera;
    private Vector3 _mouseInput;
    public LayerMask validLayers;
    public float speed = 5f;
    private Vector3 _targetPosition;
    private bool _hasTargetPosition = false;
    private PlayerLength _playerLength;
    private Rigidbody _rigidbody;

    private readonly ulong[] _targetClientsArray = new ulong[1];

    [CanBeNull] public static event System.Action GameOverEvent;
    private void Initialize() {
        _mainCamera = Camera.main; 
        _playerLength = GetComponent<PlayerLength>();
        _rigidbody = GetComponent<Rigidbody>();
        lastPlayerCollisionTime = Time.realtimeSinceStartup;
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        Debug.Log("NetworkSpawn");
        Initialize();
    }
    void Update() {
        // Move only if the application is focused and right mouse button is pressed
        if (!Application.isFocused || !IsOwner) return;

        if(serverMovement) {
            MovePlayerServer();
        } else {
            MoveToCursor();
        }
    }
    private void MovePlayerServer() {
        // Move only if right mouse button is pressed
        if (!Input.GetMouseButton(1)) return;

        // Cast a ray from the mouse position into the world
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform raycast with layer mask, hit only valid layers
        if (Physics.Raycast(ray, out hit, 100f, validLayers)) {
            // Get the world coordinates from the raycast hit
            Vector3 targetPosition = hit.point;
            MovePlayerServerRpc(targetPosition);
        }
    }
    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 mouseWorldCoordiantes) {
        // Move the player towards the target position
        transform.position = Vector3.MoveTowards(transform.position, mouseWorldCoordiantes, Time.deltaTime * speed);

        // Rotate the player to face the target direction
        if (mouseWorldCoordiantes != transform.position) {
            Vector3 targetDirection = mouseWorldCoordiantes - transform.position;
            transform.rotation = Quaternion.LookRotation(targetDirection); 
        }
    } 
    private void MoveToCursor() {
        // Move only if right mouse button is pressed
        if (!Input.GetMouseButton(1)) return;

        // Cast a ray from the mouse position into the world
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform raycast with layer mask, hit only valid layers
        if (Physics.Raycast(ray, out hit, 100f, validLayers)) {
            // Get the world coordinates from the raycast hit
            Vector3 targetPosition = hit.point;

            // Move the player towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

            // Rotate the player to face the target direction
            if (targetPosition != transform.position) {
                Vector3 targetDirection = targetPosition - transform.position;
                transform.rotation = Quaternion.LookRotation(targetDirection); // Rotate in the direction of movement
                //Debug.Log("Target Position= " + targetPosition + " Target Direction=" + targetDirection);
                _rigidbody.velocity = Vector3.zero;
            }
        }
    }

    private void DetermineWinner(PlayerData player1, PlayerData player2) {
        Debug.Log("DetermineWinner isServerRPC=" + !serverMovement);
        if(serverMovement) {
            DetermineCollisionWinner(player1, player2);
        } else {
            DetermineCollisionWinnerServerRpc(player1, player2);
        }
    }
    private void DetermineCollisionWinner(PlayerData player1, PlayerData player2) {
        Debug.Log("DetermineWinner Player1 Length=" + player1.length + " Player2 Length=" + player2.length);
        if (player1.length > player2.length) {
            Debug.Log("Winner Player1");
            WinInformation(player1.id, player2.id);
        } else {
            Debug.Log("Winner Player2");
            WinInformation(player2.id, player1.id);
        }
        Debug.Log("Winner determined");
    }
    [ServerRpc]
    private void DetermineCollisionWinnerServerRpc(PlayerData player1, PlayerData player2) {
        DetermineCollisionWinner(player1, player2);
    }
    private void WinInformation(ulong winner, ulong loser) {
        Debug.Log("WinInformationServerRpc");
        _targetClientsArray[0] = winner;
        ClientRpcParams clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = _targetClientsArray
            }
        };

        AtePlayerClientRpc(clientRpcParams);

        _targetClientsArray[0] = loser;
        clientRpcParams.Send.TargetClientIds = _targetClientsArray;
        GameOverClientRpc(clientRpcParams);
        Debug.Log("Winner=" + winner + " Loser=" + loser + " => ClientRpcs sent");
    }

    [ClientRpc]
    private void AtePlayerClientRpc(ClientRpcParams clientRpcParams = default) {
        Debug.Log("AtePlayerClientRpc! IsOwner="+IsOwner+ " OwnerClientId=" + OwnerClientId);
        if (!IsOwner) return;
        Debug.Log("You ate a Player!");
    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default) {
        Debug.Log("GameOverClientRpc! IsOwner=" + IsOwner + " OwnerClientId=" + OwnerClientId);
        if (!IsOwner) return;
        Debug.Log("You lose!");
        GameOverEvent?.Invoke();
        NetworkManager.Singleton.Shutdown();
    }
    float lastPlayerCollisionTime = 0f;
    private void OnCollisionEnter(Collision collision) {
        var collisionGO = collision.gameObject;
        
        if (!serverMovement && !IsOwner) return;
        
        if (!collisionGO.CompareTag("Player")) return;
        
        Debug.Log("OnCollisionEnter with " + collision.gameObject);
        
        if (collisionGO.TryGetComponent(out PlayerLength playerLength)) {
            Debug.Log("Head on Collision! OwnerClientId="+OwnerClientId);
            var player1 = new PlayerData() {
                id = OwnerClientId,
                length = _playerLength.length.Value
            };

            var player2 = new PlayerData() {
                id = playerLength.OwnerClientId,
                length = playerLength.length.Value
            };

            if (Time.realtimeSinceStartup - lastPlayerCollisionTime > 1f) {
                Debug.Log("Player Collision with player" + collisionGO + " myId="+OwnerClientId+" otherPlayer="+ playerLength.OwnerClientId);
                lastPlayerCollisionTime = Time.realtimeSinceStartup;
                DetermineWinner(player1, player2);
            }
        } else if (collisionGO.TryGetComponent(out Tail tail)) {
            Debug.Log("Tail Collision");
            //WinInformationServerRpc(tail.networkedOwner.GetComponent<PlayerController>().OwnerClientId, OwnerClientId);
        } else {
            Debug.LogWarning("Failed to find valid component");
        }
    }
}
struct PlayerData : INetworkSerializable {
    public ulong id;
    public ushort length;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref length);
    }
}

