using Microsoft.MixedReality.QR;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpatialGraphNodeTracker))]
public class QRCode : MonoBehaviour {
    public Microsoft.MixedReality.QR.QRCode qrCode;
    public float PhysicalSize { get; private set; }
    public string CodeText { get; private set; }

    public bool isDebug;
    bool isMoved = false;

    ManagersControl managersControl;
    QRCodesManager qrCodesManager;

    private void Awake() {
        managersControl = FindObjectOfType<ManagersControl>();
        qrCodesManager = managersControl.GetSpecificManagerScript<QRCodesManager>();
    }

    void Update() {
        CompareQRCode(qrCode, this.transform);
    }

    public void CompareQRCode(Microsoft.MixedReality.QR.QRCode _qrCode, Transform _transform) {
        int diff = DateTimeOffset.Compare(_qrCode.LastDetectedTime, qrCodesManager.startScanningTime);
        if (diff >= 0 && _qrCode.Data.ToString() == qrCodesManager.qrCodeString) {
            StartCoroutine(ShowContent(_transform));
        }
    }

    public IEnumerator ShowContent(Transform _anchorLocation) {
        if(!isMoved) {
            qrCodesManager.containerGameObject.transform.localPosition = _anchorLocation.position;
            qrCodesManager.containerGameObject.transform.rotation = _anchorLocation.rotation;
            qrCodesManager.containerGameObject.transform.Rotate(qrCodesManager.qrRotationOffset, Space.Self);
            yield return new WaitForSeconds(1);
            isMoved = true;
            qrCodesManager.StopQRTracking();
        }
    }
}
