using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public enum PlayerType {
    //Host,
    //Client

    MT,
    Electrician,
    FO
}

[Serializable]
public class PlayerData {
    public int playerId;
    public PlayerType playerType;
    public NetworkConnection clientConnection;
}

public class ManagerNetworkingBaseScript : NetworkBehaviour {


    [SerializeField] protected ManagerBaseScript managerScript;
    [SerializeField] protected List<PlayerData> playersList = new List<PlayerData>();

    protected Action serverDetectPlayerJoin;
    protected Action<int> serverDetectPlayerLeft;

    protected virtual void Awake() {
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnClientConnectionStateChanged;
    }

    protected virtual void OnClientConnectionStateChanged(NetworkConnection fishNetConnection, RemoteConnectionStateArgs newState) {
        if (newState.ConnectionState == RemoteConnectionState.Stopped) {
            RemovePlayerFromListServer(fishNetConnection);
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();
        AddingNewPlayerToListServer(new PlayerData {
            playerId = InstanceFinder.ClientManager.Connection.ClientId,
            playerType = Login.instance.playerType,
            clientConnection = InstanceFinder.ClientManager.Connection
        });
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void AddingNewPlayerToListServer(PlayerData playerData) {
        playersList.Add(playerData);
        AddingNewPlayerToListObserver(playersList);
        serverDetectPlayerJoin?.Invoke();
    }

    [ObserversRpc(BufferLast = true)]
    protected virtual void AddingNewPlayerToListObserver(List<PlayerData> playersList) {
        this.playersList = playersList;
        serverDetectPlayerJoin?.Invoke();
    }

    protected virtual void RemovePlayerFromListServer(NetworkConnection fishNetConnection) {
        playersList.Remove(playersList.FirstOrDefault(player => player.playerId == fishNetConnection.ClientId));
        RemovePlayerFromListObserver(playersList, fishNetConnection.ClientId);
        serverDetectPlayerLeft?.Invoke(fishNetConnection.ClientId);
    }

    [ObserversRpc(BufferLast = true)]
    protected virtual void RemovePlayerFromListObserver(List<PlayerData> playersList, int playerId) {
        this.playersList = playersList;
        serverDetectPlayerLeft?.Invoke(playerId);
    }

    protected virtual void SendingSignalToSpecificClient(NetworkConnection reciverConnection) {
        Debug.LogError("You Are Sending A Signal To: " + reciverConnection.ClientId);
        SendingSignalToSpecificClientServer(reciverConnection, InstanceFinder.ClientManager.Connection);
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SendingSignalToSpecificClientServer(NetworkConnection reciverConnection, NetworkConnection senderConnection) {
        SendingSignalToSpecificClientTargetRPC(reciverConnection, InstanceFinder.ClientManager.Connection);
    }

    [TargetRpc]
    protected virtual void SendingSignalToSpecificClientTargetRPC(NetworkConnection reciverConnection, NetworkConnection senderConnection) {
        Debug.LogError("You Received A Signal From: " + senderConnection.ClientId);
    }
}
