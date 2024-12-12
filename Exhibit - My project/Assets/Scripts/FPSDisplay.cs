using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{

    [SerializeField] private TMP_Text fpsText;
    float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = "FPS: " + Mathf.RoundToInt(fps).ToString();
    }
}