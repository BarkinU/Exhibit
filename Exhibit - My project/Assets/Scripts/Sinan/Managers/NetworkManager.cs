using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public const string BaseURL = "https://api.minego.io/v1"; //

    public delegate void NetworkCallback(string text);

    private NetworkCallback _registerSuccessCallback;

    private NetworkCallback _registerFailCallback;

    private IEnumerator _GetRequest(string baseURL, NetworkCallback failCallback, NetworkCallback successCallback)
    {
        using (var webRequest = UnityWebRequest.Get(baseURL))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            var pages = baseURL.Split('/');
            var page = pages.Length - 1;

            var error = webRequest.error;
            var response = webRequest.downloadHandler.text;
            var webRequestResult = webRequest.result;
            print(webRequestResult);
            webRequest.Dispose();


            switch (webRequestResult)
            {
                case UnityWebRequest.Result.ConnectionError:
                    // errors will be handled here
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    failCallback(response);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    failCallback(response);
                    break;
                case UnityWebRequest.Result.Success:
                    print(response);
                    print(pages[page]);
                    successCallback?.Invoke(response);
                    break;
            }
        }
    }

    private IEnumerator _PostRequest(string baseURL, WWWForm formData, NetworkCallback failCallback,
        NetworkCallback successCallback)
    {
        Debug.Log("Posting: " + baseURL);
        using (UnityWebRequest webRequest = UnityWebRequest.Post(baseURL, formData))
        {
            //webRequest.SetRequestHeader("AUTHORIZATION", PlayerPrefs.GetString(PrefKeys._sessionTokenPref));
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = baseURL.Split('/');
            int page = pages.Length - 1;

            var error = webRequest.error;
            var response = webRequest.downloadHandler.text;
            var webRequestResult = webRequest.result;
            print(webRequestResult);
            webRequest.Dispose();


            switch (webRequestResult)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + error);
                    failCallback?.Invoke(response);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + error);
                    Debug.Log("Error message: " + response);
                    failCallback?.Invoke(response);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + response);
                    successCallback?.Invoke(response);
                    break;
            }
        }
    }

    public void FailCallback(string data)
    {
        var response = JsonUtility.FromJson<FailCallbackError>(data);
        print(data);
        // when the request fails, the response will be handled here
    }


    #region Auth

    public void RegisterRequest(string email, string password, string passwordConfirm,
        NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("email", email);
        formData.AddField("password", password);
        formData.AddField("password2", passwordConfirm);
        formData.AddField("role", "User");
        StartCoroutine(_PostRequest(BaseURL + "/game/register", formData, FailCallback, successCallback));
    }

    public void LoginRequest(string email, string password,
        NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("email", email);
        formData.AddField("password", password);
        StartCoroutine(_PostRequest(BaseURL + "/game/login", formData, FailCallback, successCallback));
    }

    public void CreateUsernameRequest(string email, string username,
        NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("email", email);
        formData.AddField("username", username);
        StartCoroutine(_PostRequest(BaseURL + "/game/createUsername", formData, FailCallback, successCallback));
    }

    #endregion

    #region CreateEventsRoomsStands

    public void CreateEventRequest(string title, NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("title", title);
        formData.AddField("start_date", System.DateTime.Now.ToString(CultureInfo.CurrentCulture));
        formData.AddField("end_date", System.DateTime.Now.ToString(CultureInfo.InvariantCulture));

        StartCoroutine(_PostRequest(BaseURL + "/game/events/create", formData, FailCallback, successCallback));
    }

    public void GetAllEvents(NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("perPage", "10");
        formData.AddField("page", "1");
        formData.AddField("search", "");

        StartCoroutine(_PostRequest(BaseURL + "/game/events/getAll", formData, FailCallback, successCallback));
    }

    public void CreateRoomRequest(string eventId, NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("game_event_id", eventId);

        StartCoroutine(_PostRequest(BaseURL + "/game/room/create", formData, FailCallback, successCallback));
    }

    public void GetAllRoomsInTheEvent(string eventId, NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("perPage", "8");
        formData.AddField("page", "1");
        formData.AddField("search", "");
        formData.AddField("game_event_id", eventId);

        StartCoroutine(_PostRequest(BaseURL + "/game/room/getAll", formData, FailCallback, successCallback));
    }

    public void GetAllStandsInTheRoom(string roomId, NetworkCallback successCallback)
    {
        var formData = new WWWForm();
        formData.AddField("room_id", roomId);
        formData.AddField("perPage", "5");
        formData.AddField("page", "1");
        formData.AddField("search", "");


        StartCoroutine(_PostRequest(BaseURL + "/game/room/stands/getAll", formData, FailCallback, successCallback));
    }

    #endregion

    #region VideoCall

    public void GetRoomToken(string roomID, string userName,
        NetworkCallback successCallback)
    {
        if (String.IsNullOrEmpty(userName))
            return;
        var formData = new WWWForm();
        formData.AddField("room_id", roomID);
        formData.AddField("user_name", userName);
        StartCoroutine(_PostRequest(BaseURL + "/live/getToken", formData, FailCallback, successCallback));
    }

    #endregion
}