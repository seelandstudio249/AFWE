using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using UnityEngine;

public class WhisperingManager : ManagerNetworkingBaseScript {
    //[SerializeField] Transform userButtonsHolder;
    [SerializeField] MRButtonClass[] userButtons;
    List<List<PlayerData>> splitDataList = new List<List<PlayerData>>();
    int currentPage = 0;

    protected override void Awake() {
        base.Awake();
        serverDetectPlayerJoin += NewPlayerButton;
    }

    protected override void OnClientConnectionStateChanged(NetworkConnection fishNetConnection, RemoteConnectionStateArgs newState) {
        base.OnClientConnectionStateChanged(fishNetConnection, newState);
        if (newState.ConnectionState == RemoteConnectionState.Stopped) {
            NewPlayerButton();
        }
    }

    void NewPlayerButton() {
        foreach (MRButtonClass button in userButtons) {
            button.button.gameObject.SetActive(false);
        }

        splitDataList = ListChunker.SplitList(playersList, userButtons.Length);

        for (int i = 0; i < splitDataList[currentPage].Count; i++) {
            userButtons[i].button.gameObject.SetActive(true);
            userButtons[i].buttonText.text = splitDataList[currentPage][i].playerId.ToString();
            userButtons[i].button.OnClicked.AddListener(delegate {
                SendingSignalToSpecificClient(splitDataList[currentPage][i].clientConnection);
            });
        }
    }
}
