using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class DartBullet : NetworkBehaviour
{
    [SerializeField] private NetworkRigidbody bulletRigidbody;
    private bool _targetHit;

    private void Start()
    {
        bulletRigidbody = GetComponent<NetworkRigidbody>();
    }

    private void OnEnable()
    {
        Invoke(nameof(DeSpawn), 6f);
    }

    public void OnCollisionEnter(Collision other)
    {
        bulletRigidbody.Rigidbody.isKinematic = true;
    }

    private void DeSpawn()
    {
        Runner.Despawn(this.Object);
    }
}