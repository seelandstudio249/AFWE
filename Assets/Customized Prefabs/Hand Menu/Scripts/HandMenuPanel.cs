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

    protected override void Awake() {
        base.Awake();
        qrCodesManager = managerControlScript.GetSpecificManagerScript<QRCodesManager>();
        palmStatus = GetComponent<PalmUpChecker>();
        mrtkPalmUp = GetComponent<HandConstraintPalmUp>();
		mrtkPalmUp.enabled = false;
		scanQRButton.button.OnClicked.AddListener(delegate {
            if (scanQRButton.buttonText.text == "Scan QR") {
                scanQRButton.buttonText.text = "Stop Scanning";
                qrCodesManager.StopQRTracking();
            } else {
                scanQRButton.buttonText.text = "Scan QR";
                qrCodesManager.StartQRTracking();
            }
        });
        enterEditModeButton.button.OnClicked.AddListener(delegate {
            if (enterEditModeButton.buttonText.text == "Enter Edit Mode") {
                enterEditModeButton.buttonText.text = "Exit Edit Mode";
            } else {
                enterEditModeButton.buttonText.text = "Enter Edit Mode";
            }
        });
    }

	protected override void AssignGamePlayType(GamePlayType gameMode) {
		base.AssignGamePlayType(gameMode);
        mrtkPalmUp.enabled = true;
	}
}
