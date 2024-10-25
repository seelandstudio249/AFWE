using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

//[RequireComponent(typeof(SaveLoadManagerNetworking))]
public class SaveLoadManager : ManagerBaseScript {
	public RoomData roomData = new RoomData();
	string streamingPath = "";

	[Header("Spawned Objects Holder")]
	public GameObject spawnedObjectsHolder;
	[SerializeField] List<GameObject> spawnableObjects;

	protected override void Awake() {
		base.Awake();
		SetPaths();
		LoadAllRoomsData();
	}

	void Start() {
		EnsureStreamingAssetsFolderExists();
	}

	#region Save Data
	RoomData SaveCurrentRoomData() {
		RoomData roomData = new RoomData();
		List<ObjectData> roomObjectsData = new List<ObjectData>();
		foreach (Transform obj in spawnedObjectsHolder.transform) {
			//SaveLoadSpawnedObjectData saveLoadSpawnedObjectData = obj.GetComponent<SaveLoadSpawnedObjectData>();
			ObjectData data = new ObjectData(obj.position, obj.rotation, obj.localScale);
			roomObjectsData.Add(data);
		}
		roomData.objects = roomObjectsData;
		return roomData;
	}

	public void SaveDataToServer() {
		//((SaveLoadManagerNetworking)networkingScript).SaveDataServerRpc(SaveCurrentRoomData());
		SaveData(SaveCurrentRoomData());
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
	//public void LoadRoom() {
	//	//foreach (ObjectData obj in roomData.objects) {
	//	//	SpawnObject(spawnableObjects[obj.ObjectIndex], obj);
	//	//}
	//}

	public void LoadRoom() {
		// Ensure the room data has objects to load and spawnedObjectsHolder has children
		if (roomData.objects == null || roomData.objects.Count == 0) return;

		// Get all children of spawnedObjectsHolder
		int childCount = spawnedObjectsHolder.transform.childCount;

		// Loop through each ObjectData and bind it to the corresponding child object in spawnedObjectsHolder
		for (int i = 0; i < roomData.objects.Count && i < childCount; i++) {
			ObjectData objData = roomData.objects[i];
			Transform childTransform = spawnedObjectsHolder.transform.GetChild(i);

			// Apply saved properties to the child object
			childTransform.localPosition = objData.position;
			childTransform.localRotation = objData.rotation;
			childTransform.localScale = objData.size;
		}
	}

	#endregion

	public void LoadAllRoomsData() {
		string loadPath = streamingPath;
		if (File.Exists(loadPath)) {
			string json = File.ReadAllText(loadPath);
			roomData = JsonUtility.FromJson<RoomData>(json);
		}
		LoadRoom();
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

	//private void SpawnObject(GameObject obj, ObjectData objData) {
	//	GameObject spawnedObj = Instantiate(obj, spawnedObjectsHolder.transform);
	//	spawnedObj.transform.localPosition = objData.position;
	//	spawnedObj.transform.localRotation = objData.rotation;
	//}

	protected override void AfterLoginFunction() {
		base.AfterLoginFunction();
		spawnedObjectsHolder.SetActive(true);
	}
}
