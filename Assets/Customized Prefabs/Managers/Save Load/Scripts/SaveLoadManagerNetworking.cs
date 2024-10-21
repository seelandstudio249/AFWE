using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SaveLoadManagerNetworking : ManagerNetworkingBaseScript {
    public override void OnStartClient() {
        base.OnStartClient();
        RequestGettingRoomList();
    }


    public override void OnStartServer() {
        base.OnStartServer();
        ((SaveLoadManager)managerScript).LoadAllRoomsData();
    }

    #region Network Request Existing Rooms
    void RequestGettingRoomList() {
        AssignListServer();
    }

    [ServerRpc(RequireOwnership = false)]
    void AssignListServer() {
        AssignListObserver(((SaveLoadManager)managerScript).roomData);
    }

    [ObserversRpc]
    void AssignListObserver(RoomData roomData) {
        ((SaveLoadManager)managerScript).roomData = roomData;
		((SaveLoadManager)managerScript).LoadRoom();
    }
    #endregion

    #region Network Saving
    [ServerRpc(RequireOwnership = false)]
    public void SaveDataServerRpc(RoomData roomData) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        ((SaveLoadManager)managerScript).SaveData(roomData);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        ClientReceiveNewData(((SaveLoadManager)managerScript).roomData);
    }

    [ObserversRpc(BufferLast = true)]
    void ClientReceiveNewData(RoomData allRoomsData) {
        ((SaveLoadManager)managerScript).roomData = allRoomsData;
    }
    #endregion

    [ServerRpc(RequireOwnership = false)]
    public void SpawnObject(GameObject obj, ObjectData objData) {
        GameObject spawnedObj = Instantiate(obj);
        InstanceFinder.ServerManager.Spawn(spawnedObj);
        spawnedObj.transform.localPosition = objData.position;
        spawnedObj.transform.localRotation = objData.rotation;
    }
}
