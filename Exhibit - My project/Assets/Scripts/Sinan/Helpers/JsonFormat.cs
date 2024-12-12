using System;
using System.Collections.Generic;

#region GetRoomToken

[Serializable]
public class RoomToken
{
    public bool success;
    public string token;
}

#endregion

#region FaillCallback

[Serializable]
public class FailCallbackError
{
    public bool success;
    public string msg;
}

#endregion

#region GetVideoLink

[Serializable]
public class GetVideoLinkSuccess
{
    public bool success;
    public string msg;
}

#endregion

#region Register

[Serializable]
public class RegisterResponse
{
    public RegisterData data;
    public string message;
}

[Serializable]
public class RegisterData
{
    public int id;
    public string email;
}

#endregion

#region Login

[Serializable]
public class LoginResponse
{
    public string message;
    public LoginData data;
}

[Serializable]
public class LoginData
{
    public int id;
    public string email;
    public LoginTokens tokens;
    public string username;
}

[Serializable]
public class LoginTokens
{
    public LoginAccess access;
    public LoginRefresh refresh;
}

[Serializable]
public class LoginAccess
{
    public string token;
    public DateTime expires;
}

[Serializable]
public class LoginRefresh
{
    public string token;
    public DateTime expires;
}

#endregion

#region CreateUsername

[Serializable]
public class CreateUsernameResponse
{
    public CreateUsernameData data;
    public string message;
}

[Serializable]
public class CreateUsernameData
{
    public int id;
    public string email;
}

#endregion

#region CreateEvent

[Serializable]
public class CreateEventResponse
{
    public CreateEventData data;
    public string message;
}

[Serializable]
public class CreateEventData
{
    public string uuid;
    public string session_id;
    public bool is_started;
    public int id;
    public string title;
    public DateTime start_date;
    public DateTime end_date;
}

#endregion

#region GetAllEvents

[Serializable]
public class GetAllEventsResponse
{
    public GetAllEventsData data;
    public string message;
}

[Serializable]
public class GetAllEventsData
{
    public List<EventData> eventData;
    public EventsMeta meta;
}

[Serializable]
public class EventData
{
    public int id;
    public string uuid;
    public string title;
    public DateTime start_date;
    public DateTime end_date;
    public string session_id;
    public bool is_started;
}

[Serializable]
public class EventsMeta
{
    public int total;
    public int page;
    public int perPage;
}

#endregion

#region CreateRoom

[Serializable]
public class GameRoomCreateResponse
{
    public User user;
    public RoomCreateTokens tokens;
}

[Serializable]
public class User
{
    public string id;
    public string email;
    public string name;
    public string role;
}

[Serializable]
public class RoomCreateTokens
{
    public RoomCreateAccess access;
    public RoomCreateRefresh refresh;
}

[Serializable]
public class RoomCreateAccess
{
    public string token;
    public DateTime expires;
}

[Serializable]
public class RoomCreateRefresh
{
    public string token;
    public DateTime expires;
}

#endregion

#region GetAllRoomsInTheEvent

[Serializable]
public class GetAllRoomsInTheEventResponse
{
    public Rooms rooms;
    public string message;
}

[Serializable]
public class Rooms
{
    public List<GetRoomData> data;
    public Meta meta;
}

[Serializable]
public class GetRoomData
{
    public int id;
    public int game_event_id;
    public string uuid;
    public string code;
    public bool is_deleted;
    public DateTime created_at;
    public DateTime updated_at;
}


[Serializable]
public class Meta
{
    public int total;
    public int page;
    public int perPage;
}

#endregion

#region GetAllStandsInTheRoom

[Serializable]
public class GetAllStandsInTheRoomResponse
{
    public RoomStands roomStands;
    public string message;
}

[Serializable]
public class RoomStands
{
    public List<GetAllStandsInTheRoomData> data;
    public GetAllStandsInTheRoomMeta meta;
}

[Serializable]
public class GetAllStandsInTheRoomMeta
{
    public int total;
    public int page;
    public int perPage;
}

[Serializable]
public class GetAllStandsInTheRoomData
{
    public int id;
    public int room_id;
    public string name;
    public string description;
    public string logo;
    public string video;
    public bool is_active;
    public bool is_deleted;
    public DateTime created_at;
    public DateTime updated_at;
    public int level;
}

#endregion