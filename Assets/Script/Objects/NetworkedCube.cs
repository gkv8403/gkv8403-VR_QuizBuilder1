using UnityEngine;
using Fusion;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkedCube : NetworkBehaviour
{
    // Networked variables to sync position and rotation
    [Networked] private Vector3 cubePosition { get; set; }
    [Networked] private Quaternion cubeRotation { get; set; }

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Add listeners for grab and release events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void Update()
    {
        if (Object.HasInputAuthority)
        {
            // If the local player has input authority, we update position and rotation based on their interaction
            if (Vector3.Distance(transform.position, cubePosition) > 0.1f || Quaternion.Angle(transform.rotation, cubeRotation) > 1f)
            {
                cubePosition = transform.position;
                cubeRotation = transform.rotation;
            }
        }

        // Apply the synchronized position and rotation across all clients
        transform.position = cubePosition;
        transform.rotation = cubeRotation;
    }

    // Called when the cube is grabbed
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Only the player who grabs the cube will have control of it
        if (Object.HasInputAuthority)
        {
            cubePosition = transform.position;
            cubeRotation = transform.rotation;

            // Assign input authority to the current player when they grab the object
            Object.AssignInputAuthority(Object.InputAuthority);
        }
    }

    // Called when the cube is released
    private void OnReleased(SelectExitEventArgs args)
    {
        // The cube should be able to sync its position when released
        if (Object.HasInputAuthority)
        {
            cubePosition = transform.position;
            cubeRotation = transform.rotation;

            // We can't clear input authority directly in Fusion, but we can rely on the object losing control after release
            // So the object will be free to be controlled by another player
        }
    }
}
