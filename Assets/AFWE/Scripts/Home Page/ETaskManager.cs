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

	#region Object Pooling 
	private Queue<GameObject> pool = new Queue<GameObject>();

	public GameObject GetButton() {
		if (pool.Count > 0) {
			GameObject pooledButton = pool.Dequeue();
			pooledButton.SetActive(true);
			return pooledButton;
		} else {
			// Instantiate a new button if the pool is empty
			return Instantiate(eTaskItemButtonPrefab, eTaskHolder);
		}
	}

	public void ReturnButton(GameObject button) {
		button.SetActive(false);
		pool.Enqueue(button);
	}
	#endregion
	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromETaskButton.button.OnClicked.AddListener(delegate {
			pageManager.PanelActivation(pageManager.jobPackPanel.jobPackPanel);
		});
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

		// Clear existing buttons by returning them to the pool
		foreach (Transform child in eTaskHolder) {
			GameObject pooledButton = child.gameObject;
			ReturnButton(pooledButton);  // Return to pool
		}

		// Spawn or reuse button prefab from pool based on job packs data
		foreach (ETaskInfo item in eTasksInfo) {
			GameObject button = GetButton();  // Get from pool or instantiate if necessary
			MRTKCustomizedButtonScript mRButtonClass = button.GetComponent<MRTKCustomizedButtonScript>();
			mRButtonClass.buttonClass.buttonText.text = item.eTaskName;

			// Remove previous listeners to avoid duplicating them
			mRButtonClass.buttonClass.button.OnClicked.RemoveAllListeners();

			// Assign button functionality based on player type
			switch (pageManager.managerControlScript.loginScript.playerType) {
				case PlayerType.MT:
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
				// Add E player-specific functionality here if needed
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
