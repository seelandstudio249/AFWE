using FishNet;
using FishNet.Component.Transforming;
using FishNet.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AllRooms {
    public List<RoomData> allrooms = new List<RoomData>();
}

[Serializable]
public class SpawnButton {
    public MRButtonClass mrButton;
    public int objectIndex;
}

[RequireComponent(typeof(SaveLoadManagerNetworking))]
public class SaveLoadManager : ManagerBaseScript {
    [HideInInspector] public AllRooms allRoomsData = new AllRooms();
    List<List<RoomData>> spiltedRooms = new List<List<RoomData>>();
    [SerializeField] int currentPage = 0;
    string currentSelectedRoom;
    string streamingPath = "";

    [Header("Spawned Objects Holder")]
    [SerializeField] GameObject spawnedObjectsHolder;
    [SerializeField] List<GameObject> spawnableObjects;
    [SerializeField] List<GameObject> spawnableObjectsWithNetworking;

    #region UI
    [Header("UI")]
    [SerializeField] SpawnButton[] spawnButtons;
    [SerializeField] MRButtonClass saveButton, loadButton, deleteButton, nextButton, previousButton, refreshButton;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] MRButtonClass[] existingRooms;
    #endregion

    void Start() {
        SetPaths();
        EnsureStreamingAssetsFolderExists();

        foreach (SpawnButton spawnButton in spawnButtons) {
            spawnButton.mrButton.button.OnClicked.AddListener(delegate {
                GameObject spawnedObj = Instantiate(spawnableObjects[spawnButton.objectIndex], spawnedObjectsHolder.transform);
                SaveLoadSpawnedObjectData saveLoadSpawnedObjectData = spawnedObj.GetComponent<SaveLoadSpawnedObjectData>();
                saveLoadSpawnedObjectData.objectIndex = spawnButton.objectIndex;
            });
        }

        saveButton.button.OnClicked.AddListener(delegate {
            if (!string.IsNullOrWhiteSpace(roomNameInputField.text)) {
                ((SaveLoadManagerNetworking)networkingScript).SaveDataServerRpc(SaveCurrentRoomData());
            } else {
                Debug.LogError("Room Name CANNOT BE NULL OR EMPTY!");
            }
        });

        loadButton.button.OnClicked.AddListener(delegate {
            if (currentSelectedRoom != null) {
                LoadRoom(currentSelectedRoom);
            }
        });

        deleteButton.button.OnClicked.AddListener(delegate {
            if (currentSelectedRoom != null) {
                DeleteRoomData(currentSelectedRoom);
                currentSelectedRoom = null;
            }
        });

        refreshButton.button.OnClicked.AddListener(delegate {
            UpdateRoomsList();
        });

        nextButton.button.OnClicked.AddListener(delegate {
            currentPage++;
            if (currentPage >= spiltedRooms.Count) {
                currentPage = 0;
            }
            UpdateRoomsList();
        });

        previousButton.button.OnClicked.AddListener(delegate {
            currentPage--;
            if (currentPage < 0) {
                currentPage = spiltedRooms.Count - 1;
                if (currentPage < 0) {
                    currentPage = 0;
                }
            }
            UpdateRoomsList();
        });
    }

    #region Save Data
    RoomData SaveCurrentRoomData() {
        RoomData roomData = new RoomData();
        List<ObjectData> roomObjectsData = new List<ObjectData>();
        foreach (Transform obj in spawnedObjectsHolder.transform) {
            SaveLoadSpawnedObjectData saveLoadSpawnedObjectData = obj.GetComponent<SaveLoadSpawnedObjectData>();
            ObjectData data = new ObjectData(obj.position, obj.rotation, obj.localScale, saveLoadSpawnedObjectData.objectIndex);
            roomObjectsData.Add(data);
        }
        roomData.RoomName = roomNameInputField.text;
        roomData.objects = roomObjectsData;
        return roomData;
    }

    public async Task SaveData(RoomData roomData) {
        string savePath = streamingPath;
        if (File.Exists(savePath)) {
            string json = File.ReadAllText(savePath);
            allRoomsData = JsonUtility.FromJson<AllRooms>(json);
            int index = allRoomsData.allrooms.FindIndex(r => r.RoomName == roomData.RoomName);
            if (index != -1) {
                allRoomsData.allrooms[index] = roomData;
            } else {
                allRoomsData.allrooms.Add(roomData);
            }
        } else {
            allRoomsData.allrooms.Add(roomData);
        }
        string jsonToSave = JsonUtility.ToJson(allRoomsData, true);
        using (StreamWriter writer = new StreamWriter(savePath, false)) {
            await writer.WriteAsync(jsonToSave);
        }
    }
    #endregion

    #region Delete Data
    public void DeleteRoomData(string roomName) {
        int index = allRoomsData.allrooms.FindIndex(r => r.RoomName == roomName);
        if (index != -1) {
            allRoomsData.allrooms.RemoveAt(index);
            ((SaveLoadManagerNetworking)networkingScript).DeleteRoomServer(allRoomsData);
        }
    }

    public void RemoveRoomFromJSon() {
        string jsonToSave = JsonUtility.ToJson(allRoomsData, true);
        File.WriteAllText(streamingPath, jsonToSave);
    }
    #endregion

    #region Load Data To Setup The Room
    void LoadRoom(string roomName) {
        RoomData selectedRoom = allRoomsData.allrooms.Find(x => x.RoomName == roomName);
        foreach (ObjectData obj in selectedRoom.objects) {
            ((SaveLoadManagerNetworking)networkingScript).SpawnObject(spawnableObjectsWithNetworking[obj.ObjectIndex], obj);
        }
    }

    #endregion

    public void UpdateRoomsList() {
        foreach (MRButtonClass button in existingRooms) {
            button.button.gameObject.SetActive(false);
        }
        if (allRoomsData.allrooms.Count > 0) {
            spiltedRooms = ListChunker.SplitList(allRoomsData.allrooms, existingRooms.Length);
            if (currentPage >= spiltedRooms.Count) {
                currentPage--;
                if (currentPage < 0 && spiltedRooms.Count > 0) {
                    currentPage = 0;
                }
                return;
            }
            int i = 0;
            foreach (MRButtonClass button in existingRooms) {
                button.button.gameObject.SetActive(false);
                if (i < spiltedRooms[currentPage].Count && spiltedRooms[currentPage][i] != null) {
                    button.buttonText.text = spiltedRooms[currentPage][i].RoomName;
                    button.button.OnClicked.AddListener(delegate {
                        currentSelectedRoom = button.buttonText.text;
                    });
                    button.button.gameObject.SetActive(true);
                }
                i++;
            }
        }
    }

    public void LoadAllRoomsData() {
        string loadPath = streamingPath;
        if (File.Exists(loadPath)) {
            string json = File.ReadAllText(loadPath);
            allRoomsData = JsonUtility.FromJson<AllRooms>(json);
        }
    }

    private void SetPaths() {
        streamingPath = Path.Combine(Application.streamingAssetsPath, "SaveData.json");
    }

    private void EnsureStreamingAssetsFolderExists() {
        string streamingAssetsFolderPath = Application.streamingAssetsPath;
        if (!Directory.Exists(streamingAssetsFolderPath)) {
            Directory.CreateDirectory(streamingAssetsFolderPath);
        }
    }
}
