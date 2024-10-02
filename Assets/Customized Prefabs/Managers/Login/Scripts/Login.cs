using FishNet;
using FishNet.Discovery;
using FishNet.Managing;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(ManagersControl))]
public class Login : ManagerBaseScript {
    List<string> _endPoints = new List<string>();
    List<List<string>> splittedEndPoints = new List<List<string>>();

    #region UI
    [Header("Mode Selection")]
    [SerializeField] GameObject modePanel;
    [SerializeField] MRButtonClass singlePlayerModeButton;
    [SerializeField] MRButtonClass multiplayerModeButton;

    [Header("Login Panel")]
    [SerializeField] GameObject loginPanel;
    //[SerializeField] MRButtonClass hostServerButton;
    [SerializeField] MRButtonClass loginButton;
    [SerializeField] MRTKTMPInputField usernameInputField;
    [SerializeField] TMP_Text errorText;

    [Header("Loading Panel")]
    [SerializeField] GameObject loadingPanel;

    [Header("Ip Address Selection")]
    [SerializeField] GameObject roomPanel;
    [SerializeField] MRButtonClass[] joinRoomButtons;

    [Header("Error Panel")]
    [SerializeField] GameObject errorPanel;
    [SerializeField] MRButtonClass searchAgainButton;
    [SerializeField] MRButtonClass cancelSearchingButton;
    [Space(15)]
    #endregion

    int currentPage = 0;

    [Header("Networking Setup")]
    [SerializeField] bool isServer = false;
    [SerializeField] bool isDirectJoinServer = false;
    #region For Network Discovery
    [SerializeField] NetworkDiscovery networkDiscovery;
    [SerializeField] ushort port;
    public ushort GetPort() {
        return port;
    }
    [SerializeField] float searchTimeout;
    public float GetSearchTimeout() {
        return searchTimeout;
    }
    [SerializeField] string secret;
    public string GetSecret() {
        return secret;
    }
    #endregion
    [Space(15)]

    [Header("Game Mode")]
    public GamePlayType gameModeLocal;

    [Header("Player Type")]
    public PlayerType playerType;

    public static Login instance;

    ManagersControl managerControl;

    protected override void Awake() {
        if (instance == null) { instance = this; }

        managerControl = GetComponent<ManagersControl>();
        networkDiscovery.ServerFoundCallback += AddingEndPoint;

        singlePlayerModeButton.button.OnClicked.AddListener(delegate {
            PanelActivation(modePanel, false);
            SinglePlayer();
        });
        multiplayerModeButton.button.OnClicked.AddListener(delegate {
            PanelActivation(loginPanel, true);
        });
        //hostServerButton.button.OnClicked.AddListener(delegate {
        //    StartServer();
        //});
        loginButton.button.OnClicked.AddListener(delegate {
            errorText.text = "";
            loginButton.button.gameObject.SetActive(false);
            switch (usernameInputField.text.ToUpper()) {
                case "MT":
                case "FO":
                case "E":
                SearchServer();
                break;
                default:
                errorText.text = "User doesn't exist";
                loginButton.button.gameObject.SetActive(true);
                break;
            }
            //playerType = PlayerType.MT;
            //SearchServer();
        });
        searchAgainButton.button.OnClicked.AddListener(delegate {
            SearchServer();
        });
        cancelSearchingButton.button.OnClicked.AddListener(delegate {
            PanelActivation(modePanel, true);
        });
    }

    private void Start() {
        InitialSetup();
    }

    #region Mode Selection
    #region Multiplayer
    void InitialSetup() {
        foreach (MRButtonClass child in joinRoomButtons) {
            child.button.gameObject.SetActive(false);
        }
        if (isServer) {
            StartServer();
        } else {
            PanelActivation(loginPanel, true);
        }
    }

    void StartServer() {
        _endPoints.Clear();
        splittedEndPoints.Clear();
        InstanceFinder.ServerManager.StartConnection(port);
        networkDiscovery.AdvertiseServer();
        PanelActivation(modePanel, false);
        managerControl.AssignGameMode(GamePlayType.Multiplayer);
    }

    void SearchServer() {
        networkDiscovery.SearchForServers();
        //PanelActivation(loadingPanel);
        errorText.text = "Joining...";
        _endPoints.Clear();
        splittedEndPoints.Clear();
        gameModeLocal = GamePlayType.Multiplayer;
    }

    public void FailedServerSearch() {
        //PanelActivation(errorPanel);
        errorText.text = "Failed To Connect To Server";
        loginButton.button.gameObject.SetActive(true);
    }

    void PanelActivation(GameObject panel = null, bool activationStatus = true) {
        modePanel.SetActive(false);
        errorPanel.SetActive(false);
        loginPanel.SetActive(false);
        loadingPanel.SetActive(false);
        roomPanel.SetActive(false);
        if (panel) panel.SetActive(activationStatus);
    }

    void AddingEndPoint(IPEndPoint endPoint) {
        if (_endPoints.Contains(endPoint.Address.ToString())) return;
        _endPoints.Add(endPoint.Address.ToString());
        splittedEndPoints = ListSplit(_endPoints);
        if (isDirectJoinServer) {
            if (splittedEndPoints[currentPage][0] != null) JoinServer(splittedEndPoints[currentPage][0]);
            PanelActivation(null);
        } else {
            StartCoroutine(AssigningButton(0));
        }
    }

    IEnumerator AssigningButton(float secondsToWait) {
        yield return new WaitForSeconds(secondsToWait);
        foreach (MRButtonClass child in joinRoomButtons) {
            child.button.gameObject.SetActive(false);
        }
        if (splittedEndPoints.Count > 0 && splittedEndPoints[currentPage].Count > 0) {
            for (int i = 0; i < splittedEndPoints[currentPage].Count; i++) {
                string ipAddress = splittedEndPoints[currentPage][i];
                joinRoomButtons[i].button.gameObject.SetActive(true);
                joinRoomButtons[i].buttonText.text = ipAddress;
                joinRoomButtons[i].button.OnClicked.AddListener(() => {
                    JoinServer(ipAddress);
                    PanelActivation(roomPanel, false);
                });
                PanelActivation(roomPanel);
            }
        }
    }

    void JoinServer(string ipAddress) {
        InstanceFinder.ClientManager.StartConnection(ipAddress, port);
        networkDiscovery.isIpButtonPressed = true;
        networkDiscovery.StopSearchingOrAdvertising();
        managerControl.AssignGameMode(GamePlayType.Multiplayer);
    }
    #endregion

    #region Singleplayer
    void SinglePlayer() {
        gameModeLocal = GamePlayType.Singleplayer;
        managerControl.AssignGameMode(GamePlayType.Singleplayer);
    }
    #endregion
    #endregion

    private List<List<string>> ListSplit(List<string> itemList) {
        if (itemList.Count <= 0) return null;
        int numberOfGroups = itemList.Count / joinRoomButtons.Length;
        List<List<string>> itemsData = new List<List<string>>();
        for (int i = 0; i < numberOfGroups; i++) {
            int startIndex = i * joinRoomButtons.Length;
            List<string> group = itemList.GetRange(startIndex, joinRoomButtons.Length);
            itemsData.Add(group);
        }
        if (itemList.Count % joinRoomButtons.Length != 0) {
            int startIndex = numberOfGroups * joinRoomButtons.Length;
            int remainingItems = itemList.Count - startIndex;
            List<string> remainingGroup = itemList.GetRange(startIndex, remainingItems);
            itemsData.Add(remainingGroup);
        }
        return itemsData;
    }
}
