using DG.Tweening.Core.Easing;
using Microsoft.MixedReality.QR;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpatialGraphNodeTracker))]
public class QRCode : MonoBehaviour {
	public Microsoft.MixedReality.QR.QRCode qrCode;
	public float PhysicalSize { get; private set; }
	public string CodeText { get; private set; }

	public bool isDebug;
	bool isMoved = false;

	private void Start() { }

	void Update() {
		CompareQRCode(qrCode, this.transform);
	}

	public void CompareQRCode(Microsoft.MixedReality.QR.QRCode _qrCode, Transform _transform) {
		int diff = DateTimeOffset.Compare(_qrCode.LastDetectedTime, QRCodesManager.instance.startScanningTime);
		if (diff >= 0) {
			if (_qrCode.Data.ToString() == QRCodesManager.instance.currentQrCodeString) {
				if (!QRCodesManager.instance.haveLogin) {
					if (_qrCode.Data.ToString() == QRCodesManager.instance.centerPointQrCodeString) {
						//ShowContent(_transform);
						StartCoroutine(ShowContent(_transform));
						//QRCodesManager.instance.CenterPointAssigned.Invoke(_transform);
					}
				} else {
					foreach (var item in QRCodesManager.instance.specificItem) {
						if (_qrCode.Data.ToString() == item.qrCodeString) {
							foreach (var panel in item.targetPanel) {
								if (panel != null) {
									panel.SetActive(true);
								}
							}
							item.functionsToRun.Invoke();

							QRCodesManager.instance.StopQRTrackingWithoutCountdown();
							break;
						}
					}
				}
			}
		//else if (_qrCode.Data.ToString() == QRCodesManager.instance.reCenterString) {
		//QRCodesManager.instance.CenterPointAssigned.Invoke(_transform);
		//} 
		else {
				QRCodesManager.instance.qrCodeErrorPanel.SetActive(true);
				if (QRCodesManager.instance.currentQrCodeString == QRCodesManager.instance.centerPointQrCodeString) {
					QRCodesManager.instance.errorCode.text = "Please scan the login QR code";
				} else {
					QRCodesManager.instance.errorCode.text = "Please scan the QR code for " + QRCodesManager.instance.currentQrCodeString;
				}
				QRCodesManager.instance.StopQRTrackingWithoutCountdown();
			}
		}
	}

	public IEnumerator ShowContent(Transform _anchorLocation) {
	//public void ShowContent(Transform _anchorLocation) {
		if (!isMoved) {
			QRCodesManager.instance.containerGameObject.transform.localPosition = _anchorLocation.position;
			QRCodesManager.instance.containerGameObject.transform.rotation = _anchorLocation.rotation;
			QRCodesManager.instance.containerGameObject.transform.Rotate(QRCodesManager.instance.qrRotationOffset, Space.Self);
			yield return new WaitForSeconds(1);
			isMoved = true;
			QRCodesManager.instance.haveLogin = true;
			QRCodesManager.instance.StopQRTracking();
		}
	}
}
