using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
    [SerializeField] private RectTransform uiHandleRectTransform;
    [SerializeField] private Color backgroundActiveColor;
    [SerializeField] private Color handleActiveColor;

    private Image _backgroundImage, _handleImage;

    private Color _backgroundDefaultColor, _handleDefaultColor;

    private Toggle _toggle;

    private Vector2 _handlePosition;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();

        _handlePosition = uiHandleRectTransform.anchoredPosition;

        _backgroundImage = uiHandleRectTransform.parent.GetComponent<Image>();
        _handleImage = uiHandleRectTransform.GetComponent<Image>();

        _backgroundDefaultColor = _backgroundImage.color;
        _handleDefaultColor = _handleImage.color;

        _toggle.onValueChanged.AddListener(OnSwitch);

        if (_toggle.isOn)
            OnSwitch(true);
    }

    private void OnSwitch(bool on)
    {
        //uiHandleRectTransform.anchoredPosition = on ? handlePosition * -1 : handlePosition ; // no anim
        uiHandleRectTransform.DOAnchorPos(on ? _handlePosition * -1 : _handlePosition, .4f).SetEase(Ease.InOutBack);

        //backgroundImage.color = on ? backgroundActiveColor : backgroundDefaultColor ; // no anim
        _backgroundImage.DOColor(on ? backgroundActiveColor : _backgroundDefaultColor, .6f);

        //handleImage.color = on ? handleActiveColor : handleDefaultColor ; // no anim
        _handleImage.DOColor(on ? handleActiveColor : _handleDefaultColor, .4f);
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnSwitch);
    }
}