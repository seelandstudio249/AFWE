using DG.Tweening.Core.Easing;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PageManager : ManagerBaseScript {
	private string logFilePath;

	public HomePage homePagePanel;
	public JobPackPanel jobPackPanel;
	public JobTaskPanel jobTaskPanel;
	public JobTaskDocument jobTaskDocument;
	public ETaskManager eTaskPanel;
	public SpecificETask specificETaskPanel;
	public EquipmentsPanelPumpShutDown equipmentsPanelPumpShutdown;
	public EquipmentsPanelElectricianIsolation equipmentsPanelElectricianIsolation;
	public EquipmentsPanelLubeOilChange equipmentsPanelLubeOilChange;
	public ToolsPanelLubeOilChange toolsPanelLubeOilChange;
	public StepByStepGuidePumpShutdown stepByStepGuidePumpShutdown;
	public StepByStepGuideElectricianIsolation stepByStepGuideElectricianIsolation;
	public StepByStepGuideLubOilChange stepByStepGuideLubOilChange;
	public StepByStepGuidePhysicalIsolation stepByStepGuidePhysicalIsolation;
	public CallPanel callPanel;
	public CautionPanel cautionPanelPumpShutdown;
	public LoadingPanel loadingPanel;
	public ErrorPanel errorPanel;
	public CameraPanel cameraPanel;
	public AiValidationPanel aiValidationPanel;
	public HazardDetectionManager hazardDetectionManager;
	public HandMenuPanelsManager handMenuPanelsManager;

	public ManagersControl managerControlScript;
	//public MRButtonClass pinButton;

	protected override void Awake() {
		base.Awake();

		AfterLogin += delegate {
			ManagerActivation(true);
			//PanelActivation(homePagePanel.homePanel);
			homePagePanel.ActivateHomePage.Invoke();
		};

		Follow followScript = GetComponent<Follow>();
		logFilePath = Path.Combine(Application.persistentDataPath, "Logs_File.txt");
		Application.logMessageReceived += HandleLog;
	}

	public void PanelActivation(GameObject TargetPanel) {
		if (TargetPanel != loadingPanel.loadingPanel && TargetPanel != errorPanel.errorPanel) {
			homePagePanel.homePanel.SetActive(false);
			jobPackPanel.jobPackPanel.SetActive(false);
			jobTaskPanel.jobTaskPanel.SetActive(false);
			jobTaskDocument.jobTaskDocumentPanel.SetActive(false);
			eTaskPanel.eTaskPanel.SetActive(false);
			specificETaskPanel.specificETaskPanel.SetActive(false);
			equipmentsPanelPumpShutdown.equipmentPagePanel.SetActive(false);
			equipmentsPanelElectricianIsolation.equipmentPagePanel.SetActive(false);
			equipmentsPanelLubeOilChange.equipmentPagePanel.SetActive(false);
			toolsPanelLubeOilChange.equipmentPagePanel.SetActive(false);
			callPanel.callPanel.SetActive(false);
			cameraPanel.cameraPanel.SetActive(false);
			aiValidationPanel.aiValidateStatusPanel.SetActive(false);
		}
		loadingPanel.loadingPanel.SetActive(false);
		errorPanel.errorPanel.SetActive(false);
		TargetPanel?.SetActive(true);
		if (TargetPanel != null) {
			//pinButton.button.gameObject.SetActive(true);
		}
	}

	public void CloseLoadingPanel() {
		loadingPanel.loadingPanel.SetActive(false);
	}

	public void LogoutCalled() {
		PanelActivation(null);
		jobPackPanel.LogoutCalled();
		jobTaskPanel.LogoutCalled();
		jobTaskDocument.LogoutCalled();
		eTaskPanel.LogoutCalled();
		specificETaskPanel.LogoutCalled();
		stepByStepGuidePumpShutdown.LogoutCalled();
		stepByStepGuideElectricianIsolation.LogoutCalled();
		stepByStepGuideLubOilChange.LogoutCalled();
		stepByStepGuidePhysicalIsolation.LogoutCalled();
		equipmentsPanelPumpShutdown.LogoutCalled();
		equipmentsPanelElectricianIsolation.LogoutCalled();
		equipmentsPanelLubeOilChange.LogoutCalled();
		toolsPanelLubeOilChange.LogoutCalled();
		cautionPanelPumpShutdown.LogoutCalled();
		cameraPanel.LogoutCalled();
		aiValidationPanel.LogoutCalled();
		hazardDetectionManager.LogoutCalled();
	}

	public void ErrorTriggered(bool status) {
		managerControlScript.loginScript.UIInteractable(status);
		homePagePanel.UIInteractable(status);
		jobPackPanel.UIInteractable(status);
		jobTaskPanel.UIInteractable(status);
		jobTaskDocument.UIInteractable(status);
		eTaskPanel.UIInteractable(status);
		specificETaskPanel.UIInteractable(status);
		stepByStepGuidePumpShutdown.UIInteractable(status);
		stepByStepGuideElectricianIsolation.UIInteractable(status);
		stepByStepGuideLubOilChange.UIInteractable(status);
		stepByStepGuidePhysicalIsolation.UIInteractable(status);
		equipmentsPanelPumpShutdown.UIInteractable(status);
		equipmentsPanelElectricianIsolation.UIInteractable(status);
		equipmentsPanelLubeOilChange.UIInteractable(status);
		toolsPanelLubeOilChange.UIInteractable(status);
		cautionPanelPumpShutdown.UIInteractable(status);
		cameraPanel.UIInteractable(status);
		aiValidationPanel.UIInteractable(status);
		hazardDetectionManager.UIInteractable(status);
	}

	private void HandleLog(string logString, string stackTrace, LogType type) {
		string logEntry = $"[{System.DateTime.Now}] {type}:\nMessage: {logString}\n";

		if (!string.IsNullOrEmpty(stackTrace)) {
			logEntry += $"StackTrace:\n{stackTrace}\n";

			// Extract the line number and file from the stack trace if possible
			string[] stackLines = stackTrace.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
			if (stackLines.Length > 0) {
				string firstRelevantLine = stackLines[0].Trim();
				logEntry += $"Possible Cause: {firstRelevantLine}\n";
			}
		}

		logEntry += "\n"; // Separate logs visually
		File.AppendAllText(logFilePath, logEntry);

		// Optional: Differentiate between log types
		//switch (type) {
		//	case LogType.Error:
		//	case LogType.Exception:
		//	Debug.LogError(logEntry); // Display in the Unity Console
		//	break;
		//	case LogType.Warning:
		//	Debug.LogWarning(logEntry);
		//	break;
		//	case LogType.Log:
		//	Debug.Log(logEntry);
		//	break;
		//}
	}

	void OnDestroy() {
		Application.logMessageReceived -= HandleLog;
	}
}
