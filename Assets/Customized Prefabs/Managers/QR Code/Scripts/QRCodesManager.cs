// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.QR;
using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

public class QRCodesManager : ManagerBaseScript {
    [Tooltip("Offset of the rotation of ContentContainer transform on top of QR code")]
    public static QRCodesManager Instance;
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
    #region UI
    [Header("UI")]
    [SerializeField] MRButtonClass startScanningButton;
    [SerializeField] MRButtonClass stopScanningButton;
    #endregion

    public GameObject qrScannerIndicatorPrefab;
    private GameObject qrCodeScanner;
    public string qrCodeString;
    public DateTimeOffset startScanningTime;
    bool timeSet = false;
    [SerializeField] GameObject[] objsToActiveAfterScan;
    public GameObject containerGameObject;
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
        if (Instance == null) {
            Instance = this;
        }

        base.Awake();

        startScanningButton.button.OnClicked.AddListener(delegate {
            StartQRTracking();
        });

        stopScanningButton.button.OnClicked.AddListener(delegate {
            StopQRTracking();
        });
    }

    // Use this for initialization
    async protected virtual void Start() {
        IsSupported = QRCodeWatcher.IsSupported();
        capabilityTask = QRCodeWatcher.RequestAccessAsync();
        accessStatus = await capabilityTask;
        capabilityInitialized = true;
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
            //DebugLog.instance.DebugLogRegular("QRCodesManager : exception starting the tracker " + ex.ToString());
        }
    }

    public void StartQRTracking() {
        if (!qrCodeScanner) {
            qrCodeScanner = Instantiate(qrScannerIndicatorPrefab, Camera.main.transform);
        }

        if (!containerGameObject) {
            Debug.LogError("No Container");
        }

        QRCodeScannerIndicator.instance.ObjectActivation(QRCodeScannerIndicator.instance.spriteIndicator, true);
        timeSet = true;
        if (timeSet) {
            startScanningTime = DateTimeOffset.UtcNow;
            timeSet = false;
        }
        if (qrTracker != null && !IsTrackerRunning) {
            //DebugLog.instance.DebugLogRegular("QR Tracking Started");
            try {
                qrTracker.Start();
                IsTrackerRunning = true;
                QRCodesTrackingStateChanged?.Invoke(this, true);
            } catch (Exception ex) {
                //DebugLog.instance.DebugLogRegular("QRCodesManager starting QRCodeWatcher Exception:" + ex.ToString());
            }
        }
    }

    public void StopQRTracking() {
        if (QRCodeScannerIndicator.instance == null) return;
        QRCodeScannerIndicator.instance.ObjectActivation(QRCodeScannerIndicator.instance.loadingStatusText.gameObject, true);
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
        StartCoroutine(IncreaseOverTime(5));
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
        //Debug.Log("QRCodesManager QrTracker_EnumerationCompleted");
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            StopQRTracking();
        }

        if (qrTracker == null && capabilityInitialized && IsSupported) {
            if (accessStatus == QRCodeWatcherAccessStatus.Allowed) {
                SetupQRTracking();
            } else {
                //DebugLog.instance.DebugLogRegular("Capability access status : " + accessStatus);
            }
        }
    }

    public void ObjecsActivation(bool toActive) {
        foreach (GameObject obj in objsToActiveAfterScan) {
            obj.SetActive(toActive);
        }
        QRCodeScannerIndicator.instance.ObjectActivation(QRCodeScannerIndicator.instance.loadingStatusText.gameObject, false);
        QRCodeScannerIndicator.instance.ObjectActivation(QRCodeScannerIndicator.instance.spriteIndicator.gameObject, false);
    }

    IEnumerator IncreaseOverTime(float duration) {
        float elapsedTime = 0f;
        float startValue = 0f;
        float endValue = 100f;

        while (elapsedTime < duration) {
            float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            QRCodeScannerIndicator.instance.loadingStatusText.text = Mathf.RoundToInt(currentValue).ToString() + "%";
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        QRCodeScannerIndicator.instance.loadingStatusText.text = Mathf.RoundToInt(endValue).ToString() + "%";
        ObjecsActivation(true);
    }
}

