using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsActivationNetworking : ManagerNetworkingBaseScript {
    [ServerRpc(RequireOwnership = false)]
    public void SettingObjectsListStatusServer(List<ObjectsActivationStatus> statusList) {
        ((ObjectsActivationFunctions)managerScript).objectStatusList = statusList;
        SettingObjectsListStatusObserver(((ObjectsActivationFunctions)managerScript).objectStatusList);
        ((ObjectsActivationFunctions)managerScript).ActivatingObject();
    }

    [ObserversRpc(BufferLast = true)]
    void SettingObjectsListStatusObserver(List<ObjectsActivationStatus> statusList) {
        ((ObjectsActivationFunctions)managerScript).objectStatusList = statusList;
        ((ObjectsActivationFunctions)managerScript).ActivatingObject();
    }
}
