using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuPanel : ManagerBaseScript {
	[SerializeField] MRButtonClass resetButton, sprint4TestingButton,restartCameraButton,enterEditModeButton, logoutButton;
	[SerializeField] EditManager editManager;
	[SerializeField] GameObject editPanel;
	RectTransform editPanelTransform;

	PalmUpChecker palmStatus;
	HandConstraintPalmUp mrtkPalmUp;
	//[SerializeField] ManagersControl managerControlScript;
	[SerializeField] PageManager pageManager;
	[SerializeField] SaveLoadManager saveLoadManager;

	private Coroutine myCoroutine;

	protected override void Awake() {
		base.Awake();
		//saveLoadManager = managerControlScript.GetSpecificManagerScript<SaveLoadManager>();
		//palmStatus = GetComponent<PalmUpChecker>();
		//mrtkPalmUp = GetComponent<HandConstraintPalmUp>();
		//mrtkPalmUp.enabled = false;
		editPanelTransform = editPanel.GetComponent<RectTransform>();

		resetButton.button.OnClicked.AddListener(delegate {
			StartCoroutine(ApiManager.instance.APICallPut(
				APIPutMiddlePath.APIPutMiddlePathList[(int)APIPutMiddlePathEnum.MasterReset], null));
		});
		sprint4TestingButton.button.OnClicked.AddListener(delegate {
			StartCoroutine(ApiManager.instance.APICallPut(
				APIPutMiddlePath.APIPutMiddlePathList[(int)APIPutMiddlePathEnum.MasterCriteriaPIC], null));
		});
		//restartCameraButton.button.OnClicked.AddListener(delegate {
		//	Debug.Log("Button Pressed");
		//	pageManager.cameraPanel.RestartCamera();
		//});

		#region On Hold Enter
		enterEditModeButton.button.firstSelectEntered.AddListener(delegate {
			StartCountdownTimer(2, () => {
				if (enterEditModeButton.buttonText.text == "Enter Edit Mode") {
					enterEditModeButton.buttonText.text = "Exit Edit Mode";
					editManager.EnterEditMode();
					editPanel.SetActive(true);
				} else {
					enterEditModeButton.buttonText.text = "Enter Edit Mode";
					saveLoadManager.SaveDataToServer();
					editManager.ExitEditMode();
					editPanel.SetActive(false);
					//pageManager.PanelActivation(pageManager.homePagePanel.homePanel);
				}
			});
		});
		#endregion

		#region On Hold Exit
		enterEditModeButton.button.onSelectExited.AddListener(delegate {
			StopCountdown();
		});
		#endregion
	}

	private void Update() {
		//if (palmStatus.leftHandUp) {
		//	editPanelTransform.localPosition = new Vector3(60, 0, 0);
		//} else {
		//	editPanelTransform.localPosition = new Vector3(-60, 0, 0);
		//}
	}

	protected override void AfterLoginFunction() {
		//mrtkPalmUp.enabled = true;
	}

	IEnumerator StartCountdown(float duration, Action action = null) {
		yield return new WaitForSeconds(duration);
		action?.Invoke();
	}

	void StartCountdownTimer(float duration, Action action = null) {
		// Start the coroutine and store the reference
		myCoroutine = StartCoroutine(StartCountdown(duration, action));
	}

	void StopCountdown() {
		if (myCoroutine != null) {
			StopCoroutine(myCoroutine);
			myCoroutine = null;
		}
	}
}
