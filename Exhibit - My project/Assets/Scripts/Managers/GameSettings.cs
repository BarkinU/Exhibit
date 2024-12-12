using UnityEngine;

public class GameSettings : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isBuildDebug = true;

    public void Awake()
    {
        if (!isBuildDebug)
        {
            AdjustGameSettings();
        }
    }

    private void AdjustGameSettings()
    {
        Debug.unityLogger.logEnabled = false;
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
    }
}