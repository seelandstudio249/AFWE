using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class GetJobPacksResponseWrapper {
	public List<JobPacksResponse> jobPackItems = new List<JobPacksResponse>();
}


public class JobPackPanel : MonoBehaviour {
	#region Jobs Pack
	[Header("Jobs Pack Settings")]
	public GameObject jobPackPanel;
	[SerializeField] Transform jobPackHolder;
	[SerializeField] GameObject jobPackItemButtonPrefab;
	[SerializeField] MRButtonClass returnFromJobPackButton;
	[SerializeField] MRButtonClass refreshButton;
	[SerializeField] TMP_Text introductionText;
	#endregion

	[SerializeField] GetJobPacksResponseWrapper mtJobPackWrapper;

	PageManager pageManager;

	#region Object Pooling 
	private Queue<GameObject> pool = new Queue<GameObject>();

	public GameObject GetButton() {
		if (pool.Count > 0) {
			GameObject pooledButton = pool.Dequeue();
			pooledButton.SetActive(true);
			return pooledButton;
		} else {
			return Instantiate(jobPackItemButtonPrefab, jobPackHolder);
		}
	}

	public void ReturnButton() {
		pool.Clear();

		foreach (Transform child in jobPackHolder) {
			GameObject pooledButton = child.gameObject;
			pooledButton.SetActive(false);
			pool.Enqueue(pooledButton);
		}
	}
	#endregion

	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromJobPackButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParamsGetUserAttributes = new Dictionary<string, string> {
			{"user_id",  pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
			};
			ApiManager.instance.APICallGETWithParameters(
			APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.UserAttributes],
			queryParamsGetUserAttributes,
			() => {
				//completePanel.SetActive(false);
				//mainGuidePanel.SetActive(false);
			},
			(DownloadHandler downloadHandler) => {
				pageManager.managerControlScript.loginScript.ProcessGetUserAttributesResponse(downloadHandler);
				pageManager.PanelActivation(pageManager.homePagePanel.homePanel);
			});
		});

		refreshButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParamsUserID = new Dictionary<string, string> {
			{
					"assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
			};
			ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobPack],
					queryParamsUserID,
					() => { },
					AssignJobPack
					);
		});
	}

	private void Update() {
		Vector3 contentPosition = jobPackHolder.localPosition;
		contentPosition.x = 0;
		jobPackHolder.localPosition = contentPosition;
	}

	public void AssignJobPack(DownloadHandler returnedItems) {
		string wrappedJson = "{ \"jobPackItems\": " + returnedItems.text + " }";
		GetJobPacksResponseWrapper wrapper = JsonUtility.FromJson<GetJobPacksResponseWrapper>(wrappedJson);
		if (wrapper != null && wrapper.jobPackItems != null) {
			wrapper.jobPackItems = wrapper.jobPackItems.OrderBy(item => item.job_order).ToList();
			mtJobPackWrapper = wrapper;
		}

		introductionText.text = "Hi " + pageManager.managerControlScript.loginScript.userAccountDetails.first_name + "! List of Job Packs assigned to you";

		ReturnButton();
		foreach (JobPacksResponse item in mtJobPackWrapper.jobPackItems) {
			GameObject button = GetButton();
			JobPackItem mRButtonClass = button.GetComponent<JobPackItem>();
			mRButtonClass.buttonClass.jobpack_noText.text = item.jobpack_no;
			mRButtonClass.buttonClass.descriptionText.text = item.description;
			mRButtonClass.buttonClass.start_dateText.text = item.start_date;
			mRButtonClass.buttonClass.end_dateText.text = item.end_date;

			if(item.job_status != "Completed") {
				mRButtonClass.buttonClass.button.enabled = true;
				mRButtonClass.buttonClass.button.OnClicked.AddListener(delegate {
					AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
					pageManager.jobTaskPanel.jobPackNumber = item.jobpack_no;
					var queryParam = new Dictionary<string, string> {
						{ "jobpack_no", item.jobpack_no },
						{ "assignee_id" , pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
					};
					ApiManager.instance.APICallGETWithParameters(
						APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobPackDocuments],
							queryParam,
							() => {
								pageManager.PanelActivation(pageManager.jobTaskPanel.jobTaskPanel);
								pageManager.jobTaskPanel.taskNo = item.jobpack_no;
								pageManager.jobTaskPanel.orderNum = item.job_order.ToString();
							},
							pageManager.jobTaskPanel.AssignJobDocuments
							);
				});
				mRButtonClass.buttonClass.completedButton.SetActive(false);
			} else {
				mRButtonClass.buttonClass.button.enabled = false;
				mRButtonClass.buttonClass.completedButton.SetActive(true);
			}
			
		}
	}

	public void LogoutCalled() {
		ReturnButton();
		mtJobPackWrapper.jobPackItems.Clear();
	}

	public void UIInteractable(bool interactableStatus) {
		returnFromJobPackButton.button.enabled = interactableStatus;
		refreshButton.button.enabled = interactableStatus;
		foreach (Transform child in jobPackHolder) {
			GameObject button = child.gameObject;
			if (button != null) {
				JobPackItem buttonClass = button.GetComponent<JobPackItem>();
				buttonClass.buttonClass.button.enabled = interactableStatus;
			}
		}
	}
}
