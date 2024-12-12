using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED

#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */


[RequireComponent(typeof(CharacterController))]
[OrderBefore(typeof(NetworkTransform))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif
[DisallowMultipleComponent]
public class BarkinThirdPersonController : NetworkTransform
{
    public CharacterController Controller { get; private set; }
    public float moveSpeed = 2;
    public float sprintSpeed = 6;
    public float speedChangeRate = 10.0f;
    public float jumpHeight = 1.2f;
    public float gravity = -15.0f;
    public float jumpTimeout = 0.50f;
    public float fallTimeout = 0.15f;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public LayerMask groundLayers;
    public Animator animator;
    public GameObject mainCamera;


    [SerializeField] private PlayerInputs input;

    private const float TerminalVelocity = 53.0f;
    private float _animationBlend;
    private int _animIDDartFire;
    private bool _hasAnimator;
    private float _jumpTimeoutDelta;
    private float _rotationVelocity;
    protected override Vector3 DefaultTeleportInterpolationVelocity => Velocity;
    protected override Vector3 DefaultTeleportInterpolationAngularVelocity => new Vector3(0f, 0f, _rotationVelocity);


    // animation IDs
    private static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimIDGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimIDJump = Animator.StringToHash("Jump");
    private static readonly int AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");


    // Networked variables
    [Networked] private float Speed { get; set; }
    [Networked] private float TargetRotation { get; set; }
    [Networked] private float TargetSpeed { get; set; }
    [Networked] private float VerticalVelocity { get; set; }
    [Networked] private bool Grounded { get; set; }
    [Networked] public bool CanGravityAct { get; set; }
    [Networked] private Vector3 Velocity { get; set; }
    [Networked] private Vector3 InputDirection { get; set; }

    [Networked(OnChanged = nameof(HoverboardStateChanged))]
    public NetworkBool isHoverboardActive { get; set; }

    public int hoverboardSpeed = 15;
    [SerializeField] private GameObject hoverboardGameobject;

    [Networked] public bool IsCharacterInElevator { get; set; }
    public NetworkBool isCharacterUpForHoverboard;
    private CharacterSittingHandler characterSittingHandler;


    protected override void Awake()
    {
        base.Awake();
        // get a reference to our main camera
        if (mainCamera == null)
        {
            mainCamera = GetComponentInChildren<Camera>().gameObject;
        }

        _hasAnimator = transform.GetChild(0).GetComponentInChildren<Animator>();

        animator = transform.GetChild(0).GetComponentInChildren<Animator>();

        Controller = GetComponent<CharacterController>();
        characterSittingHandler = GetComponent<CharacterSittingHandler>();
    }

    private void Start()
    {
        // reset our timeouts on start
        _jumpTimeoutDelta = jumpTimeout;
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = Grounded ? transparentGreen : transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        var position = transform.position;
        Gizmos.DrawSphere(
            new Vector3(position.x, position.y - groundedOffset, position.z),
            groundedRadius);
    }

    [SerializeField] private List<GameObject> objectsToCloseWithoutAuthority;

    public override void Spawned()
    {
        base.Spawned();
        // Caveat: this is needed to initialize the Controller's state and avoid unwanted spikes in its perceived velocity
        Controller.Move(transform.position);

        if (!Object.HasStateAuthority & !Object.HasInputAuthority)
        {
            for (int i = 0; i < objectsToCloseWithoutAuthority.Count; i++)
            {
                objectsToCloseWithoutAuthority[i].SetActive(false);
            }
        }

        if (Object.HasInputAuthority)
        {
            input.gameObject.SetActive(true);
        }

        CanGravityAct = true;
    }

    protected override void CopyFromBufferToEngine()
    {
        // Trick: CC must be disabled before resetting the transform state
        Controller.enabled = false;

        // Pull base (NetworkTransform) state from networked data buffer
        base.CopyFromBufferToEngine();

        // Re-enable CC
        Controller.enabled = true;
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
    public void SendConferenceEntranceRPC()
    {
        input._isHoverboardActive = false;
        input.hoverboardButton.interactable = false;
        isHoverboardActive = false;
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
    public void SendConferenceExitRPC()
    {
        input.hoverboardButton.interactable = true;
    }


    public static void HoverboardStateChanged(Changed<BarkinThirdPersonController> changed)
    {
        changed.Behaviour.hoverboardGameobject.SetActive(!changed.Behaviour.hoverboardGameobject.activeSelf);
        changed.Behaviour.CanGravityAct = !changed.Behaviour.CanGravityAct;
        changed.Behaviour.Controller.Move(Vector3.up / 3 * 2);
    }

    public override void FixedUpdateNetwork()
    {
        if (characterSittingHandler.IsSitButtonPressed)
        {
            return;
        }

        GroundedCheck();

        if (GetInput(out NetworkInputData networkInputData))
        {
            if (!networkInputData.isHoverboardActive)
            {
                Elevator(IsCharacterInElevator);
            }

            isHoverboardActive = networkInputData.isHoverboardActive;

            if (CanGravityAct)
            {
                JumpAndGravity(networkInputData.IsJumpPressed);
            }

            Move(networkInputData.MovementInput, networkInputData.Sprinting, networkInputData.CameraRotation,
                networkInputData.RadToDeg, networkInputData.isHoverboardActive);
        }
    }

    private void Move(Vector2 networkMovementInput, bool sprinting, Vector3 cameraRotation, float radToDeg,
        bool hoverboardActivated)
    {
        networkMovementInput = networkMovementInput.normalized;

        // set target speed based on move speed, sprint speed and if sprint is pressed
        if (!hoverboardActivated)
        {
            TargetSpeed = sprinting ? sprintSpeed : moveSpeed;
        }
        else
        {
            TargetSpeed = hoverboardSpeed;
            VerticalVelocity = 0;
        }

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (networkMovementInput == Vector2.zero) TargetSpeed = 0.0f;

        float inputMagnitude = 1f;

        Speed = TargetSpeed;

        _animationBlend = Mathf.Lerp(_animationBlend, Speed, speedChangeRate * Runner.DeltaTime);
        if (_animationBlend < 0.01f) _animationBlend = 0f;


        InputDirection = new Vector3(networkMovementInput.x, 0, networkMovementInput.y).normalized;
        // player's target rotation
        TargetRotation = Mathf.Atan2(InputDirection.x, InputDirection.z) * radToDeg + cameraRotation.y;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation, ref _rotationVelocity,
            Runner.DeltaTime * 6);

        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

        Vector3 targetDirection =
            Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;

        Velocity = Speed * targetDirection.normalized +
                   new Vector3(0, VerticalVelocity, 0);
        // move the player

        if (hoverboardActivated)
        {
            Velocity *= 1.5f;
        }

        Controller.Move(Velocity * Runner.DeltaTime);


        animator.SetBool("isHoverboardActive", hoverboardActivated);

        // update animator if using character
        if (_hasAnimator && !hoverboardActivated)
        {
            animator.SetFloat(AnimIDSpeed, _animationBlend);
            animator.SetFloat(AnimIDMotionSpeed, inputMagnitude);
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        var position = transform.position;

        Vector3 spherePosition = new Vector3(position.x, position.y - groundedOffset,
            position.z);
        Grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            animator.SetBool(AnimIDGrounded, Grounded);
        }
    }

    private void JumpAndGravity(bool isJumpPressed)
    {
        if (Grounded)
        {
            // reset the fall timeout timer

            // update animator if using character
            if (_hasAnimator)
            {
                animator.SetBool(AnimIDJump, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (VerticalVelocity < 0.0f && CanGravityAct)
            {
                VerticalVelocity = -.2f;
            }

            // Jump
            if (isJumpPressed && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                VerticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    animator.SetBool(AnimIDJump, true);
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Runner.DeltaTime;
            }
        }
        else
        {
            // if we are not grounded, do not jump
            input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (VerticalVelocity < TerminalVelocity)
        {
            VerticalVelocity += gravity * Runner.DeltaTime;
        }
    }

    private void Elevator(bool state)
    {
        if (!state) return;

        if (transform.position.y > 11)
        {
            Controller.Move(new Vector3(0, 0, Runner.DeltaTime * 5));
        }
        else
        {
            Controller.Move(new Vector3(0, Runner.DeltaTime * 5, 0));
        }
    }
}