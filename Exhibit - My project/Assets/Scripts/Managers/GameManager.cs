using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector2 cameraViewRotation = Vector2.zero;

    public string PlayerNickName
    {
        get => playerNickName;
        set => playerNickName = value;
    }

    public string playerNickName;

    [SerializeField] private TMP_Text playerName;
    [SerializeField] private GameObject loadingPanel;

    public void OpenLoadingPanel()
    {
        loadingPanel.SetActive(true);
    }

    public void CloseLoadingPanel()
    {
        loadingPanel.SetActive(false);
    }
}