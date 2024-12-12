using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIController : MonoBehaviour
{
    [Header("Start Fade Out")] [SerializeField]
    private Image startFadeOutImage;

    private bool _isStartFadeOutDone;
    private float _startFadeOutAlpha;
    private float _startFadeOutTimer;

    // References

    // Set the FPS camera sensitivity
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button menuCloseButton;


    private void Start()
    {
        menuButton.onClick.AddListener(OpenMenuPanel);
        menuCloseButton.onClick.AddListener(CloseMenuPanel);
        _startFadeOutAlpha = startFadeOutImage.color.a;
    }

    private void Update()
    {
        if (!_isStartFadeOutDone)
        {
            FadeOut();
        }
    }

    private void OpenMenuPanel()
    {
        menuPanel.SetActive(true);
    }

    private void CloseMenuPanel()
    {
        menuPanel.SetActive(false);
    }

    private void FadeOut()
    {
        _startFadeOutTimer += Time.deltaTime;
        if (_startFadeOutTimer >= 3)
        {
            startFadeOutImage.color = Color.Lerp(startFadeOutImage.color, Color.clear, Time.deltaTime / 2);

            if (startFadeOutImage.color.a <= _startFadeOutAlpha / 3)
            {
                startFadeOutImage.gameObject.SetActive(false);
                _isStartFadeOutDone = true;
            }
        }
    }
}