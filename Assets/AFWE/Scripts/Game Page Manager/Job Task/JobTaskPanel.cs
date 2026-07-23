using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class GetJobPackDocumentsResponseWrapper {
	public List<JobpackDocumentsResponse> jobPackDocumentsList = new List<JobpackDocumentsResponse>();
}

public class JobTaskPanel : MonoBehaviour {
	#region Job Task
	[Header("Job Tasks Settings")]
	public GameObject jobTaskPanel;
	[SerializeField] Transform jobTaskHolder;
	[SerializeField] GameObject jobTaskItemButtonPrefab;
	[SerializeField] MRButtonClass returnFromJobTaskButton;
	[SerializeField] MRButtonClass refreshButton;
	[SerializeField] MRButtonClass startButton;
	[SerializeField] TMP_Text introductionText;
	#endregion

	bool isDocument1Approved, isDocument2Approved, isDocument3Approved, isDocument4Approved, isEICApproved, isPICApproved;

	public string jobPackNumber;

	[SerializeField] GetJobPackDocumentsResponseWrapper jobPackDocumentsResponseWrapper;

	[SerializeField] Texture2D pendingSprite, approveSprite;

	PageManager pageManager;

	public string taskNo, orderNum;

	#region Object Pooling 
	private Queue<GameObject> pool = new Queue<GameObject>();

	public GameObject GetButton() {
		if (pool.Count > 0) {
			GameObject pooledButton = pool.Dequeue();
			pooledButton.SetActive(true);
			return pooledButton;
		} else {
			return Instantiate(jobTaskItemButtonPrefab, jobTaskHolder);
		}
	}

	public void ReturnButton() {
		pool.Clear();

		foreach (Transform child in jobTaskHolder) {
			GameObject pooledButton = child.gameObject;
			pooledButton.SetActive(false);
			pool.Enqueue(pooledButton);
		}
	}
	#endregion

	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromJobTaskButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParam = new Dictionary<string, string> {
					{ "jobpack_no", jobPackNumber },
					{ "assignee_id" , pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
				};
			ApiManager.instance.APICallGETWithParameters(
				APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobPackDocuments],
					queryParam,
					() => {
						pageManager.PanelActivation(pageManager.jobPackPanel.jobPackPanel);

					},
					pageManager.jobTaskPanel.AssignJobDocuments
					);
		});

		refreshButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParam = new Dictionary<string, string> {
					{ "jobpack_no", jobPackNumber },
					{ "assignee_id" , pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
				};
			ApiManager.instance.APICallGETWithParameters(
				APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobPackDocuments],
					queryParam,
					() => { },
					pageManager.jobTaskPanel.AssignJobDocuments
					);
		});

		startButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			pageManager.handMenuPanelsManager.stillCanShowMoreOptionDetails = false;
			var queryParamStepByStepGuide = new Dictionary<string, string> {
				{"jobpack_no", taskNo}
			};
			ApiManager.instance.APICallGETWithParameters(
				APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobpackInstructions],
						queryParamStepByStepGuide,
						() => { },
						pageManager.stepByStepGuideLubOilChange.AssignJobPackInstruction
						);
			QRCodesManager.instance.currentQrCodeString = "Lube Oil Change";
			pageManager.PanelActivation(pageManager.equipmentsPanelLubeOilChange.equipmentPagePanel);
		});
	}

	private void Update() {
		Vector3 contentPosition = jobTaskHolder.localPosition;
		contentPosition.x = 0;
		jobTaskHolder.localPosition = contentPosition;
	}

	public void AssignJobDocuments(DownloadHandler returnedItems) {
		string wrappedJson = "{ \"jobPackDocumentsList\": " + returnedItems.text + " }";
		GetJobPackDocumentsResponseWrapper wrapper = JsonUtility.FromJson<GetJobPackDocumentsResponseWrapper>(wrappedJson);
		if (wrapper != null && wrapper.jobPackDocumentsList != null) {
			wrapper.jobPackDocumentsList = wrapper.jobPackDocumentsList.OrderBy(item => item.document_order).ToList();
			jobPackDocumentsResponseWrapper = wrapper;
		}
		introductionText.text = introductionText.text = "Hi " + pageManager.managerControlScript.loginScript.userAccountDetails.first_name + "! List of Job Packs assigned to you";
		ReturnButton();
		JobTaskItemWPending permitToWorkbuttonScript;
		isEICApproved = false;
		isPICApproved = false;
		isDocument1Approved = false;
		isDocument2Approved = false;
		isDocument3Approved = false;
		isDocument4Approved = false;
		foreach (JobpackDocumentsResponse item in jobPackDocumentsResponseWrapper.jobPackDocumentsList) {
			if (item.document_name != "Electrical Isolation Certificate" && item.document_name != "Physical Isolation Certificate") {
				GameObject button = GetButton();
				JobTaskItemWPending buttonScript = button.GetComponent<JobTaskItemWPending>();
				buttonScript.buttonClass.buttonText.text = item.document_name;
				switch (item.document_status) {
					default:
					buttonScript.buttonClass.sprite.SetActive(false);
					break;
					case "Reviewed":
					buttonScript.buttonClass.sprite.SetActive(true);
					break;
				}

				if(item.document_order == 1 && item.document_status == "Reviewed") {
					isDocument1Approved = true;
				} else if (item.document_order == 2 && item.document_status == "Reviewed") {
					isDocument2Approved = true;
				} else if(item.document_order == 3 && item.document_status == "Reviewed") {
					isDocument3Approved = true;
				}

				if (item.document_name == "Permit To Work") {
					permitToWorkbuttonScript = buttonScript;
					buttonScript.buttonClass.pendingPIC.SetActive(true);
					buttonScript.buttonClass.pendingEIC.SetActive(true);

					switch (jobPackDocumentsResponseWrapper.jobPackDocumentsList.Where(x => x.document_name == "Electrical Isolation Certificate").First().document_status) {
						case "Approved":
						buttonScript.buttonClass.pendingEIC.GetComponent<RawImage>().texture = approveSprite;
						buttonScript.buttonClass.eicStatusText.text = "EIC Approved";
						isEICApproved = true;
						break;
						default:
						buttonScript.buttonClass.pendingEIC.GetComponent<RawImage>().texture = pendingSprite;
						buttonScript.buttonClass.eicStatusText.text = "Pending EIC";
						isEICApproved = false;
						break;
					}
					switch (jobPackDocumentsResponseWrapper.jobPackDocumentsList.Where(x => x.document_name == "Physical Isolation Certificate").First().document_status) {
						case "Approved":
						buttonScript.buttonClass.pendingPIC.GetComponent<RawImage>().texture = approveSprite;
						buttonScript.buttonClass.picStatusText.text = "PIC Approved";
						isPICApproved = true;
						break;
						default:
						buttonScript.buttonClass.pendingPIC.GetComponent<RawImage>().texture = pendingSprite;
						buttonScript.buttonClass.picStatusText.text = "Pending PIC";
						isPICApproved = false;
						break;
					}
					if(isEICApproved && isPICApproved) {
						buttonScript.buttonClass.button.enabled = true;
					} else {
						buttonScript.buttonClass.button.enabled = false;
					}
					switch (item.document_status) {
						default:
						isDocument4Approved = false;
						buttonScript.buttonClass.sprite.SetActive(false);
						break;
						case "Approved":
						isDocument4Approved = true;
						buttonScript.buttonClass.sprite.SetActive(true);
						break;
					}
				}

				buttonScript.buttonClass.button.OnClicked.AddListener(delegate {
					AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
					pageManager.PanelActivation(pageManager.jobTaskDocument.jobTaskDocumentPanel);
					pageManager.jobTaskDocument.jobPackDocumentStatus.jobpack_no = item.jobpack_no;
					pageManager.jobTaskDocument.jobPackDocumentStatus.document_name = item.document_name;
					pageManager.jobTaskDocument.jobPackDocumentStatus.new_status = item.document_status;
					pageManager.jobTaskDocument.jobPackDocumentTitle.text = item.document_name;
					if (item.blob_url != "" && item.blob_url != null) {
						pageManager.jobTaskDocument.pdfURL = item.blob_url;
					} else {
						pageManager.jobTaskDocument.pdfURL = "https://www.open-std.org/jtc1/sc22/wg21/docs/papers/2013/n3690.pdf";
					}
					pageManager.jobTaskDocument.LoadPDFDocumentAsync(pageManager.jobTaskDocument.pdfURL);
				});
			}
		}

		if (isDocument1Approved && isDocument2Approved && isDocument3Approved && isDocument4Approved) {
			startButton.button.enabled = true;
		} else {
			startButton.button.enabled = false;
		}
	}

	public void LogoutCalled() {
		ReturnButton();
		jobPackDocumentsResponseWrapper.jobPackDocumentsList.Clear();
	}

	public void UIInteractable(bool interactableStatus) {
		returnFromJobTaskButton.button.enabled = interactableStatus;
		refreshButton.button.enabled = interactableStatus;
		foreach (Transform child in jobTaskHolder) {
			GameObject button = child.gameObject;
			JobTaskItemWPending buttonClass = button.GetComponent<JobTaskItemWPending>();
			buttonClass.buttonClass.button.enabled = interactableStatus;
		}
	}
}
