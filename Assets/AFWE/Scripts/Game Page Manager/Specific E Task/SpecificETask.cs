using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[Serializable]
public class GetETaskDetailsResponseWrapper {
	public List<ETaskRequirementsResponse> GetETaskDetailsResponseList = new List<ETaskRequirementsResponse>();
}

public class SpecificETask : MonoBehaviour {
	#region Specific E Task Page
	[Header("Specific E Task Settings")]
	public GameObject specificETaskPanel;
	[SerializeField] MRButtonClass returnFromSpecificETaskButton;
	[SerializeField] MRButtonClass refreshButton;
	[SerializeField] Transform itemHolder;
	[SerializeField] GameObject buttonPrefab;
	[SerializeField] Texture2D incompleteButtonBGSprite, completeButtonBGSprite;
	public TMP_Text introductionText;
	#endregion

	[SerializeField] GetETaskDetailsResponseWrapper getETaskDetailsResponseWrapper;
	public string taskNumber = "";
	public string taskOrderNumber = "";
	public string ETaskOrderNumber = "";
	PageManager pageManager;

	ETaskRequirementsResponse pumpShutdownETask;

	#region Object Pooling 
	private Queue<GameObject> pool = new Queue<GameObject>();

	public GameObject GetButton() {
		if (pool.Count > 0) {
			GameObject pooledButton = pool.Dequeue();
			pooledButton.SetActive(true);
			return pooledButton;
		} else {
			// Instantiate a new button if the pool is empty
			return Instantiate(buttonPrefab, itemHolder);
		}
	}

	public void ReturnButton() {
		pool.Clear();

		foreach (Transform child in itemHolder) {
			GameObject pooledButton = child.gameObject;
			SpecificETaskItem mRButtonClass = pooledButton.GetComponent<SpecificETaskItem>();
			mRButtonClass.buttonClass.button.OnClicked.RemoveAllListeners();
			pooledButton.SetActive(false);
			pool.Enqueue(pooledButton);
		}
	}
	#endregion

	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromSpecificETaskButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParamsUserID = new Dictionary<string, string> {
				{"assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() },
				{"order_num", pageManager.eTaskPanel.ETaskOrderNumber }
			};
			ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETask],
					queryParamsUserID,
					() => {
						pageManager.PanelActivation(pageManager.eTaskPanel.eTaskPanel);
					},
					pageManager.eTaskPanel.AssignETask
					);
		});

		refreshButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParam = new Dictionary<string, string> {
						{ "task_no", taskNumber },
						{ "assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
					};
			ApiManager.instance.APICallGETWithParameters(
				APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETaskRequirements],
					queryParam,
					() => { },
					AssignSpecificETask
					);
		});
	}

	private void Update() {
		Vector3 contentPosition = itemHolder.localPosition;
		contentPosition.x = 0;
		itemHolder.localPosition = contentPosition;
	}

	public void AssignSpecificETask(DownloadHandler returnedItems) {
		string wrappedJson = "{ \"GetETaskDetailsResponseList\": " + returnedItems.text + " }";
		GetETaskDetailsResponseWrapper wrapper = JsonUtility.FromJson<GetETaskDetailsResponseWrapper>(wrappedJson);

		if (wrapper != null && wrapper.GetETaskDetailsResponseList != null) {
			wrapper.GetETaskDetailsResponseList = wrapper.GetETaskDetailsResponseList.OrderBy(item => item.order_num).ToList();
			getETaskDetailsResponseWrapper = wrapper;
		}
		ReturnButton();
		pumpShutdownETask = null;

		foreach (ETaskRequirementsResponse item in getETaskDetailsResponseWrapper.GetETaskDetailsResponseList) {
			GameObject button = GetButton();
			SpecificETaskItem mRButtonClass = button.GetComponent<SpecificETaskItem>();
			mRButtonClass.buttonClass.buttonText.text = item.etask_description;
			switch (item.task_status) {
				case "Started":
				mRButtonClass.buttonClass.unactivatedButton.SetActive(false);
				mRButtonClass.buttonClass.activatedButton.SetActive(true);
				mRButtonClass.buttonClass.completedButton.SetActive(false);
				mRButtonClass.buttonClass.buttonBGImage.texture = incompleteButtonBGSprite;
				mRButtonClass.buttonClass.button.enabled = true;
				break;
				case "Not Started":
				mRButtonClass.buttonClass.unactivatedButton.SetActive(true);
				mRButtonClass.buttonClass.activatedButton.SetActive(false);
				mRButtonClass.buttonClass.completedButton.SetActive(false);
				mRButtonClass.buttonClass.buttonBGImage.texture = incompleteButtonBGSprite;
				mRButtonClass.buttonClass.button.enabled = true;
				break;
				case "Completed":
				mRButtonClass.buttonClass.unactivatedButton.SetActive(false);
				mRButtonClass.buttonClass.activatedButton.SetActive(false);
				mRButtonClass.buttonClass.completedButton.SetActive(true);
				mRButtonClass.buttonClass.buttonBGImage.texture = completeButtonBGSprite;
				mRButtonClass.buttonClass.button.enabled = false;
				break;
			}

			if (item.order_num == 1) {
				pumpShutdownETask = item;
			}else if(item.order_num == 2) {
				if(pumpShutdownETask.task_status == "Completed") {
					if(item.task_status != "Completed") {
						mRButtonClass.buttonClass.button.enabled = true;
					}
				} else {
					mRButtonClass.buttonClass.button.enabled = false;
				}
			}


			// Button item on click open up E Task panel
			mRButtonClass.buttonClass.button.OnClicked.AddListener(delegate {
				AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
				taskOrderNumber = item.order_num.ToString();
				StartCoroutine(EditSpecificETaskStatus(item.task_no, item.order_num.ToString()));
				pageManager.handMenuPanelsManager.stillCanShowMoreOptionDetails = false;
			});
			//}
		}
	}

	IEnumerator EditSpecificETaskStatus(string taskNo, string orderNum) {
		string jsonDataEditJobPack = JsonUtility.ToJson(new ETaskStatus(taskNo, "Started", orderNum));
		yield return StartCoroutine(ApiManager.instance.APICallPut(APIPutMiddlePath.APIPutMiddlePathList[(int)APIPutMiddlePathEnum.ETaskStatus], jsonDataEditJobPack));

		var queryParam = new Dictionary<string, string> {
						{ "task_no", taskNo },
						{ "assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
					};
		ApiManager.instance.APICallGETWithParameters(
			APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETaskRequirements],
				queryParam,
				() => {
					var queryParamStepByStepGuide = new Dictionary<string, string> {
								{"task_no", taskNo},
								{"order_num", orderNum }
					};
					ApiManager.instance.APICallGETWithParameters(
						APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETaskInstructions],
						queryParamStepByStepGuide,
						() => { },
						(DownloadHandler downloadHandler) => {
							switch (orderNum) {
								case "1":
								pageManager.PanelActivation(pageManager.equipmentsPanelPumpShutdown.equipmentPagePanel);
								pageManager.stepByStepGuidePumpShutdown.AssignETaskInstruction(downloadHandler);
								pageManager.callPanel.linkingType = LinkingType.Pumpshutdown;
								break;
								case "2":
								pageManager.stepByStepGuidePhysicalIsolation.AssignETaskInstruction(downloadHandler);
								pageManager.PanelActivation(pageManager.callPanel.callPanel);
								pageManager.callPanel.StartCountdown();
								pageManager.callPanel.linkingType = LinkingType.PhysicalIsolation;
								break;
								case "3":
								pageManager.PanelActivation(null);
								pageManager.stepByStepGuideElectricianIsolation.AssignETaskInstruction(downloadHandler);
								break;
							}
						}
						);
				},
				AssignSpecificETask
				);
	}

	public void LogoutCalled() {
		ReturnButton();
		getETaskDetailsResponseWrapper.GetETaskDetailsResponseList.Clear();
		taskNumber = "";
		taskOrderNumber = "";
		ETaskOrderNumber = "";
	}

	public void UIInteractable(bool interactableStatus) {
		returnFromSpecificETaskButton.button.enabled = interactableStatus;
		refreshButton.button.enabled = interactableStatus;
		foreach (Transform child in itemHolder) {
			GameObject button = child.gameObject;
			SpecificETaskItem buttonClass = button.GetComponent<SpecificETaskItem>();
			buttonClass.buttonClass.button.enabled = interactableStatus;
		}
	}
}
