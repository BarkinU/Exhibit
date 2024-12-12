using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;


public sealed class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;

    private NetworkRunner _networkRunner;

    private void Awake()
    {
        var networkRunnerInScene = FindObjectOfType<NetworkRunner>();

        //If we already have a network runner in the scene then we should not create another one but rather use the existing one
        if (networkRunnerInScene != null)
            _networkRunner = networkRunnerInScene;
    }

    private void Start()
    {
        if (_networkRunner == null)
        {
            _networkRunner = Instantiate(networkRunnerPrefab);
            _networkRunner.name = "Network runner instantiated";

            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                InitializeNetworkRunner(_networkRunner, GameMode.Server, "TestSession",
                    NetAddress.Any(),
                    SceneManager.GetActiveScene().buildIndex, null);
            }
        }

        Debug.Log($"Server NetworkRunner started:" + _networkRunner.GameMode);
    }


    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        var sceneManager =
            runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault() ??
            //Handle networked objects that already exits in the scene
            runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        return sceneManager;
    }

    private void InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, string sessionName,
        NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        var sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;

        runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            Initialized = initialized,
            SceneManager = sceneManager,
        });
    }

    public void OnJoinLobby()
    {
        // ReSharper disable once UnusedVariable
        var clientTask = JoinLobby();
    }

    private async Task JoinLobby()
    {
        Debug.Log("JoinLobby started");

        string lobbyID = "OurLobbyID";

        var result = await _networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if (!result.Ok)
        {
            Debug.LogError($"Unable to join lobby {lobbyID}");
        }
        else
        {
            Singleton.Main.gameManager.CloseLoadingPanel();
            Debug.Log("JoinLobby ok");
        }
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log(
            $"Create session {sessionName} scene {sceneName} build Index {SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")}");

        //Join existing game as a client
        InitializeNetworkRunner(_networkRunner, GameMode.AutoHostOrClient, sessionName,
            NetAddress.Any(),
            SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}"), null);
    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        Debug.Log($"Join session {sessionInfo.Name}");

        //Join existing game as a client
        InitializeNetworkRunner(_networkRunner, GameMode.Client, sessionInfo.Name,
            NetAddress.Any(), SceneManager.GetActiveScene().buildIndex,
            null);
    }
}