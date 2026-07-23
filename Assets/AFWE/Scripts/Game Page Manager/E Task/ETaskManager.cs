using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class GetETasksResponseWrapper {
	public List<ETaskResponse> getETasksResponsesList = new List<ETaskResponse>();
}

public class ETaskManager : MonoBehaviour {
	#region E Tasks
	[Header("E Tasks Settings")]
	public GameObject eTaskPanel;
	[SerializeField] Transform eTaskHolder;
	[SerializeField] GameObject eTaskItemButtonPrefab;
	[SerializeField] MRButtonClass returnFromETaskButton;
	[SerializeField] MRButtonClass refreshButton;
	[SerializeField] TMP_Text introductionText;
	[SerializeField] Texture2D incompleteButtonBGSprite, completeButtonBGSprite;
	#endregion

	[SerializeField] GetETasksResponseWrapper GetETasksResponseWrapper;

	PageManager pageManager;
	public string ETaskOrderNumber;

	#region Object Pooling 
	private Queue<GameObject> pool = new Queue<GameObject>();

	public GameObject GetButton() {
		if (pool.Count > 0) {
			GameObject pooledButton = pool.Dequeue();
			pooledButton.SetActive(true);
			return pooledButton;
		} else {
			return Instantiate(eTaskItemButtonPrefab, eTaskHolder);
		}
	}

	public void ReturnButton() {
		pool.Clear();

		foreach (Transform child in eTaskHolder) {
			GameObject pooledButton = child.gameObject;
			pooledButton.SetActive(false);
			pool.Enqueue(pooledButton);
		}
	}
	#endregion

	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromETaskButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParamsGetUserAttributes = new Dictionary<string, string> {
			{"user_id",  pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
			};
			ApiManager.instance.APICallGETWithParameters(
			APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.UserAttributes],
			queryParamsGetUserAttributes,
			() => { },
			(DownloadHandler downloadHandler) => {
				pageManager.managerControlScript.loginScript.ProcessGetUserAttributesResponse(downloadHandler);
				pageManager.PanelActivation(pageManager.homePagePanel.homePanel);
			});
		});

		refreshButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParamsUserID = new Dictionary<string, string> {
			{"assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() },
			{"order_num", ETaskOrderNumber }
			};
			ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETask],
					queryParamsUserID,
					() => { },
					AssignETask
					);
		});
	}

	private void Update() {
		Vector3 contentPosition = eTaskHolder.localPosition;
		contentPosition.x = 0;
		eTaskHolder.localPosition = contentPosition;
	}

	public void AssignETask(DownloadHandler returnedItems) {
		string wrappedJson = "{ \"getETasksResponsesList\": " + returnedItems.text + " }";
		GetETasksResponseWrapper wrapper = JsonUtility.FromJson<GetETasksResponseWrapper>(wrappedJson);

		if (wrapper != null && wrapper.getETasksResponsesList != null) {
			wrapper.getETasksResponsesList = wrapper.getETasksResponsesList.OrderBy(item => item.order_num).ToList();
			GetETasksResponseWrapper = wrapper;
		}
		introductionText.text = "Hi " + pageManager.managerControlScript.loginScript.userAccountDetails.first_name + "! List of task assigned to you";
		ReturnButton();

		foreach (ETaskResponse item in GetETasksResponseWrapper.getETasksResponsesList) {
			GameObject button = GetButton();
			ETaskPanelItems mRButtonClass = button.GetComponent<ETaskPanelItems>();
			mRButtonClass.buttonClass.task_no.text = item.task_no;
			mRButtonClass.buttonClass.etask_description.text = item.etask_description;
			mRButtonClass.buttonClass.start_date.text = item.start_date;
			mRButtonClass.buttonClass.end_date.text = item.end_date;

			switch (item.task_status) {
				case "Started":
				mRButtonClass.buttonClass.buttonBGImage.texture = incompleteButtonBGSprite;
				mRButtonClass.buttonClass.button.enabled = true;
				mRButtonClass.buttonClass.completedButton.SetActive(false);
				break;
				case "Not Started":
				mRButtonClass.buttonClass.buttonBGImage.texture = incompleteButtonBGSprite;
				mRButtonClass.buttonClass.button.enabled = true;
				mRButtonClass.buttonClass.completedButton.SetActive(false);
				break;
				case "Completed":
				mRButtonClass.buttonClass.buttonBGImage.texture = completeButtonBGSprite;
				mRButtonClass.buttonClass.button.enabled = false;
				mRButtonClass.buttonClass.completedButton.SetActive(true);
				break;
			}

			// Button item on click open up E Task panel
			mRButtonClass.buttonClass.button.OnClicked.AddListener(delegate {
				AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
				pageManager.specificETaskPanel.taskNumber = item.task_no;
				//Debug.Log(item.task_no);
				//Debug.Log(pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString());

				var queryParam = new Dictionary<string, string> {
						{ "task_no", item.task_no },
						{ "assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
					};
				ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETaskRequirements],
						queryParam,
						() => {
							pageManager.PanelActivation(pageManager.specificETaskPanel.specificETaskPanel);
							pageManager.specificETaskPanel.introductionText.text = item.etask_description;
						},
						pageManager.specificETaskPanel.AssignSpecificETask
						);
			});
		}
	}

	public void LogoutCalled() {
		ReturnButton();
		GetETasksResponseWrapper.getETasksResponsesList.Clear();
		ETaskOrderNumber = "";
	}

	public void UIInteractable(bool interactableStatus) {
		returnFromETaskButton.button.enabled = interactableStatus;
		refreshButton.button.enabled = interactableStatus;
		foreach (Transform child in eTaskHolder) {
			GameObject button = child.gameObject;
			ETaskPanelItems buttonClass = button.GetComponent<ETaskPanelItems>();
			buttonClass.buttonClass.button.enabled = interactableStatus;
		}
	}
}
