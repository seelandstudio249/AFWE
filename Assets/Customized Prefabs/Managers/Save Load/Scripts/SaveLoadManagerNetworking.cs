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
        ((SaveLoadManager)managerScript).UpdateRoomsList();
    }

    #region Network Request Existing Rooms
    void RequestGettingRoomList() {
        AssignListServer();
    }

    [ServerRpc(RequireOwnership = false)]
    void AssignListServer() {
        AssignListObserver(((SaveLoadManager)managerScript).allRoomsData);
    }

    [ObserversRpc]
    void AssignListObserver(AllRooms roomData) {
        ((SaveLoadManager)managerScript).allRoomsData = roomData;
        ((SaveLoadManager)managerScript).UpdateRoomsList();
    }
    #endregion

    #region Network Saving
    [ServerRpc(RequireOwnership = false)]
    public void SaveDataServerRpc(RoomData roomData) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        ((SaveLoadManager)managerScript).SaveData(roomData);
        ((SaveLoadManager)managerScript).UpdateRoomsList();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        ClientReceiveNewData(((SaveLoadManager)managerScript).allRoomsData);
    }

    [ObserversRpc(BufferLast = true)]
    void ClientReceiveNewData(AllRooms allRoomsData) {
        ((SaveLoadManager)managerScript).allRoomsData = allRoomsData;
        ((SaveLoadManager)managerScript).UpdateRoomsList();
    }
    #endregion

    #region Network Delete Room
    [ServerRpc(RequireOwnership = false)]
    public void DeleteRoomServer(AllRooms allRoomsData) {
        ((SaveLoadManager)managerScript).allRoomsData = allRoomsData;
        DeleteRoomObserver(allRoomsData);
        ((SaveLoadManager)managerScript).RemoveRoomFromJSon();
        ((SaveLoadManager)managerScript).UpdateRoomsList();
    }

    [ObserversRpc]
    public void DeleteRoomObserver(AllRooms allRoomsData) {
        ((SaveLoadManager)managerScript).allRoomsData = allRoomsData;
        ((SaveLoadManager)managerScript).UpdateRoomsList();
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
