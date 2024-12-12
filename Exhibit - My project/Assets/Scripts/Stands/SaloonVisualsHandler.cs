using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[Serializable]
public struct Stand
{
    public Image leftImage;
    public Image rightImage;
    public List<VideoPlayer> videoPlayers;
}

public class SaloonVisualsHandler : MonoBehaviour
{
    [Header("                                                                   Images")]
    public List<Stand> stands;

    public int roomMeetingIds;

    public void ChangeImage(int level, Sprite sprite)
    {
        switch (level)
        {
            case 11:
                stands[0].leftImage.sprite = sprite;
                stands[0].rightImage.sprite = sprite;
                break;
            case 12:
                stands[1].leftImage.sprite = sprite;
                stands[1].rightImage.sprite = sprite;
                break;
            case 21:
                stands[2].leftImage.sprite = sprite;
                stands[2].rightImage.sprite = sprite;
                break;
            case 22:
                stands[3].leftImage.sprite = sprite;
                stands[3].rightImage.sprite = sprite;
                break;
            case 31:
                stands[4].leftImage.sprite = sprite;
                stands[4].rightImage.sprite = sprite;
                break;
        }
    }

    public void ChangeVideo(int level, string videoUrl)
    {
        switch (level)
        {
            case 21:
                stands[2].videoPlayers[0].url = videoUrl;
                break;
            case 22:
                stands[3].videoPlayers[0].url = videoUrl;
                break;
            case 31:
                stands[4].videoPlayers[0].url = videoUrl;
                stands[4].videoPlayers[1].url = videoUrl;
                stands[4].videoPlayers[2].url = videoUrl;
                break;
        }
    }
}