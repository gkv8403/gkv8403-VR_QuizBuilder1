using UnityEngine;
using TMPro;
using Fusion;

public class NetworkedPlayer : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private GameObject playerCapsule;
    [SerializeField] private GameObject mainRIG;

    [Networked] private Vector3 capsulePosition { get; set; }
    [Networked] public string PlayerName { get; private set; }

    private void Start()
    {
        if (Object.HasStateAuthority)
        {
            PlayerName = NetworkManager.Instance.GetPlayerName(Object.InputAuthority);
        }

        if (!Object.HasInputAuthority)
        {
            mainCameraTransform.gameObject.SetActive(false);
            mainRIG.gameObject.SetActive(false);
        }

        UpdatePlayerName();
    }

    private void Update()
    {
        if (Object.HasInputAuthority)
        {
            SyncPositionWithCamera();
        }

        ApplyNetworkedCapsulePosition();
    }

    /// <summary>
    /// Sync local player position with main camera (only client side).
    /// </summary>
    private void SyncPositionWithCamera()
    {
        if (mainCameraTransform == null || playerCapsule == null) return;

        Vector3 targetPosition = mainCameraTransform.position;
        targetPosition.y = playerCapsule.transform.position.y; // Maintain capsule height

        // Local player directly moves the capsule
        playerCapsule.transform.position = targetPosition;

        // Send position update to the host
        if (Runner != null)
        {
            RpcSendPositionToHost(targetPosition);
        }
    }

    /// <summary>
    /// Sends the local player position to the host (StateAuthority).
    /// </summary
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSendPositionToHost(Vector3 position)

    {
        capsulePosition = position;
    }

    /// <summary>
    /// Applies the networked capsule position for all remote players.
    /// </summary>
    private void ApplyNetworkedCapsulePosition()
    {
        if (Object.HasStateAuthority)
        {
            // The host applies the received position to the capsule
            playerCapsule.transform.position = capsulePosition;
        }
        else if (!Object.HasInputAuthority)
        {
            // Remote players follow the networked position
            playerCapsule.transform.position = capsulePosition;
        }
    }

    /// <summary>
    /// Sets the player name, used by the NetworkManager.
    /// </summary>
    public void SetPlayerName(string name)
    {
        PlayerName = name;
        UpdatePlayerName();
    }

    /// <summary>
    /// Updates the player name text above the player's head.
    /// </summary>
    private void UpdatePlayerName()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PlayerName;
        }
        else
        {
            Debug.LogError("[NetworkedPlayer] Player name TextMeshPro is not assigned!");
        }
    }
}
