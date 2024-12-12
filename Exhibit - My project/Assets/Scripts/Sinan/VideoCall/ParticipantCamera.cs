using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ParticipantCamera : MonoBehaviour
{
    private bool _isAnimationOpen, _isFullScreenOpenable;

    [SerializeField] private Animator fullScreenAnimator;
    public VideoCallHandler videoCallHandler;
    private Vector2 _oldPosition;
    private int childIndex;
    public TMP_Text myUsername;
    public Texture camClosedTexture;

    private void OnEnable()
    {
        //    _videoCallHandler = 
        childIndex = transform.GetSiblingIndex();
    }

    public void FullScreenAnimation()
    {
        if (!videoCallHandler.isFullScreenOpen || _isFullScreenOpenable)
        {
            _isAnimationOpen = !_isAnimationOpen;

            if (_isAnimationOpen)
            {
                videoCallHandler.isFullScreenOpen = true;
                _isFullScreenOpenable = true;
                transform.SetParent(videoCallHandler.panelParent.transform);
                _oldPosition = transform.position;
                StartCoroutine(LerpPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), 0.5f));
                fullScreenAnimator.Play("FullScreenAnimation");
            }
            else
            {
                fullScreenAnimator.Play("MinimizeScreenAnimation");
                StartCoroutine(LerpPosition(_oldPosition, 0.5f));
                SetParentToOldParent();
            }
        }
    }

    public void SetParentToOldParent()
    {
        transform.SetParent(videoCallHandler.viewParent.transform);
        transform.SetSiblingIndex(childIndex);
        videoCallHandler.isFullScreenOpen = false;
        _isFullScreenOpenable = false;
    }

    IEnumerator LerpPosition(Vector2 targetPosition, float duration)
    {
        float time = 0;
        Vector2 startPosition = transform.position;
        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    public void ChangeMyTextureToClosed()
    {
        GetComponent<RawImage>().texture = camClosedTexture;
    }
}