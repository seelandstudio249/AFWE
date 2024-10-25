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
		int diff = DateTimeOffset.Compare(_qrCode.LastDetectedTime, QRCodesManager.instance.startScanningTime);
		if (diff >= 0) {
			//if (!QRCodesManager.instance.haveLogin) {
			if (_qrCode.Data.ToString() == QRCodesManager.instance.qrCodeString) {
				StartCoroutine(ShowContent(_transform));
				QRCodesManager.instance.haveLogin = true;
			}
		 //}
		 else {
				foreach (var item in QRCodesManager.instance.specificItem) {
					if (_qrCode.Data.ToString() == item.qrCodeString) {
						foreach (var panel in item.targetPanel) {
							if (panel != null) {
								panel.SetActive(true);
							}
						}
						QRCodesManager.instance.StopQRTrackingWithoutCountdown();
						break;
					}
				}
			}
		}
	}

	public IEnumerator ShowContent(Transform _anchorLocation) {
		if (!isMoved) {
			QRCodesManager.instance.containerGameObject.transform.localPosition = _anchorLocation.position;
			QRCodesManager.instance.containerGameObject.transform.rotation = _anchorLocation.rotation;
			QRCodesManager.instance.containerGameObject.transform.Rotate(QRCodesManager.instance.qrRotationOffset, Space.Self);
			yield return new WaitForSeconds(1);
			isMoved = true;
			QRCodesManager.instance.StopQRTracking();
		}
	}
}
