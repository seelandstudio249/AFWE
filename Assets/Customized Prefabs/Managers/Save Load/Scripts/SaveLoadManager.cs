using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class SaveLoadManager : ManagerBaseScript {
	public RoomData roomData = new RoomData();

	[Header("Spawned Objects Holder")]
	public GameObject spawnedObjectsHolder;
	[SerializeField] PageManager pageManager;

	[SerializeField] ObjectResponse objectResponse;

	protected override void Awake() {
		base.Awake();
		LoadAllRoomsData();
	}

	void Start() {

	}

	#region Save Data
	RoomData SaveCurrentRoomData() {
		RoomData roomData = new RoomData();
		List<ObjectData> roomObjectsData = new List<ObjectData>();
		foreach (Transform obj in spawnedObjectsHolder.transform) {
			ObjectData data = new ObjectData(obj.transform.name, obj.localPosition, obj.localRotation, obj.localScale);
			roomObjectsData.Add(data);
		}
		roomData.objects = roomObjectsData;
		return roomData;
	}

	public void SaveDataToServer() {
		SaveData(SaveCurrentRoomData());
	}

	public async Task SaveData(RoomData roomData) {
		ApiManager.instance.StartCoroutine(ApiManager.instance.APICallDelete(
			APIDeleteMiddlePath.APIDeleteMiddlePathList[(int)APIDeletePathEnum.ObjectDelete], () => {
				string jsonToSave = JsonUtility.ToJson(roomData, true);
				ApiManager.instance.StartCoroutine(ApiManager.instance.APICallPOST(
						APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.ObjectsCreate],
						jsonToSave,
						() => {
							LoadAllRoomsData();
						}));
			}
			));
	}
	#endregion

	#region Load Data To Setup The Room
	public void LoadAllRoomsData() {
		ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.Objects],
					null, () => { },
					LoadRoom);
	}

	public void LoadRoom(DownloadHandler returnedItems) {
		ObjectResponse retrunedData = JsonUtility.FromJson<ObjectResponse>(returnedItems.text);
		if (objectResponse != null) {
			objectResponse = retrunedData;

			foreach (Objects item in objectResponse.objects) {
				Transform childTransform = spawnedObjectsHolder.transform.Find(item.name);
				if (childTransform) {
					childTransform.localPosition = new Vector3(item.position.x, item.position.y, item.position.z);
					childTransform.localRotation = Quaternion.Euler(item.rotation.x, item.rotation.y, item.rotation.z);
					childTransform.localScale = new Vector3(item.size.x, item.size.y, item.size.z);
				}
			}
		}
	}

	#endregion
	protected override void AfterLoginFunction() {
		base.AfterLoginFunction();
		spawnedObjectsHolder.SetActive(true);
	}
}
