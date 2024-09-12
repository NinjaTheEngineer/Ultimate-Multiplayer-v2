using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : NetworkBehaviour
{
    private Camera _mainCamera;
    private Vector3 _mouseInput;
    public LayerMask validLayers; 
    public float speed = 5f;
    private Vector3 _targetPosition;
    private bool _hasTargetPosition = false;
    private void Initialize() {
        _mainCamera = Camera.main;
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        Debug.Log("NetworkSpawn");
        Initialize();
    }
    void Update() {
        // Move only if the application is focused and right mouse button is pressed
        if (!Application.isFocused || !IsOwner) return;

        MoveToCursor();
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
                Debug.Log("Target Position= " + targetPosition + " Target Direction=" + targetDirection);
            }
        }
    }
}
