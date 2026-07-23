// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.QR;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public static class QRCodeEventArgs {
	public static QRCodeEventArgs<TData> Create<TData>(TData data) {
		return new QRCodeEventArgs<TData>(data);
	}
}

[Serializable]
public class QRCodeEventArgs<TData> : EventArgs {
	public TData Data { get; private set; }

	public QRCodeEventArgs(TData data) {
		Data = data;
	}
}

[Serializable]
public class QRCodeTargetItem {
	public string qrCodeString;
	public GameObject[] targetPanel;
	public UnityEvent functionsToRun;
}

public class QRCodesManager : ManagerBaseScript {
	[Tooltip("Offset of the rotation of ContentContainer transform on top of QR code")]
	public Vector3 qrRotationOffset;

	public bool IsTrackerRunning { get; private set; }

	public bool IsSupported { get; private set; }

	public event EventHandler<bool> QRCodesTrackingStateChanged;
	public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeAdded;
	public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeUpdated;
	public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeRemoved;

	private System.Collections.Generic.SortedDictionary<System.Guid, Microsoft.MixedReality.QR.QRCode> qrCodesList = new SortedDictionary<System.Guid, Microsoft.MixedReality.QR.QRCode>();

	private QRCodeWatcher qrTracker;
	private bool capabilityInitialized = false;
	private QRCodeWatcherAccessStatus accessStatus;
	private System.Threading.Tasks.Task<QRCodeWatcherAccessStatus> capabilityTask;

	#region Brandon Custom Behavior
	public GameObject qrScannerIndicatorPrefab;
	private GameObject qrCodeScanner;
	public QRCodeScannerIndicator scannerIndicator;
	public string centerPointQrCodeString;
	public string currentQrCodeString;
	public DateTimeOffset startScanningTime;
	bool timeSet = false;
	[SerializeField] GameObject[] objsToActiveAfterScan;
	public GameObject containerGameObject;
	public bool haveLogin = false;
	public QRCodeTargetItem[] specificItem;

	public GameObject qrCodeErrorPanel;
	public TMP_Text errorCode;
	public MRButtonClass retryButton;
	public static QRCodesManager instance;
	public PageManager pageManager;
	#endregion

	public System.Guid GetIdForQRCode(string qrCodeData) {
		lock (qrCodesList) {
			foreach (var ite in qrCodesList) {
				if (ite.Value.Data == qrCodeData) {
					return ite.Key;
				}
			}
		}
		return new System.Guid();
	}

	public System.Collections.Generic.IList<Microsoft.MixedReality.QR.QRCode> GetList() {
		lock (qrCodesList) {
			return new List<Microsoft.MixedReality.QR.QRCode>(qrCodesList.Values);
		}
	}
	protected override void Awake() {
		base.Awake();
		instance = this;
		retryButton.button.OnClicked.AddListener(delegate {
			qrCodeErrorPanel.SetActive(false);
			StartQRTracking();
		});
	}

	// Use this for initialization
	async protected virtual void Start() {
		IsSupported = QRCodeWatcher.IsSupported();
		capabilityTask = QRCodeWatcher.RequestAccessAsync();
		accessStatus = await capabilityTask;
		capabilityInitialized = true;
		StartCoroutine(PrepareToStartScanning());
	}

	IEnumerator PrepareToStartScanning() {
		yield return new WaitForSeconds(1.5f);
		StartQRTracking();
	}

	private void SetupQRTracking() {
		try {
			qrTracker = new QRCodeWatcher();
			IsTrackerRunning = false;
			qrTracker.Added += QRCodeWatcher_Added;
			qrTracker.Updated += QRCodeWatcher_Updated;
			qrTracker.Removed += QRCodeWatcher_Removed;
			qrTracker.EnumerationCompleted += QRCodeWatcher_EnumerationCompleted;
		} catch (Exception ex) {
		}
	}

	public void StartQRTracking() {
		if (!qrCodeScanner) {
			qrCodeScanner = Instantiate(qrScannerIndicatorPrefab, Camera.main.transform);
			scannerIndicator = qrCodeScanner.GetComponent<QRCodeScannerIndicator>();
		}

		if (!containerGameObject) {
			Debug.LogError("No Container");
		}

		scannerIndicator.ObjectActivation(scannerIndicator.hintText.gameObject, true);
		scannerIndicator.ObjectActivation(scannerIndicator.spriteIndicator, true);
		timeSet = true;
		if (timeSet) {
			startScanningTime = DateTimeOffset.UtcNow;
			timeSet = false;
		}
		if (qrTracker != null && !IsTrackerRunning) {
			try {
				qrTracker.Start();
				IsTrackerRunning = true;
				QRCodesTrackingStateChanged?.Invoke(this, true);
			} catch (Exception ex) {

			}
		}
	}

	public void StopQRTracking() {
		if (scannerIndicator == null) return;
		scannerIndicator.ObjectActivation(scannerIndicator.loadingStatusText.gameObject, true);
		if (IsTrackerRunning) {
			IsTrackerRunning = false;
			if (qrTracker != null) {
				qrTracker.Stop();
				qrCodesList.Clear();
			}

			var handlers = QRCodesTrackingStateChanged;
			if (handlers != null) {
				handlers(this, false);
			}
		}
		StartCoroutine(IncreaseOverTime(0.5f));
	}

	public void StopQRTrackingWithoutCountdown() {
		if (scannerIndicator == null) return;
		scannerIndicator.ObjectActivation(scannerIndicator.loadingStatusText.gameObject, true);
		if (IsTrackerRunning) {
			IsTrackerRunning = false;
			if (qrTracker != null) {
				qrTracker.Stop();
				qrCodesList.Clear();
			}

			var handlers = QRCodesTrackingStateChanged;
			if (handlers != null) {
				handlers(this, false);
			}
		}
		scannerIndicator.ObjectActivation(scannerIndicator.loadingStatusText.gameObject, false);
		scannerIndicator.ObjectActivation(scannerIndicator.spriteIndicator.gameObject, false);
		scannerIndicator.ObjectActivation(scannerIndicator.hintText.gameObject, false);
	}

	private void QRCodeWatcher_Removed(object sender, QRCodeRemovedEventArgs args) {
		bool found = false;
		lock (qrCodesList) {
			if (qrCodesList.ContainsKey(args.Code.Id)) {
				qrCodesList.Remove(args.Code.Id);
				found = true;
			}
		}
		if (found) {
			var handlers = QRCodeRemoved;
			if (handlers != null) {
				handlers(this, QRCodeEventArgs.Create(args.Code));
			}
		}
	}

	private void QRCodeWatcher_Updated(object sender, QRCodeUpdatedEventArgs args) {
		bool found = false;
		lock (qrCodesList) {
			if (qrCodesList.ContainsKey(args.Code.Id)) {
				found = true;
				qrCodesList[args.Code.Id] = args.Code;
			}
		}
		if (found) {
			var handlers = QRCodeUpdated;
			if (handlers != null) {
				handlers(this, QRCodeEventArgs.Create(args.Code));
			}
		}
	}

	private void QRCodeWatcher_Added(object sender, QRCodeAddedEventArgs args) {
		lock (qrCodesList) {
			qrCodesList[args.Code.Id] = args.Code;
		}
		var handlers = QRCodeAdded;
		if (handlers != null) {
			handlers(this, QRCodeEventArgs.Create(args.Code));
		}
	}

	private void QRCodeWatcher_EnumerationCompleted(object sender, object e) {

	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			StopQRTracking();
		}

		if (qrTracker == null && capabilityInitialized && IsSupported) {
			if (accessStatus == QRCodeWatcherAccessStatus.Allowed) {
				SetupQRTracking();
			}
		}

		if (Input.GetKeyDown(KeyCode.F1)) {
			pageManager.stepByStepGuidePumpShutdown.QrScanned("P3-201C");
			//foreach (var item in specificItem) {
			//	foreach (GameObject itemPanel in item.targetPanel) {
			//		itemPanel.SetActive(true);
			//	}
			//	item.functionsToRun.Invoke();
			//}
			StopQRTrackingWithoutCountdown();
		}

		if (Input.GetKeyDown(KeyCode.F2)) {
			pageManager.stepByStepGuideElectricianIsolation.QrScanned("NG3108");
			//foreach (var item in specificItem) {
			//	foreach (GameObject itemPanel in item.targetPanel) {
			//		itemPanel.SetActive(true);
			//	}
			//	item.functionsToRun.Invoke();
			//}
			StopQRTrackingWithoutCountdown();
		}

		if(Input.GetKeyDown(KeyCode.F3)) {
			pageManager.stepByStepGuideLubOilChange.QrScanned("Lube Oil Change");
			StopQRTrackingWithoutCountdown();
		}
	}

	public void ObjecsActivation(bool toActive) {
		foreach (GameObject obj in objsToActiveAfterScan) {
			obj.SetActive(toActive);
		}
		scannerIndicator.ObjectActivation(scannerIndicator.loadingStatusText.gameObject, false);
		scannerIndicator.ObjectActivation(scannerIndicator.spriteIndicator.gameObject, false);
		scannerIndicator.ObjectActivation(scannerIndicator.hintText.gameObject, false);
	}

	IEnumerator IncreaseOverTime(float duration) {
		float elapsedTime = 0f;
		float startValue = 0f;
		float endValue = 100f;

		while (elapsedTime < duration) {
			float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
			scannerIndicator.loadingStatusText.text = Mathf.RoundToInt(currentValue).ToString() + "%";
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		scannerIndicator.loadingStatusText.text = Mathf.RoundToInt(endValue).ToString() + "%";
		ObjecsActivation(true);
	}

	public void LogoutCalled() {
		scannerIndicator.ObjectActivation(scannerIndicator.loadingStatusText.gameObject, false);
		scannerIndicator.ObjectActivation(scannerIndicator.spriteIndicator.gameObject, false);
		scannerIndicator.ObjectActivation(scannerIndicator.hintText.gameObject, false);
	}
}

