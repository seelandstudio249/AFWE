using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SaveLoadManagerNetworking))]
public class SaveLoadManager : ManagerBaseScript {
	public RoomData roomData = new RoomData();
	string streamingPath = "";

	[Header("Spawned Objects Holder")]
	[SerializeField] GameObject spawnedObjectsHolder;
	[SerializeField] List<GameObject> spawnableObjects;

	void Start() {
		EnsureStreamingAssetsFolderExists();
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
		roomData.objects = roomObjectsData;
		return roomData;
	}

	public void SaveDataToServer() {
		((SaveLoadManagerNetworking)networkingScript).SaveDataServerRpc(SaveCurrentRoomData());
	}

	public async Task SaveData(RoomData roomData) {
		string savePath = streamingPath;
		if (File.Exists(savePath)) {
			string jsonToSave = JsonUtility.ToJson(roomData, true);
			using (StreamWriter writer = new StreamWriter(savePath, false)) {
				await writer.WriteAsync(jsonToSave);
			}
		}
	}
	#endregion

	#region Load Data To Setup The Room
	public void LoadRoom() {
		foreach (ObjectData obj in roomData.objects) {
			((SaveLoadManagerNetworking)networkingScript).SpawnObject(spawnableObjects[obj.ObjectIndex], obj);
		}
	}

	#endregion

	public void LoadAllRoomsData() {
		string loadPath = streamingPath;
		if (File.Exists(loadPath)) {
			string json = File.ReadAllText(loadPath);
			roomData = JsonUtility.FromJson<RoomData>(json);
			//roomData = JsonConvert.DeserializeObject<RoomData>(json);
			//Debug.LogError(roomData);
		}
	}

	public void SetPaths() {
		streamingPath = Path.Combine(Application.streamingAssetsPath, "SaveData.json");
	}

	private void EnsureStreamingAssetsFolderExists() {
		string streamingAssetsFolderPath = Application.streamingAssetsPath;
		if (!Directory.Exists(streamingAssetsFolderPath)) {
			Directory.CreateDirectory(streamingAssetsFolderPath);
		}
	}
}
