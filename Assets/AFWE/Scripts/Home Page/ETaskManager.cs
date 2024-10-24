using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ETaskManager : MonoBehaviour {
	#region E Tasks
	[Header("E Tasks Settings")]
	public GameObject eTaskPanel;
	[SerializeField] Transform eTaskHolder;
	[SerializeField] GameObject eTaskItemButtonPrefab;
	[SerializeField] MRButtonClass returnFromETaskButton;
	#endregion

	List<ETaskInfo> selectedEtask = new List<ETaskInfo>();

	PageManager pageManager;
	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromETaskButton.button.OnClicked.AddListener(delegate {
			pageManager.PanelActivation(pageManager.jobPackPanel.jobPackPanel);
		});
	}

	private void OnEnable() {
		// Call API to refresh the page
	}

	public void UpdatePanel() {
		if (selectedEtask == null) return;
		List<MRTKCustomizedButtonScript> itemButtonsList = new List<MRTKCustomizedButtonScript>();
		foreach (Transform child in eTaskHolder) {
			// Get the MRTKCustomizedButtonScript component from the child, if it exists
			MRTKCustomizedButtonScript buttonScript = child.GetComponent<MRTKCustomizedButtonScript>();

			if (buttonScript != null) {
				// Add it to the list if the component is found
				itemButtonsList.Add(buttonScript);
			}
		}
		for (int i = 0; i < selectedEtask.Count; i++) {
			itemButtonsList[i].buttonClass.sprite.SetActive(selectedEtask[i].eTaskStatus);
		}
	}

	public void AssignDummyJobPack(List<ETaskInfo> eTasksInfo) {
		selectedEtask = eTasksInfo;
		// Spawn button prefab based on job packs data
		foreach (ETaskInfo item in eTasksInfo) {
			GameObject button = Instantiate(eTaskItemButtonPrefab, eTaskHolder);
			MRTKCustomizedButtonScript mRButtonClass = button.GetComponent<MRTKCustomizedButtonScript>();
			mRButtonClass.buttonClass.buttonText.text = item.eTaskName;

			switch (pageManager.managerControlScript.loginScript.playerType) {
				case PlayerType.MT:
				// Button item on click open up E Task panel
				mRButtonClass.buttonClass.button.OnClicked.AddListener(delegate {
					pageManager.PanelActivation(pageManager.specificETaskPanel.specificETaskPanel);
					pageManager.specificETaskPanel.AssignDummyDataSpecificETask(item);
				});
				break;
				case PlayerType.FO:
				mRButtonClass.buttonClass.button.OnClicked.AddListener(delegate {
					pageManager.PanelActivation(pageManager.equipmentsPanel.equipmentPagePanel);
				});
				break;
				case PlayerType.E:

				break;
			}
		}
	}

	public void UpdateSelectedETask(ETaskInfo updatedETaskInfo) {
		// Loop through each ETaskInfo in selectedEtask
		foreach (var eTask in selectedEtask) {
			// Find the specific task in the list that matches the name
			if (eTask.eTaskName == updatedETaskInfo.eTaskName) {
				// Update the found task with the new status passed in updatedETaskInfo
				eTask.eTaskStatus = updatedETaskInfo.eTaskStatus;

				// If more fields are added in the future, update them here as well
				break;
			}
		}
	}
}
