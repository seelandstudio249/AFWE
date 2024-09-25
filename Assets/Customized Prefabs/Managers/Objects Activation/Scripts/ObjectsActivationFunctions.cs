using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ObjectsActivationData {
    public List<GameObject> objectsToActive = new List<GameObject>();
}

[Serializable]
public class ObjectsActivationStatus {
    public bool activationStatus;
}

public class ObjectsActivationFunctions : ManagerBaseScript {
    [Serializable]
    public class ButtonClass {
        public MRButtonClass mrButton;
        public int objIndex;
    }

    public List<ObjectsActivationData> objectsList = new List<ObjectsActivationData>();
    public List<ObjectsActivationStatus> objectStatusList = new List<ObjectsActivationStatus>();

    #region UI
    [Header("UI")]
    [SerializeField] ButtonClass[] buttons;
    #endregion

    protected override void Awake() {
        base.Awake();
        foreach (ObjectsActivationData data in objectsList) {
            objectStatusList.Add(new ObjectsActivationStatus());
        }
        foreach (ButtonClass item in buttons) {
            item.mrButton.button.OnClicked.AddListener(delegate { DeactivateAllObjects(); ActivateSpecificObject(item.objIndex); });
        }
    }

    public void ActivateSpecificObject(int i) {
        objectStatusList[i].activationStatus = true;
        if (gameMode == GamePlayType.Multiplayer) {
            ((ObjectsActivationNetworking)networkingScript).SettingObjectsListStatusServer(objectStatusList);
        }
        ActivatingObject();
    }

    public void DeactivateSpecificObject(int i) {
        objectStatusList[i].activationStatus = false;
        if (gameMode == GamePlayType.Multiplayer) {
            ((ObjectsActivationNetworking)networkingScript).SettingObjectsListStatusServer(objectStatusList);
        }
        ActivatingObject();
    }

    public void DeactivateAllObjects() {
        foreach (ObjectsActivationStatus objStatus in objectStatusList) {
            objStatus.activationStatus = false;
        }
        if (gameMode == GamePlayType.Multiplayer) {
            ((ObjectsActivationNetworking)networkingScript).SettingObjectsListStatusServer(objectStatusList);
        }
        ActivatingObject();
    }

    public void ActivatingObject() {
        for (int i = 0; i < objectsList.Count; i++) {
            if (objectStatusList[i].activationStatus == true) {
                foreach (GameObject obj in objectsList[i].objectsToActive) {
                    obj.SetActive(true);
                }
            } else {
                foreach (GameObject obj in objectsList[i].objectsToActive) {
                    obj.SetActive(false);
                }
            }
        }
    }
}
