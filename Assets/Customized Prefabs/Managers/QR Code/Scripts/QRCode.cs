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

    void Update() {
        CompareQRCode(qrCode, this.transform);
    }

    public void CompareQRCode(Microsoft.MixedReality.QR.QRCode _qrCode, Transform _transform) {
        int diff = DateTimeOffset.Compare(_qrCode.LastDetectedTime, QRCodesManager.Instance.startScanningTime);
        if (diff >= 0 && _qrCode.Data.ToString() == QRCodesManager.Instance.qrCodeString) {
            StartCoroutine(ShowContent(_transform));
        }
    }

    public IEnumerator ShowContent(Transform _anchorLocation) {
        if(!isMoved) {
            QRCodesManager.Instance.containerGameObject.transform.localPosition = _anchorLocation.position;
            QRCodesManager.Instance.containerGameObject.transform.rotation = _anchorLocation.rotation;
            QRCodesManager.Instance.containerGameObject.transform.Rotate(QRCodesManager.Instance.qrRotationOffset, Space.Self);
            yield return new WaitForSeconds(1);
            isMoved = true;
            QRCodesManager.Instance.StopQRTracking();
        }
    }
}
