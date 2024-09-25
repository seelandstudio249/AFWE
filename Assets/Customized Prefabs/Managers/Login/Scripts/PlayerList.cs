using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using FishNet.Object;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

public class PlayerList : ManagerNetworkingBaseScript {
    [Serializable]
    protected enum PlayerType {
        Host,
        Client
    }

    [Serializable]
    protected class PlayerClientHostData : PlayerData {
        public PlayerType playerType;
    }
    [SerializeField] protected List<PlayerClientHostData> playersHostClientList = new List<PlayerClientHostData>();

    protected override void Awake() {
        base.Awake();
        serverDetectPlayerJoin += OnClientConnectionStateChanged;
        serverDetectPlayerLeft += RemovePlayersHostClientListServer;
        serverDetectPlayerLeft += delegate {
            OnClientConnectionStateChanged();
        };
    }

    private void OnClientConnectionStateChanged() {
        foreach (PlayerData playerBase in playersList) {
            if (!playersHostClientList.Any(player => player.playerId == playerBase.playerId)) {
                playersHostClientList.Add(new PlayerClientHostData {
                    playerId = playerBase.playerId,
                    playerType = PlayerType.Client
                });
            }
        }
        if (playersList.Count > 0) {
            if (!playersHostClientList.Any(player => player.playerType == PlayerType.Host)) {
                HostChangedServer();
            }
        }
    }

    void HostChangedServer() {
        playersHostClientList[0].playerType = PlayerType.Host;
        HostChangedObserver(playersList[0].playerId);
    }

    [ObserversRpc(BufferLast = true)]
    void HostChangedObserver(int clientId) {
        if (InstanceFinder.ClientManager.Connection.ClientId == clientId) {
            Debug.LogError("You R The Player No 1");
        }
    }

    void RemovePlayersHostClientListServer(int playerId) {
        PlayerClientHostData targetPlayer = playersHostClientList.FirstOrDefault(player => player.playerId == playerId);
        playersHostClientList.Remove(targetPlayer);
    }

    void RemovePlayersHostClientListObserver(List<PlayerClientHostData> playersCHList) {
        playersHostClientList = playersCHList;
    }
}

