using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject chatGPTCanvas;

    public void ClearScreen()
    {
        chatGPTCanvas.SetActive(false);
    }

    public void OpenChatGPTCanvas()
    {
        ClearScreen();
        chatGPTCanvas.SetActive(true);
    }
}