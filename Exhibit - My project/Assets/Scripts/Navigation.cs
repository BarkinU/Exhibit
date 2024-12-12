using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Navigation : NetworkBehaviour
{

    [SerializeField] private GameObject mapImage;
    [SerializeField] private Button mapCloseButton;
    [SerializeField] private Button mapOpenButton;
    [SerializeField] private Camera mapCamera;
    Camera mapCameraComponent;
    private void Awake()
    {
        mapCamera.transform.SetParent(null);
        mapImage.transform.SetParent(null);
    }
    private void Start()
    {
        mapCameraComponent = mapCamera.GetComponent<Camera>();
        transform.SetParent(null);
        mapCloseButton.onClick.AddListener(CloseMap);
        mapOpenButton.onClick.AddListener(MapButton);
    }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            mapCamera.transform.gameObject.SetActive(false);
            mapImage.transform.gameObject.SetActive(false);
            mapCloseButton.gameObject.SetActive(false);
            mapOpenButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    public void MapButton()
    {
        mapCamera.gameObject.SetActive(true);
    }

    public void CloseMap()
    {
        mapCamera.gameObject.SetActive(false);
    }
}
