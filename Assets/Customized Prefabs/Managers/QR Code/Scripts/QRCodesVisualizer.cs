// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QRCodesVisualizer : MonoBehaviour {
    public GameObject qrCodePrefab;

    public SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
    private bool clearExisting = false;
    struct ActionData {
        public enum Type {
            Added,
            Updated,
            Removed
        };
        public Type type;
        public Microsoft.MixedReality.QR.QRCode qrCode;

        public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this() {
            this.type = type;
            qrCode = qRCode;
        }
    }

    private Queue<ActionData> pendingActions = new Queue<ActionData>();

    ManagersControl managersControl;
    QRCodesManager qrCodeManager;

    private void Awake() {
        managersControl = FindObjectOfType<ManagersControl>();
        qrCodeManager = managersControl.GetSpecificManagerScript<QRCodesManager>();
    }

    // Use this for initialization
    void Start() {
        //Debug.Log("QRCodesVisualizer start");
        qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

        qrCodeManager.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
        qrCodeManager.QRCodeAdded += Instance_QRCodeAdded;
        qrCodeManager.QRCodeUpdated += Instance_QRCodeUpdated;
        qrCodeManager.QRCodeRemoved += Instance_QRCodeRemoved;
        if (qrCodePrefab == null) {
            throw new System.Exception("Prefab not assigned");
        }
    }
    private void Instance_QRCodesTrackingStateChanged(object sender, bool status) {
        if (!status) {
            clearExisting = true;
        }
    }

    public void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e) {
        lock (pendingActions) {
            pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
        }
    }

    public void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e) {
        lock (pendingActions) {
            pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
        }
    }

    public void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e) {
        lock (pendingActions) {
            pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
        }
    }

    private void HandleEvents() {
        lock (pendingActions) {
            while (pendingActions.Count > 0) {
                var action = pendingActions.Dequeue();
                if (action.type == ActionData.Type.Added) {
                    GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                    qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                    qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                } else if (action.type == ActionData.Type.Updated) {
                    if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id)) {
                        GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                        qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                        qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                    }
                } else if (action.type == ActionData.Type.Removed) {
                    if (qrCodesObjectsList.ContainsKey(action.qrCode.Id)) {
                        Destroy(qrCodesObjectsList[action.qrCode.Id]);
                        qrCodesObjectsList.Remove(action.qrCode.Id);
                    }
                }
            }
        }
        if (clearExisting) {
            clearExisting = false;
            foreach (var obj in qrCodesObjectsList) {
                Destroy(obj.Value);
            }
            qrCodesObjectsList.Clear();

        }
    }

    // Update is called once per frame
    void Update() {
        HandleEvents();
    }
}