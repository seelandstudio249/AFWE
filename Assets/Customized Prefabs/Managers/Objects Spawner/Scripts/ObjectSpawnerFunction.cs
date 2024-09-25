using FishNet.Object;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class spawnedObjectData {
    public GameObject spawnedObject;
    public Vector3 oriPosition;
}

public class ObjectSpawnerFunction : ManagerBaseScript {
    [Serializable]
    public class ButtonClass {
        public MRButtonClass mrButton;
        public int objIndex;
    }
    public GameObject[] spawnableObjects;
    public Transform objectsHolder;

    public List<spawnedObjectData> spawnedObjects = new List<spawnedObjectData>();

    #region UI
    [Header("UI")]
    [SerializeField] ButtonClass[] buttons;
    #endregion
    protected override void Awake() {
        base.Awake();
        foreach (ButtonClass item in buttons) {
            item.mrButton.button.OnClicked.AddListener(delegate {
                if (gameMode == GamePlayType.Multiplayer) {
                    ((ObjectSpawnerNetworking)networkingScript).SpawnObjectServer(item.objIndex);
                } else {
                    LocalSpawning(item.objIndex);
                }
            });
        }
    }

    public GameObject SpawnObject(int index) {
        return Instantiate(spawnableObjects[index]);
    }

    void LocalSpawning(int index) {
        GameObject obj = Instantiate(spawnableObjects[index], objectsHolder);
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        if (networkObject != null) {
            Destroy(networkObject);
        }
        obj.SetActive(true);
        spawnedObjectData objData = new spawnedObjectData {
            spawnedObject = obj,
            oriPosition = obj.transform.position
        };
        spawnedObjects.Add(objData);
    }

    void LocalDeSpawning() {
        foreach (spawnedObjectData obj in spawnedObjects) {
            Destroy(obj.spawnedObject);
        }
        spawnedObjects.Clear();
    }
}
