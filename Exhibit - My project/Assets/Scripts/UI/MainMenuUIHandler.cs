using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Fusion.Sockets;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Fusion.Sample.DedicatedServer
{
    public class MainMenuUIHandler : MonoBehaviour, INetworkRunnerCallbacks
    {
        private enum State
        {
            SelectMode,
            ip,
            JoinLobby,
            LobbyJoined,
            Started,
        }

        [Header("Fusion Sample Vars")] [SerializeField]
        private NetworkRunner _runnerPrefab;

        public string _sessionName;
        private string _lobbyName;
        private NetworkRunner _instanceRunner;
        private State _currentState;
        private List<SessionInfo> _currentSessionList;

        [Header("Panels")] public GameObject playerDetailsPanel;
        public GameObject sessionBrowserPanel;
        public GameObject createSessionPanel;
        public GameObject statusPanel;
        private SessionListUIHandler _sessionListUIHandler;

        [Header("New game session")] public TMP_InputField sessionNameInputField;

        [Header("User Auth Register")] [SerializeField]
        private TMP_InputField registerEmailInputField;

        [SerializeField] private TMP_InputField registerPasswordInputField;
        [SerializeField] private TMP_InputField registerConfirmPasswordInputField;

        [Header("User Auth Login")] [SerializeField]
        private TMP_InputField loginEmailInputField;

        [SerializeField] private TMP_InputField loginPasswordInputField;

        [Header("User Auth Username")] [SerializeField]
        private TMP_InputField usernameInputField;

        [Header("Auth Panels")] [SerializeField]
        private GameObject loginPanel;

        [SerializeField] private Toggle _termsAndPrivacyToggleFirst, _termsAndPrivacyToggleSecond;
        [SerializeField] private TMP_Text termsAndPrivacyErrorText;

        [SerializeField] private GameObject registerPanel;
        [SerializeField] private GameObject createUsernamePanel;

        void Start()
        {
            _sessionListUIHandler = FindObjectOfType<SessionListUIHandler>();
            CheckUserExists();
        }

        private void CheckUserExists()
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("email")))
            {
                registerPanel.SetActive(false);
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString("username")))
                {
                    OnFindGameClicked();
                }
                else
                {
                    createUsernamePanel.SetActive(true);
                }
            }
        }

        void HideAllPanels()
        {
            playerDetailsPanel.SetActive(false);
            sessionBrowserPanel.SetActive(false);
            statusPanel.SetActive(false);
        }

        public void OnFindGameClicked()
        {
            _currentState = State.JoinLobby;
            State_JoinLobby();


            Singleton.Main.gameManager.OpenLoadingPanel();

            Singleton.Main.gameManager.PlayerNickName = PlayerPrefs.GetString("username");
            // PlayerPrefs.SetString(PlayerPrefsKeys.PlayerNickname, Singleton.Main.gameManager.PlayerNickName);
            // PlayerPrefs.Save();

            // OnJoinLobby();

            HideAllPanels();
            sessionBrowserPanel.gameObject.SetActive(true);
            FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();
        }

        // public void OnCreateNewGameClicked()
        // {
        //     HideAllPanels();
        //
        //     createSessionPanel.SetActive(true);
        // }

        // public void OnStartNewSessionClicked()
        // {
        //     NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        //
        //     networkRunnerHandler.CreateGame(sessionNameInputField.text, "Playground20");
        //
        //     HideAllPanels();
        //
        //     statusPanel.gameObject.SetActive(true);
        // }

        public void OnJoiningServer()
        {
            HideAllPanels();

            statusPanel.gameObject.SetActive(true);
        }

        public void StartClient()
        {
            State_StartClient();
        }


        async void State_StartClient()
        {
            _instanceRunner = GetRunner("Client");

            _currentState = State.Started;

            var result = await StartSimulation(_instanceRunner, GameMode.Client, _sessionName);

            if (result.Ok == false)
            {
                Debug.LogWarning(result.ShutdownReason);

                _currentState = State.SelectMode;
            }
            else
            {
                Debug.Log("Done");
            }
        }

        async void State_JoinLobby()
        {
            _instanceRunner = GetRunner("Client");

            _currentState = State.LobbyJoined;

            var result = await JoinLobby(_instanceRunner);

            if (result.Ok == false)
            {
                Debug.LogWarning(result.ShutdownReason);

                _currentState = State.SelectMode;
            }
            else
            {
                Debug.Log("Done");
                Singleton.Main.gameManager.CloseLoadingPanel();
            }
        }


        private NetworkRunner GetRunner(string name)
        {
            if (_instanceRunner == null)
            {
                var runner = Instantiate(_runnerPrefab);
                runner.name = name;
                runner.ProvideInput = true;
                runner.AddCallbacks(this);
                return runner;
            }
            else
            {
                return _instanceRunner;
            }
        }

        public Task<StartGameResult> StartSimulation(
            NetworkRunner runner,
            GameMode gameMode,
            string sessionName
        )
        {
            Debug.Log("StartSimulation");

            return runner.StartGame(new StartGameArgs()
                {
                    SessionName = sessionName,
                    GameMode = gameMode,
                    SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                    Scene = SceneManager.GetActiveScene().buildIndex,
                    DisableClientSessionCreation = true,
                }
            );
        }

        public Task<StartGameResult> JoinLobby(NetworkRunner runner)
        {
            return runner.JoinSessionLobby(
                string.IsNullOrEmpty(_lobbyName) ? SessionLobby.ClientServer : SessionLobby.Custom, _lobbyName);
        }


        // ------------ RUNNER CALLBACKS ------------------------------------------------------------------------------------

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _currentSessionList = null;
            _currentState = State.SelectMode;

            // Reload scene after shutdown

            if (Application.isPlaying)
            {
                SceneManager.LoadScene((byte)SceneDefs.GAME);
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            runner.Shutdown();
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            runner.Shutdown();
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        // Other callbacks
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnJoinLobby()
        {
            var clientTask = JoinLobby();
        }

        private async Task JoinLobby()
        {
            Debug.Log("JoinLobby started");

            string lobbyID = "OurLobbyID";

            var result = await _instanceRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

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

        ////////////////Authentification///////////////////////


        #region Authentification

        private void AllResponsesDebugger(string message)
        {
            Debug.Log(message);
        }

        public void RegisterButtonListener()
        {
            if (_termsAndPrivacyToggleSecond.isOn && _termsAndPrivacyToggleFirst.isOn)
            {
                Singleton.Main.NetworkManager.RegisterRequest(registerEmailInputField.text,
                    registerPasswordInputField.text,
                    registerConfirmPasswordInputField.text, ReturnFromRegisterResponse);
                termsAndPrivacyErrorText.text = string.Empty;
                loginPanel.SetActive(true);
            }
            else
            {
                termsAndPrivacyErrorText.text = "Please accept terms and privacy";
            }
        }


        private void ReturnFromRegisterResponse(string data)
        {
            var json = JsonUtility.FromJson<RegisterResponse>(data);
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
            AllResponsesDebugger(json.message);
        }

        public void LoginButtonListener()
        {
            Singleton.Main.NetworkManager.LoginRequest(loginEmailInputField.text, loginPasswordInputField.text,
                ReturnFromLoginResponse);
        }

        private void ReturnFromLoginResponse(string data)
        {
            var json = JsonUtility.FromJson<LoginResponse>(data);
            loginPanel.SetActive(false);
            PlayerPrefs.SetString("email", json.data.email);
            PlayerPrefs.SetString("username", json.data.username);
            PlayerPrefs.Save();
            if (string.IsNullOrEmpty(json.data.username))
            {
                createUsernamePanel.SetActive(true);
            }
            else
            {
                OnFindGameClicked();
            }

            AllResponsesDebugger(json.message);
        }

        public void CreateUsernameButtonListener()
        {
            Singleton.Main.NetworkManager.CreateUsernameRequest(PlayerPrefs.GetString("email"),
                usernameInputField.text,
                ReturnCreateUsernameResponse);
        }

        private void ReturnCreateUsernameResponse(string data)
        {
            var json = JsonUtility.FromJson<CreateUsernameResponse>(data);
            PlayerPrefs.SetString("username", usernameInputField.text);
            OnFindGameClicked();
            AllResponsesDebugger(json.message);
        }

        #endregion
    }
}