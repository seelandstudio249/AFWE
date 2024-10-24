using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuPanel : ManagerBaseScript {
    [SerializeField] MRButtonClass scanQRButton, enterEditModeButton;

    PalmUpChecker palmStatus;
    HandConstraintPalmUp mrtkPalmUp;
    [SerializeField] ManagersControl managerControlScript;
    QRCodesManager qrCodesManager;
    SaveLoadManager saveLoadManager;

    protected override void Awake() {
        base.Awake();
        qrCodesManager = managerControlScript.GetSpecificManagerScript<QRCodesManager>();
		saveLoadManager = managerControlScript.GetSpecificManagerScript<SaveLoadManager>();
		palmStatus = GetComponent<PalmUpChecker>();
        mrtkPalmUp = GetComponent<HandConstraintPalmUp>();
		mrtkPalmUp.enabled = false;
		scanQRButton.button.OnClicked.AddListener(delegate {
            if (scanQRButton.buttonText.text == "Scan QR") {
                scanQRButton.buttonText.text = "Stop Scanning";
                qrCodesManager.StopQRTrackingWithoutCountdown();
            } else {
                scanQRButton.buttonText.text = "Scan QR";
                qrCodesManager.StartQRTracking();
            }
        });
        enterEditModeButton.button.OnClicked.AddListener(delegate {
            if (enterEditModeButton.buttonText.text == "Enter Edit Mode") {
                enterEditModeButton.buttonText.text = "Exit Edit Mode";
                saveLoadManager.SaveDataToServer();

			} else {
                enterEditModeButton.buttonText.text = "Enter Edit Mode";
            }
        });
    }

    protected override void AfterLoginFunction() {
		mrtkPalmUp.enabled = true;

	}
}
