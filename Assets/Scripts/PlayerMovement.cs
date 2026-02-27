using UnityEngine;
using Fusion;
using Network_Scripts; //This is where the network input data is located, in the editor > assets > scritps > network

public class PlayerMovement : NetworkBehaviour
{
    //Changeable
    private float _speed = 3; 
    //DO NOT CHANGE
    private Vector3 _velocity;
    private const float Sensitivity = 1;
    private const float Gravity = 9.81f;

    [Header("Components Needed")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController controller;

    #region Network Properties
        /* <summary>
            this sets up the local positions to the fusion 2 network, do not change the public variable to private,
            I think I called these variables in other scripts as well - dani, 19:21 | Feb 17
         </summary>
         */
        [Networked] public Vector3 NetworkedPosition { get; set; }
        [Networked] public Quaternion NetworkedRotation { get; set; }
        [Networked] private float NetworkedXRotation { get; set; }
    #endregion

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            //when the player controls the camera, the cursor is not active
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            /* This checks for the player's camera, the camera should only follow the local player, but since
             * but we're only using ONE player prefab so if the remote player's camera is also in the world this makes
             * sure their camera and audio listener is turned off. --dani
             */
            if (playerCamera != null) //LocalPlayer
            {
                playerCamera.gameObject.SetActive(true);
                Camera yourCam = playerCamera.GetComponent<Camera>();
                if (yourCam != null) yourCam.enabled = true;
                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }
            Debug.Log($"Your player has joined the room: {Object.InputAuthority}");
        }
        else //RemotePlayer
        {
            if (playerCamera != null)
            {
                Camera notYourCam = playerCamera.GetComponent<Camera>();
                if (notYourCam != null) notYourCam.enabled = false;
                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }

            Debug.Log($"another player has spawned: {Object.InputAuthority}");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            // LOCAL: process movement
            MovePlayer(input);
            RotatePlayer(input);
            RotateCamera(input);

            // Write to networked properties so remote players can read them
            NetworkedPosition = transform.position;
            NetworkedRotation = transform.rotation;
            NetworkedXRotation = playerCamera.localEulerAngles.x;
        }

        // Gravity always runs for local player only
        ApplyGravity();
    }

    // ✅ Update() handles smooth interpolation for remote players
    // This runs every frame unlike FixedUpdateNetwork which runs on network ticks
    private void Update()
    {
        // Only interpolate remote players
        if (HasInputAuthority) return;

        // Smoothly move remote players to their networked position
        transform.position = Vector3.Lerp(
            transform.position,
            NetworkedPosition,
            Time.deltaTime * 15f
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            NetworkedRotation,
            Time.deltaTime * 15f
        );

        // Sync camera rotation for remote players
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Lerp(
                playerCamera.localRotation,
                Quaternion.Euler(NetworkedXRotation, 0f, 0f),
                Time.deltaTime * 15f
            );
        }
    }

    private void ApplyGravity()
    {
        // Only local player drives CharacterController
        if (!HasInputAuthority) return;

        if (controller.isGrounded)
            _velocity.y = -2f;
        else
            _velocity.y -= Gravity * 2f * Runner.DeltaTime;

        controller.Move(new Vector3(0, _velocity.y, 0) * Runner.DeltaTime);
    }

    private void MovePlayer(NetworkInputData input)
    {
        Vector3 moveDirection = transform.right * input.movementInput.x
                              + transform.forward * input.movementInput.z;

        _speed = input.isSprinting ? 9f : 3f;
        controller.Move(moveDirection * _speed * Runner.DeltaTime);
    }

    private void RotatePlayer(NetworkInputData input)
    {
        transform.Rotate(0f, input.mouseInput.x * Sensitivity, 0f);
    }

    private void RotateCamera(NetworkInputData input)
    {
        if (playerCamera == null) return;

        float currentXRotation = playerCamera.localEulerAngles.x;

        if (currentXRotation > 180f)
            currentXRotation -= 360f;

        currentXRotation -= input.mouseInput.y * Sensitivity;
        currentXRotation = Mathf.Clamp(currentXRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
    }

    #region AudioSource
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_PlayFootstep(Vector3 position)
        {
            // AudioSource.PlayClipAtPoint(footstepSound, position);
        }
    #endregion
}