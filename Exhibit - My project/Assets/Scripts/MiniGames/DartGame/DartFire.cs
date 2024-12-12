using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class DartFire : NetworkBehaviour
{
    [SerializeField] private Transform attackPoint;

    [Header("Settings")]
    public float throwCooldown = 3f;
    [SerializeField] private float throwUpwardForce = 10f;
    [SerializeField] private float throwStrength;

    [FormerlySerializedAs("_objectToThrow")]
    [Header("Throwing")]
    [SerializeField]
    private NetworkObject objectToThrow;

    [SerializeField] private GameObject myCrossHair;

    [Networked(OnChanged = nameof(OnDartGameStarted))]
    public bool IsCharacterInDartGame { get; set; }

    [Networked] private bool ReadyToThrow { get; set; }
    private NetworkObject _bullet;
    [Networked] private Vector3 ForceDirection { get; set; }
    private NetworkRigidbody _bulletRb;
    [Networked] private Vector3 ForceToAdd { get; set; }
    [Networked]
    public TickTimer throwTimer { get; set; }

    public static void OnDartGameStarted(Changed<DartFire> changed)
    {
        changed.Behaviour.myCrossHair.SetActive(!changed.Behaviour.myCrossHair.activeSelf);
    }

    public override void Spawned()
    {
        ReadyToThrow = true;
        if (Runner.IsServer)
        {
            // initialize timer
            throwTimer = TickTimer.CreateFromSeconds(Runner, throwCooldown);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;
        if (GetInput(out NetworkInputData networkInputData))
        {
            ForceDirection = networkInputData.AimForwardVector;
            if (networkInputData.IsDartButtonPressed)
            {
                if (IsCharacterInDartGame)
                {
                    if (throwTimer.Expired(Runner))
                    {
                        ThrowRPC(attackPoint.position + networkInputData.AimForwardVector, ForceDirection,
                            throwStrength,
                            throwUpwardForce, ForceDirection);
                        throwTimer = TickTimer.CreateFromSeconds(Runner, throwCooldown);
                    }
                }
            }
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void ThrowRPC(Vector3 spawnPosition, Vector3 rotation, float throwingStrength,
        float throwingUpwardForce,
        Vector3 forceDirection)
    {
        // spawn object to throw
        if (Runner.IsServer)
        {
            _bullet = Runner.Spawn(objectToThrow, spawnPosition, Quaternion.LookRotation(rotation),
                Object.InputAuthority);

            // get rigidbody component
            _bulletRb = _bullet.GetComponent<NetworkRigidbody>();
            // add force
            ForceToAdd = forceDirection * throwingStrength + transform.up * throwingUpwardForce;

            _bulletRb.Rigidbody.AddForceAtPosition(ForceToAdd, spawnPosition, ForceMode.Impulse);
            throwTimer = TickTimer.CreateFromSeconds(Runner, throwCooldown);
        }
    }

}