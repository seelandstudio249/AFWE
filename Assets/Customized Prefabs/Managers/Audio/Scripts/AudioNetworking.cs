using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioNetworking : ManagerNetworkingBaseScript {
    [ServerRpc(RequireOwnership = false)]
    public void PlayStopAudioServer(int index, AudioType audioType) {
        ((AudioFunction)managerScript).ServerTurnOnOffAudio(index, audioType);
        PlayStopAudioObserver(index, audioType);
    }

    [ObserversRpc(BufferLast = true)]
    private void PlayStopAudioObserver(int index, AudioType audioType) {
        ((AudioFunction)managerScript).ServerTurnOnOffAudio(index, audioType);
    }
}
