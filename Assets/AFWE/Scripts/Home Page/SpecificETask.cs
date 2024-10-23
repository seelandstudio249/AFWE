using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificETask : MonoBehaviour {
	#region Specific E Task Page
	[Header("Specific E Task Settings")]
	public GameObject specificETaskPanel;
	[SerializeField] MRButtonClass returnFromSpecificETaskButton;
	[SerializeField] MRButtonClass confirmETaskButton;
	#endregion

	ETaskInfo specificETaskInfo;

	PageManager pageManager;
	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromSpecificETaskButton.button.OnClicked.AddListener(delegate {
			pageManager.PanelActivation(pageManager.eTaskPanel.eTaskPanel);
		});

		confirmETaskButton.button.OnClicked.AddListener(delegate {
			// Need to send API
			pageManager.PanelActivation(pageManager.eTaskPanel.eTaskPanel);
			specificETaskInfo.eTaskStatus = true;
			pageManager.eTaskPanel.UpdateSelectedETask(specificETaskInfo);
		});
	}

	public void AssignDummyDataSpecificETask(ETaskInfo specificETaskInfo) {
		this.specificETaskInfo = specificETaskInfo;
	}
}
