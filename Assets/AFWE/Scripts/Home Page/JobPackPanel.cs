using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JobPackDetails {
	public string jobPackName;
	public List<ETaskInfo> jobPackItems = new List<ETaskInfo>();
}

[Serializable]
public class ETaskInfo {
	public string eTaskName;
	public bool eTaskStatus;
	public List<string> requiredPeople = new List<string>();
}

public class JobPackPanel : MonoBehaviour {
	#region Jobs Pack
	[Header("Jobs Pack Settings")]
	public GameObject jobPackPanel;
	[SerializeField] Transform jobPackHolder;
	[SerializeField] GameObject jobPackItemButtonPrefab;
	[SerializeField] MRButtonClass returnFromJobPackButton;
	#endregion

	#region Dummy Data
	[Header("MT")]
	[SerializeField] List<JobPackDetails> mtJobPackList = new List<JobPackDetails>();

	[Header("FO")]
	[SerializeField] List<JobPackDetails> foJobPackList = new List<JobPackDetails>();
	#endregion

	List<JobPackDetails> selectedJobPackList = new List<JobPackDetails>();

	PageManager pageManager;

	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromJobPackButton.button.OnClicked.AddListener(delegate {
			pageManager.PanelActivation(pageManager.homePagePanel.homePanel);
		});
	}

	public void AssignDummyJobPack() {
		// Need to receive job packs Data
		switch (pageManager.managerControlScript.loginScript.playerType) {
			case PlayerType.MT:
			selectedJobPackList = mtJobPackList;
			break;
			case PlayerType.FO:
			selectedJobPackList = foJobPackList;
			break;
			case PlayerType.E:

			break;
		}

		// Spawn button prefab based on job packs data
		foreach (JobPackDetails item in selectedJobPackList) {
			GameObject button = Instantiate(jobPackItemButtonPrefab, jobPackHolder);
			MRTKCustomizedButtonScript mRButtonClass = button.GetComponent<MRTKCustomizedButtonScript>();
			mRButtonClass.buttonClass.buttonText.text = item.jobPackName;

			// Button item on click open up E Task panel
			mRButtonClass.buttonClass.button.OnClicked.AddListener(delegate {
				pageManager.PanelActivation(pageManager.eTaskPanel.eTaskPanel);
				pageManager.eTaskPanel.AssignDummyJobPack(item.jobPackItems);
			});
		}
	}
}
