using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Component.Transforming;
using System;
using FishNet.Object.Synchronizing;

public class FishNetPlayer : NetworkBehaviour {
    private static FishNetPlayer _instance;
    public delegate void FishNetPlayerDelegate();
    // public FishNetPlayerDelegate PlayerDelegate;

    [Header("General")]
    [SerializeField] private string playerName;
    //[SerializeField] private GameObject crystal;
    [SerializeField] private GameObject deviceName;
    [SerializeField] private Transform localMainCamera;
    [SerializeField] private NetworkTransform networkTransform;

    [SerializeField]
    private TextMeshPro playerNameLabel;
    public readonly SyncVar<NetworkConnection> playerConnection = new();
    public readonly SyncVar<int> playerConnectionID = new();
    // [SyncVar(OnChange = nameof(PlayerUpdateContainer))]
    public Vector3 playerContainerPosition;

    public readonly SyncVar<bool> isPlayerReady = new();
    private void Awake() {
        _instance = this;
        // PlayerDelegate += CallIsPlayerReady;
    }
    public override void OnStartClient() {
        base.OnStartClient();

        //Disable Player's Label and Crystal if isServer
        if (IsServer)
            //if ((IsServer && IsOwner) || (base.Owner.ClientId == 0))
            UpdatePlayerVisualState(false);

        if (IsOwner) {
            localMainCamera = Camera.main.transform;
            UpdatePlayerConnection();
            //QRCodesManager.Instance.confirmPositionButton.OnClicked.AddListener(CallIsPlayerReady);1
            // Need a place to let the server know the users are ready
        }

        PlayerNameTracker.SetName(SystemInfo.deviceName, base.Owner);
        PlayerNameTracker.OnNameChange += PlayerNameTracker_OnNameChange;


    }

    [ServerRpc]
    private void UpdatePlayerConnection() {
        localMainCamera = Camera.main.transform;
        playerConnection.Value = base.Owner;
        playerConnectionID.Value = playerConnection.Value.ClientId;
        playerContainerPosition = GameObject.Find("Container").transform.position;
    }

    public void CallIsPlayerReady() // call when Player Ready
    {
        if (base.IsOwner) {
            Debug.Log("player " + base.OwnerId + " ready!");
            playerContainerPosition = GameObject.Find("Container").transform.position;
            isPlayerReady.Value = true;
            OnUpdatePlayerReady();

        }
    }
    [ServerRpc]
    private void OnUpdatePlayerReady() {
        Debug.Log("player " + base.OwnerId + " ready!");
        playerContainerPosition = GameObject.Find("Container").transform.position;
        isPlayerReady.Value = true;
    }

    public NetworkConnection getPlayerConnection() {
        Debug.Log("Reuqest Get Player Connection ID::" + base.Owner.ClientId);
        return playerConnection.Value;
    }
    public override void OnStopClient() {
        base.OnStopClient();
        PlayerNameTracker.OnNameChange -= PlayerNameTracker_OnNameChange;
    }

    private void PlayerNameTracker_OnNameChange(NetworkConnection arg1, string arg2) {
        if (arg1 != base.Owner)
            return;

        SetName();
    }

    private void OnEnable() {

    }

    private void Start() {

    }

    private void OnDestroy() {

#if !UNITY_WSA

        try {
            //Check Server Camera View
            if (ServerCameraView.Instance.GetCurrentCameraViewTransform() == this.transform) {
                ServerCameraView.Instance.ResetCameraViewToDefault();
            }
        } catch (Exception e) {
            string errorMsg = e.Message;
        }
#endif
    }

    public void UpdatePlayerVisualState(bool _state) {
        //Disable Player's Label and Crystal if isServer
        if (IsServer) {
            //crystal.SetActive(false);
            deviceName.SetActive(false);
            return;
        }

        //crystal.SetActive(_state);
        deviceName.SetActive(_state);

    }

    public void SetName() {
        string result = null;

        if (base.Owner.IsValid)
            result = PlayerNameTracker.GetPlayerName(base.Owner);

        if (string.IsNullOrEmpty(result))
            result = "Unnamed User";

        this.gameObject.name = "PlayerPrefab (" + result + ")";
        playerName = result + " " + base.OwnerId;
        deviceName.GetComponentInChildren<TextMeshPro>().text = playerName;
        syncName(playerName);
    }

    [ServerRpc]
    public void syncName(string name) {
        deviceName.GetComponentInChildren<TextMeshPro>().text = GetName();
    }
    public string GetName() {
        string result = "";

        if (base.Owner.IsValid)
            result = PlayerNameTracker.GetPlayerName(base.Owner) + " " + base.OwnerId;

        return result;
    }

    private void Update() {
        //Crystal rotation
        //crystal.transform.Rotate(0, 0, 20 * Time.deltaTime);

        //Update transform based on local mixed reality camera
        if (localMainCamera != null && base.IsOwner) {
            transform.SetPositionAndRotation(localMainCamera.position, localMainCamera.rotation);
            //transform.SetLocalPositionAndRotation(localMainCamera.localPosition, localMainCamera.localRotation);
        }
    }

}