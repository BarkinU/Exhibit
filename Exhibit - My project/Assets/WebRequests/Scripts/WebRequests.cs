using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

public static class WebRequests
{
    private class WebRequestsMonoBehaviour : MonoBehaviour
    {
    }

    private static WebRequestsMonoBehaviour webRequestsMonoBehaviour;

    private static void Init()
    {
        if (webRequestsMonoBehaviour == null)
        {
            GameObject gameObject = new GameObject("WebRequests");
            webRequestsMonoBehaviour = gameObject.AddComponent<WebRequestsMonoBehaviour>();
        }
    }

    public static void Get(string url, Action<string> onError, Action<string> onSuccess)
    {
        Init();
        webRequestsMonoBehaviour.StartCoroutine(GetCoroutine(url, onError, onSuccess));
    }

    private static IEnumerator GetCoroutine(string url, Action<string> onError, Action<string> onSuccess)
    {
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(url))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
            {
                onError(unityWebRequest.error);
            }
            else
            {
                onSuccess(unityWebRequest.downloadHandler.text);
            }
        }
    }

    public static void GetTexture(string url, Action<string> onError, Action<Texture2D> onSuccess)
    {
        Init();
        webRequestsMonoBehaviour.StartCoroutine(GetTextureCoroutine(url, onError, onSuccess));
    }

    public static void GetVideo(string url, Action<string> onError, Action<string> videoPath)
    {
        Init();
        webRequestsMonoBehaviour.StartCoroutine(GetVideoCoroutine(url, onError, videoPath));
    }

    private static IEnumerator GetTextureCoroutine(string url, Action<string> onError, Action<Texture2D> onSuccess)
    {
        using (UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
            {
                onError(unityWebRequest.error);
            }
            else
            {
                DownloadHandlerTexture downloadHandlerTexture =
                    unityWebRequest.downloadHandler as DownloadHandlerTexture;
                if (downloadHandlerTexture != null) onSuccess(downloadHandlerTexture.texture);
            }
        }
    }

    private static int _counter;

    private static IEnumerator GetVideoCoroutine(string url, Action<string> onError, Action<string> videoPath)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string savePath =
                    string.Format("{0}/{1}.mp4", Application.persistentDataPath, "videooo");
                System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
                Debug.Log("Video saved to: " + savePath);
            }
        }
    }
}