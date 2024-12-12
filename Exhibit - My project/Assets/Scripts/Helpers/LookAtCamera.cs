using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private void Start()
    {
        _camera = GameObject.FindObjectOfType<Camera>();
    }

    private void Update()
    {
        if (_camera) transform.LookAt(_camera.transform.position);
        transform.Rotate(0, 180, 0, Space.World);
    }
}