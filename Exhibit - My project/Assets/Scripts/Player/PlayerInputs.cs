using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInputs : MonoBehaviour
{
    [Header("Character Input Values")] public Vector2 move = Vector2.zero;
    public Vector2 look = Vector2.zero;
    public bool jump;
    public bool sprint;
    public bool select;

    [Header("Movement Settings")] public bool analogMovement;


    [SerializeField] private CameraController myCamera;
    [SerializeField] private BarkinThirdPersonController controller;
    [SerializeField] private NetworkPlayer networkPlayer;
    [SerializeField] private GameObject cineMachineBrain;
    [SerializeField] private CharacterSittingHandler sittingHandler;
    private PlayerUI playerUI;

    private void Awake()
    {
        var root = transform.root;
        controller = root.GetComponentInChildren<BarkinThirdPersonController>();
        sittingHandler = root.GetComponentInChildren<CharacterSittingHandler>();
        networkPlayer = root.GetComponentInChildren<NetworkPlayer>();
        playerUI = networkPlayer.playerUI;
    }

    public Button hoverboardButton;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        myCamera = controller.GetComponent<CameraController>();
        hoverboardButton.onClick.AddListener(HoverboardInput);
        cineMachineBrain = controller.mainCamera.GetComponent<CinemachineBrain>().gameObject;
    }

    public NetworkInputData GetNetworkInput()
    {
        if (cineMachineBrain == null || playerUI._isMessageInputFieldSelected)
        {
            return new NetworkInputData();
        }

        var networkInputData = new NetworkInputData
        {
            MovementInput = move,
            IsJumpPressed = jump,
            Sprinting = sprint,
            CameraRotation = cineMachineBrain.transform.eulerAngles,
            RadToDeg = Mathf.Rad2Deg,
            IsCharacterSelectButtonPressed = select,
            AimForwardVector = cineMachineBrain.transform.forward,
            IsDartButtonPressed = _isFireButtonPressed,
            isHoverboardActive = _isHoverboardActive,
            IsBallShootButtonPressed = _isBallShootButtonPressed,
            isSitButtonPressed = _isSitButtonPressed
        };

        jump = false;
        _isFireButtonPressed = false;
        _isBallShootButtonPressed = false;
        _isSitButtonPressed = false;

        return networkInputData;
    }

    private bool _isFireButtonPressed;
    public bool _isHoverboardActive;
    public bool _isBallShootButtonPressed;
    public bool _isSitButtonPressed;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _isFireButtonPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            _isBallShootButtonPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _isSitButtonPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (hoverboardButton.interactable)
                HoverboardInput();
        }
    }

    private void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    private void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
        myCamera.SetCameraInput(look);
    }

    private void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void HoverboardInput()
    {
        _isHoverboardActive = !_isHoverboardActive;
        hoverboardButton.interactable = false;

        Invoke(nameof(HoverboardButtonInteractableAdjustment), 1f);

        Debug.Log(_isHoverboardActive ? "Hoverboard Active" : "Hoverboard Inactive");
    }

    public void HoverboardButtonInteractableAdjustment()
    {
        hoverboardButton.interactable = true;
    }

    private void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    private void SelectInput(bool newSelectState)
    {
        select = newSelectState;
    }

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        LookInput(value.Get<Vector2>());
    }

    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    public void OnSelect(InputValue value)
    {
        SelectInput(value.isPressed);
    }
#endif

    // private void SetCursorState(bool newState)
    // {
    //     Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    // }
}