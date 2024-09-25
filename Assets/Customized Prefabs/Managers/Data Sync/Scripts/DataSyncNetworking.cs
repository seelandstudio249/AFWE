using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSyncNetworking : ManagerNetworkingBaseScript {
    [ServerRpc(RequireOwnership = false)]
    public void AssignValueServer(DataStructure[] dataList) {
        ((DataSyncFunctions)managerScript).dataList = dataList;
        AssignValueObserver(dataList);
        ((DataSyncFunctions)managerScript).UpdateDataReceivedText();
    }

    [ObserversRpc(BufferLast = true)]
    private void AssignValueObserver(DataStructure[] dataList) {
        ((DataSyncFunctions)managerScript).dataList = dataList;
        ((DataSyncFunctions)managerScript).UpdateDataReceivedText();
    }
}
