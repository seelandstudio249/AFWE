using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class DataStructure {
    public string[] expectedDatas;
    public List<string> userData;
}

[Serializable]
public class StoredData {
    public MRButtonClass mrButton;
    public int dataIndex;
    public string dataGiven;
}

[Serializable]
public class PanelDetails {
    public StoredData[] buttons;
}

public class DataSyncFunctions : ManagerBaseScript {
    public DataStructure[] dataList;

    #region UI
    [Header("UI")]
    [SerializeField] PanelDetails[] panels;
    [SerializeField] TMP_Text dataReceivedText;
    #endregion

    protected override void Awake() {
        base.Awake();
        foreach (PanelDetails panel in panels) {
            foreach (StoredData item in panel.buttons) {
                item.mrButton.button.OnClicked.AddListener(delegate { AssignData(item); });
            }
        }
    }

    public void AssignData(StoredData givenData) {
        if (dataList[givenData.dataIndex].userData.Count >= dataList[givenData.dataIndex].expectedDatas.Length) {
            dataList[givenData.dataIndex].userData.Clear();
        }
        dataList[givenData.dataIndex].userData.Add(givenData.dataGiven);

        if (gameMode == GamePlayType.Multiplayer) {
            ((DataSyncNetworking)networkingScript).AssignValueServer(dataList);
        }
        UpdateDataReceivedText();
    }

    public void RemoveAllData() {
        foreach (DataStructure data in dataList) {
            data.userData.Clear();
        }
        ((DataSyncNetworking)networkingScript).AssignValueServer(dataList);
    }

    public void UpdateDataReceivedText() {
        List<string> allUserData = new List<string>();
        foreach (DataStructure data in dataList) {
            allUserData.AddRange(data.userData);
        }
        dataReceivedText.text = string.Join(", ", allUserData);
    }
}
