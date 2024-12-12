using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SearchService;

public class CameraController : NetworkBehaviour
{
    [Header("CineMachine")] public GameObject cineMachineCameraTarget;
    public Enums.CameraMode cameraMode;
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    public float cameraAngleOverride;
    public bool lockCameraPosition = false;
    [SerializeField] private Toggle cameraModeToggle;
    [SerializeField] private GameObject fpsCamera;
    [SerializeField] private GameObject tpsCamera;
    [SerializeField] private CinemachinePOV fpsCameraReference;
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Slider cameraSensitivitySlider;
    private float _cineMachineTargetPitch;

    // cineMachine
    private float _cineMachineTargetYaw;

    private Vector3 _look;
    private float _oldFpsXSpeed;
    private float _oldFpsYSpeed;
    [SerializeField] private GameObject[] fpsCameraIgnoreObjects;

    private void Awake()
    {
        mainCamera = GetComponentInChildren<Camera>();
        fpsCameraReference = fpsCamera.GetComponent<CinemachineVirtualCamera>()
            .GetCinemachineComponent<CinemachinePOV>();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            transform.Rotate(0, 90, 0);
        }
        if (!Object.HasInputAuthority)
        {
            Destroy(fpsCamera);
            Destroy(tpsCamera);
            this.enabled = false;
        }
    }

    private void Start()
    {
        GetPlayerPrefs();

        _cineMachineTargetYaw = cineMachineCameraTarget.transform.root.eulerAngles.y;
        // set old camera speed values to set new values over them.
        _oldFpsXSpeed = fpsCameraReference.m_HorizontalAxis.m_MaxSpeed;
        _oldFpsYSpeed = fpsCameraReference.m_VerticalAxis.m_MaxSpeed;

        fpsCamera.transform.SetParent(null);
        tpsCamera.transform.SetParent(null);

        Invoke(nameof(AdjustListeners), 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraModeToggle.isOn = !cameraModeToggle.isOn;
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void GetPlayerPrefs()
    {
        cameraSensitivitySlider.value = PlayerPrefs.GetFloat(PlayerPrefsKeys.Sensitivity);
    }

    private void AdjustListeners()
    {
        cameraSensitivitySlider.onValueChanged.AddListener(delegate { SetCameraSensitivity(); });
        cameraModeToggle.onValueChanged.AddListener(delegate { ToggleCameraMode(); });
    }

    private void SetCameraSensitivity()
    {
        fpsCameraReference.m_VerticalAxis.m_MaxSpeed = cameraSensitivitySlider.value * _oldFpsXSpeed;
        fpsCameraReference.m_HorizontalAxis.m_MaxSpeed = cameraSensitivitySlider.value * _oldFpsYSpeed;

        PlayerPrefs.SetFloat(PlayerPrefsKeys.Sensitivity, cameraSensitivitySlider.value);

        Debug.Log("Camera sensitivity set to");
    }

    private void ToggleCameraMode()
    {
        if (cameraMode == Enums.CameraMode.Tps)
        {
            cameraMode = Enums.CameraMode.Fps;
            fpsCamera.SetActive(true);
            tpsCamera.SetActive(false);
            mainCamera.cullingMask &=
                ~LayerMask.GetMask(LayerMasks.FPSCameraIgnore); // remove this layer from culling mask of camera
            mainCamera.cullingMask &=
                ~LayerMask.GetMask(LayerMasks.LocalPlayerModel); // remove this layer from culling mask of camera
            for (int i = 0; i < fpsCameraIgnoreObjects.Length; i++)
            {
                fpsCameraIgnoreObjects[i].layer = LayerMask.NameToLayer(LayerMasks.FPSCameraIgnore);
            }

            return;
        }

        if (cameraMode == Enums.CameraMode.Fps)
        {
            cameraMode = Enums.CameraMode.Tps;
            fpsCamera.SetActive(false);
            tpsCamera.SetActive(true);
            mainCamera.cullingMask = -1;
            for (int i = 0; i < fpsCameraIgnoreObjects.Length; i++)
            {
                fpsCameraIgnoreObjects[i].layer = 3; // 3 is player layer
            }
        }
    }

    public void SetCameraInput(Vector3 look)
    {
        _look = look;
    }


    private void CameraRotation()
    {
        int sensitivity = cameraMode == Enums.CameraMode.Fps ? 10 : 100;
        if (Input.GetMouseButton(1))
        {
            if (_look.sqrMagnitude >= .001f && !lockCameraPosition)
            {
                _cineMachineTargetYaw += _look.x * cameraSensitivitySlider.value * sensitivity * Time.deltaTime;
                _cineMachineTargetPitch += _look.y * cameraSensitivitySlider.value * sensitivity * Time.deltaTime;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cineMachineTargetYaw =
                ClampAngle(_cineMachineTargetYaw, float.MinValue, float.MaxValue);
            _cineMachineTargetPitch = ClampAngle(_cineMachineTargetPitch, bottomClamp, topClamp);
        }

        cineMachineCameraTarget.transform.rotation = Quaternion.Euler(_cineMachineTargetPitch + cameraAngleOverride,
            _cineMachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


}