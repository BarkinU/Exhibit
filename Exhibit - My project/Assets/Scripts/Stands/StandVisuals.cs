using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StandVisuals : MonoBehaviour
{
    // Panel Types

    // Stand 3
    // Left, Rigth video ; mid left mid right Panel

    // Stand 2
    // mid video; left right Panel

    // Stand 1
    // mid left and right Panel


    public enum StandType
    {
        Stand1,
        Stand2,
        Stand3
    }

    [SerializeField] private List<SaloonVisualsHandler> rooms;
    private int taskCondition;
    private bool isTaskCompleted;
    private int taskCount;

    private Dictionary<int, List<GetAllStandsInTheRoomData>> standsInRooms =
        new Dictionary<int, List<GetAllStandsInTheRoomData>>();

    private List<int> roomIds = new List<int>();

    private void Start()
    {
        taskCondition = 0;
        // if (Singleton.Main != null)
        // {
        //     GetAllRoomsInTheEventRequestSender();
        // }
    }

    // public void GetTexture(int level, string logo_url, int roomNumber)
    // {
    //     WebRequests.GetTexture(logo_url, (string error) =>
    //     {
    //         // Error
    //         Debug.Log("Error: " + error);
    //     }, (Texture2D texture2D) =>
    //     {
    //         // Successfully contacted URL
    //         Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
    //             new Vector2(.5f, .5f), 10f);
    //         rooms[roomNumber].ChangeImage(level, sprite);
    //     });
    // }

    // public void GetVideo()
    // {
    //     string url = "http://192.168.1.121:8000/uploads/video1.mp4";
    //     WebRequests.GetVideo(url, (string error) =>
    //         {
    //             // Error
    //             Debug.Log("Error: " + error);
    //             textMesh.SetText("Error: " + error);
    //         }, (string videoPath) =>
    //         {
    //             // Successfully contacted URL
    //             textMesh.SetText("Success!");
    //             Debug.Log("Video Path: " + videoPath);
    //         }
    //     );
    // }

    #region GetEventDetails

    private void GetAllRoomsInTheEventRequestSender()
    {
        Singleton.Main.NetworkManager.GetAllRoomsInTheEvent("3", GetAllRoomsInTheEventResponse);
    }

    private void GetAllRoomsInTheEventResponse(string data)
    {
        var json = JsonUtility.FromJson<GetAllRoomsInTheEventResponse>(data);
        taskCount = json.rooms.data.Count;
        GetAllRoomsInTheEventResponse(json);
    }

    private async Task GetAllRoomsInTheEventResponse(GetAllRoomsInTheEventResponse json)
    {
        for (int i = 0; i < json.rooms.data.Count; i++)
        {
            standsInRooms.Add(json.rooms.data[i].id, new List<GetAllStandsInTheRoomData>());
            roomIds.Add(json.rooms.data[i].id);
            GetAllStandsInTheRoomRequestSender(json.rooms.data[i].id);
        }

        AllResponsesDebugger(json.message);

        await TaskUtils.WaitUntil(() => isTaskCompleted);
        Debug.LogWarning("Task Completed video and logos loading..");
        // or as lambda
        //await TaskUtils.WaitUntil(IsConditionTrue);
        // private bool IsConditionTrue()
        // {
        //     return condition;
        // }


        FillLogosAndVideosInStand();
    }

    private void GetAllStandsInTheRoomRequestSender(int number)
    {
        Singleton.Main.NetworkManager.GetAllStandsInTheRoom(number.ToString(), GetAllStandsInTheRoomResponse);
    }

    private void GetAllStandsInTheRoomResponse(string data)
    {
        var json = JsonUtility.FromJson<GetAllStandsInTheRoomResponse>(data);
        standsInRooms[json.roomStands.data[0].room_id] = json.roomStands.data;

        AllResponsesDebugger(json.message);
        taskCondition++;
        if (taskCondition == taskCount)
        {
            isTaskCompleted = true;
        }
    }

    private void FillLogosAndVideosInStand()
    {
        roomIds.Sort();

        for (int i = 0; i < roomIds.Count; i++)
        {
            Debug.LogWarning($"Total stand count{standsInRooms[roomIds[i]].Count}");
            Debug.LogWarning($"Room ID{roomIds[i]}");
            Debug.LogWarning($"Total Room Count{roomIds.Count}");
            for (int j = 0; j < standsInRooms[roomIds[i]].Count; j++)
            {
                StartCoroutine(GetTexture(standsInRooms[roomIds[i]][j].level,
                    "http://192.168.1.116:8000/uploads/image.png", i));
                // ChangeVideoPlayersUrl(standsInRooms[roomIds[i]][j].level, standsInRooms[roomIds[i]][j].video, i);
                Debug.LogWarning($"{standsInRooms[roomIds[i]][j].level}.  standın video ve logo url'si değiştirildi.");
                Debug.LogWarning($"Total Giriş debugta");
            }
        }
    }

    private void ChangeVideoPlayersUrl(int level, string videoURL, int roomNumber)
    {
        rooms[roomNumber].ChangeVideo(level, videoURL);
    }

    IEnumerator GetTexture(int level, string logo_url, int roomNumber)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(logo_url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height),
                new Vector2(.5f, .5f), 10f);
            rooms[roomNumber].ChangeImage(level, sprite);
        }
    }

    #endregion


    private void AllResponsesDebugger(string message)
    {
        Debug.LogWarning(message);
    }
}