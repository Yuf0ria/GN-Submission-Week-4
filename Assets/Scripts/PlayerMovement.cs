using UnityEngine;
using Fusion;

public class PlayerMovement : NetworkBehaviour
{
    //vectors for movement
    private Vector3 _velocity;
    private Vector3 _playerMovementInput;
    private Vector2 _playerMouseInput;
    
    //Movement & Rotation
    private float _speed = 3,
        _xRotation;
    //constants
    private const float Sensitivity = 1,
        Gravity = 9.81f;

    [Header("Components Needed")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform player;

    #region Network Functions
        [Networked] private Vector3 NetworkPosition { get; set; }
        [Networked] private Quaternion NetworkRotation { get; set; }
        [Networked] private bool IsSprinting { get; set; }
    #endregion

    public override void Spawned()
    {
        // Only lock cursor for the local player
        if (HasInputAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only process input for the player we control
        if (HasInputAuthority)
        {
            _playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            _playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            MovePlayer();
            MoveCamera();

            // Update networked state
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
            IsSprinting = Input.GetKey(KeyCode.LeftShift);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, NetworkPosition, Runner.DeltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, NetworkRotation, Runner.DeltaTime * 10f);
        }

    }

    private void MovePlayer()
    {
        var moveVector = transform.TransformDirection(_playerMovementInput);

        if (controller.isGrounded)
        {
            _velocity.y = -1f;
            _speed = IsSprinting ? 9 : 3; // 3 * 3
            controller.Move(moveVector * _speed * Runner.DeltaTime);
        }
        else
        {
            _velocity.y += Gravity * -2f * Runner.DeltaTime;
        }
        
        controller.Move(_velocity * Runner.DeltaTime);
    }

    private void MoveCamera()
    {
        _xRotation -= _playerMouseInput.y * Sensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.Rotate(0f, _playerMouseInput.x * Sensitivity, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
    #region AudioSource
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_PlayFootstep(Vector3 position)
        {
            // AudioSource.PlayClipAtPoint(footstepSound, position);
        }
    #endregion
    
}