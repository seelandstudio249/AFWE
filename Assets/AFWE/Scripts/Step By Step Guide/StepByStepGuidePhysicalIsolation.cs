using Paroxe.PdfRenderer;
using Paroxe.PdfRenderer.WebGL;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class StepByStepGuidePhysicalIsolation : StepByStepGuide {
	[Header("Call Panel")]
	[SerializeField] GameObject callPanel;
	[SerializeField] TMP_Text callPanelText;

	[Space(10), Header("Tracking Position Trigger")]
	public bool startTrackingValvePosition;

	[Space(10), Header("Valve 1 Positioning Setting")]
	[SerializeField] GameObject valve1PositionStandPoint;
	BoxCollider valve1BoxCollider;
	[SerializeField] Transform valve1PanelPosition, valve1HandPosition;

	[Space(10), Header("Valve 2 Positioning Setting")]
	[SerializeField] GameObject valve2PositionStandPoint;
	BoxCollider valve2BoxCollider;
	[SerializeField] Transform valve2PanelPosition, valve2HandPosition;

	[Space(10), Header("Hand Coach")]
	[SerializeField] GameObject handCoach;

	#region PDF Variables
	[Space(15)]
	[Header("PDF Panel")]
	[SerializeField] MRButtonClass showPdfButton;
	[SerializeField] GameObject pdfPanel;
	private PDFDocument pdfDocument;
	private PDFJS_Promise<PDFDocument> documentPromise;
	public int m_Page = 0;
	[SerializeField] ScrollRect scrollView;
	[SerializeField] GameObject pagePrefab;

	#region Object Pooling 
	private Queue<GameObject> pool = new Queue<GameObject>();

	public GameObject GetPage() {
		if (pool.Count > 0) {
			GameObject pooledPage = pool.Dequeue();
			pooledPage.SetActive(true);
			return pooledPage;
		} else {
			return Instantiate(pagePrefab, scrollView.content);
		}
	}

	public void ReturnPage() {
		pool.Clear();

		foreach (Transform child in scrollView.content) {
			GameObject pooledPage = child.gameObject;
			pooledPage.SetActive(false);
			pool.Enqueue(pooledPage);
		}
	}
	#endregion
	#endregion

	[SerializeField] int currentIndex = 1;

	private PhysicalIsolationActiveValidationReturnedData physicalIsolationActiveValidationReturnedData;

	private void Awake() {
		pageManager.cameraPanel.PhotoTakenSendAPIPhysicalIsolation += PhotoTaken;
		valve1BoxCollider = valve1PositionStandPoint.GetComponent<BoxCollider>();
		valve2BoxCollider = valve2PositionStandPoint.GetComponent<BoxCollider>();

		nextStepButtonTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchPanel(1);
		});

		previousStepButtonTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchPanel(-1);
		});

		completeButtonTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			TaskCompleted();
		});

		nextStepButtonImageTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchPanel(1);
		});

		previousStepButtonImageTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchPanel(-1);
		});

		completeButtonImageTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			TaskCompleted();
		});

		nextStepButtonVideoTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchPanel(1);
		});

		previousStepButtonVideoTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchPanel(-1);
		});

		completeButtonVideoTextPanel.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			TaskCompleted();
		});

		completePanelButton.button.OnClicked.AddListener(delegate {
			currentIndex = 1;
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			string jsonDataEditJobPack = JsonUtility.ToJson(new ETaskStatus(pageManager.specificETaskPanel.taskNumber, "Completed", pageManager.specificETaskPanel.taskOrderNumber));
			StartCoroutine(ApiManager.instance.APICallPut(
				APIPutMiddlePath.APIPutMiddlePathList[(int)APIPutMiddlePathEnum.ETaskStatus],
				jsonDataEditJobPack,
				() => {
					string jsonDataEditJobPack = JsonUtility.ToJson(new ETaskStatus(pageManager.specificETaskPanel.taskNumber, "Completed", pageManager.specificETaskPanel.taskOrderNumber));
					StartCoroutine(ApiManager.instance.APICallPut(
						APIPutMiddlePath.APIPutMiddlePathList[(int)APIPutMiddlePathEnum.ETaskStatus],
						jsonDataEditJobPack,
						() => {

						}
					));

					string passInParams = JsonUtility.ToJson(new JobpackDocumentStatus("80771772", 3626624, "Physical Isolation Certificate", "Approved"));
					StartCoroutine(ApiManager.instance.APICallPut(APIPutMiddlePath.APIPutMiddlePathList[(int)APIPutMiddlePathEnum.JobPackDocumentStatus], passInParams,
						() => {
							var queryParamsGetUserAttributes = new Dictionary<string, string> {
							{"user_id",  pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
								};
							ApiManager.instance.APICallGETWithParameters(
							APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.UserAttributes],
							queryParamsGetUserAttributes,
							() => {
								pageManager.handMenuPanelsManager.stillCanShowMoreOptionDetails = true;
								completePanel.SetActive(false);
								pdfPanel.SetActive(false);
							},
							(DownloadHandler downloadHandler) => {
								isStepInForTheFirstTime = false;
								pageManager.managerControlScript.loginScript.ProcessGetUserAttributesResponse(downloadHandler);
								pageManager.homePagePanel.homePanel.SetActive(true);
							});
						}));
				}
				));
		});

		showPdfButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			PerformActionAfterCompleteTaskCountdown();
		});
	}

	private void Update() {
		if (documentPromise != null) {
			if (documentPromise.HasFinished) {
				if (documentPromise.HasSucceeded) {
					OnDocumentLoaded(documentPromise.Result);
				} else {
					OnDocumentLoadFailed();
				}
				documentPromise = null;
			}
		}

		if (startTrackingValvePosition) {
			Vector3 rayOrigin = Camera.main.transform.position;
			Vector3 rayDirection = Vector3.down;
			if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, Mathf.Infinity, startingPointLayer)) {
				outOfPointWarningPanel.SetActive(false);
				if (!isStepInForTheFirstTime) {
					QrScanned("");
					isStepInForTheFirstTime = true;
				} else {
					currentIndex = 3;
					DisplayPanel();
					stepByStepGuidePanelParentObj.SetActive(true);
					//SwitchPanel(1);
				}
			}
		} else {
			outOfPointWarningPanel.SetActive(false);
		}
	}

	public override void AssignETaskInstruction(DownloadHandler returnedItems) {
		base.AssignETaskInstruction(returnedItems);
		callPanelText.text = ShowProperText(eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[0].message);

		for (int i = 0; i < eTaskInstructionsResponseWrapper.eTaskInstructionsResponses.Count; i++) {
			if (eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[i].blob_url != "") {
				GameObject item2 = Instantiate(StepByStepGuideTextAndVideoItemPrefab, guideItemHolder);
				StepByStepGuideTextAndVideoItem itemScript2 = item2.GetComponent<StepByStepGuideTextAndVideoItem>();
				itemScript2.descriptionTextField.text = eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[i].message;
				itemScript2.videoPlayer.url = eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[i].blob_url;
				itemScript2.videoPlayer.Play();
			} else {
				GameObject item = Instantiate(stepByStepGuideTextItemPrefab, guideItemHolder);
				StepByStepGuideTextItem itemScript = item.GetComponent<StepByStepGuideTextItem>();
				itemScript.descriptionTextField.text = eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[i].message;
			}
		}
	}

	private void SwitchPanel(int direction) {
		if (eTaskInstructionsResponseWrapper.eTaskInstructionsResponses.Count == 0) return;
		currentIndex += direction;
		if (currentIndex >= eTaskInstructionsResponseWrapper.eTaskInstructionsResponses.Count) {
			currentIndex = eTaskInstructionsResponseWrapper.eTaskInstructionsResponses.Count - 1;
		} else if (currentIndex < 1) {
			currentIndex = 1;
		}
		DisplayPanel();
		UpdateButtonStates(currentIndex, eTaskInstructionsResponseWrapper.eTaskInstructionsResponses.Count);
	}

	void DisplayPanel() {
		stepByStepGuideTextOnly_HeaderTextField.text = (currentIndex + 1).ToString();
		stepByStepGuideImageText_HeaderTextField.text = (currentIndex + 1).ToString();
		stepByStepGuideVideoText_HeaderTextField.text = (currentIndex + 1).ToString();
		stepByStepGuideTextOnly_TextField.text = ShowProperText(eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[currentIndex].message);
		stepByStepGuideImageText_TextField.text = ShowProperText(eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[currentIndex].message);
		stepByStepGuideVideoText_TextField.text = ShowProperText(eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[currentIndex].message);

		if (eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[currentIndex].blob_url != "" && eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[0].blob_url != null) {
			stepByStepGuideTextPanel.SetActive(false);
			stepByStepGuidePanelWithVideo.SetActive(true);
			videoPlayer.url = eTaskInstructionsResponseWrapper.eTaskInstructionsResponses[currentIndex].blob_url;
		} else {
			stepByStepGuideTextPanel.SetActive(true);
			stepByStepGuidePanelWithVideo.SetActive(false);
		}
		handCoach.SetActive(false);
		switch (currentIndex) {
			case 1:
			valve1PositionStandPoint.SetActive(false);
			startTrackingStartingPoint = false;
			valve1BoxCollider.enabled = false;
			pageManager.cameraPanel.StopAutoTakePhotoForActiveValidation();
			UIInteractable(true);
			//stepByStepGuidePanelParentObj.transform.position = valve1PanelPosition.position;
			handCoach.SetActive(true);
			handCoach.transform.position = valve1HandPosition.position;
			break;
			case 2:
			pageManager.cameraPanel.AutoTakePhotoForActiveValidation();
			pageManager.cameraPanel.currentPhotoMessageInput = "Is there a blue card? answer in json format {is_Blue_Card:boolean}";
			UIInteractable(false);
			break;
			case 3:
			valve2PositionStandPoint.SetActive(false);
			startTrackingStartingPoint = false;
			valve2BoxCollider.enabled = false;
			pageManager.cameraPanel.StopAutoTakePhotoForActiveValidation();
			UIInteractable(true);
			//stepByStepGuidePanelParentObj.transform.position = valve2PanelPosition.position;
			handCoach.SetActive(true);
			handCoach.transform.position = valve2HandPosition.position;
			break;
			case 4:
			pageManager.cameraPanel.AutoTakePhotoForActiveValidation();
			pageManager.cameraPanel.currentPhotoMessageInput = "Is there a blue card? answer in json format {is_Blue_Card:boolean}";
			UIInteractable(false);
			break;
		}
	}

	void TaskCompleted() {
		handCoach.SetActive(false);

		isStepInForTheFirstTime = false;

		stepByStepGuidePanelParentObj.SetActive(false);

		stepByStepGuideTextPanel.SetActive(false);
		stepByStepGuideTextOnly_HeaderTextField.text = "";
		stepByStepGuideTextOnly_TextField.text = "";

		stepByStepGuidePanelWithImage.SetActive(false);
		stepByStepGuideImageText_HeaderTextField.text = "";
		stepByStepGuideImageText_TextField.text = "";
		imageDisplay.texture = null;

		stepByStepGuidePanelWithVideo.SetActive(false);
		videoPlayer.url = null;
		videoPlayer.Stop();
		stepByStepGuideVideoText_HeaderTextField.text = "";
		stepByStepGuideVideoText_TextField.text = "";

		completePanel.SetActive(true);
		//completePanel.transform.position = valve2PanelPosition.position;

		UpdateButtonStates(currentIndex, eTaskInstructionsResponseWrapper.eTaskInstructionsResponses.Count);
		//pageManager.handMenuPanelsManager.stillCanShowMoreOptionDetails = true;
	}

	protected override void UpdateButtonStates(int currentIndex, int maxIndex) {
		//// Disable the previous button if we're on the first image
		previousStepButtonTextPanel.button.gameObject.SetActive(currentIndex > 0);
		previousStepButtonImageTextPanel.button.gameObject.SetActive(currentIndex > 0);
		previousStepButtonVideoTextPanel.button.gameObject.SetActive(currentIndex > 0);

		//// Disable the next button if we're on the last image
		nextStepButtonTextPanel.button.gameObject.SetActive(currentIndex < maxIndex - 2);
		nextStepButtonImageTextPanel.button.gameObject.SetActive(currentIndex < maxIndex - 2);
		nextStepButtonVideoTextPanel.button.gameObject.SetActive(currentIndex < maxIndex - 2);
		completeButtonTextPanel.button.gameObject.SetActive(currentIndex == maxIndex - 2);
		completeButtonImageTextPanel.button.gameObject.SetActive(currentIndex == maxIndex - 2);
		completeButtonVideoTextPanel.button.gameObject.SetActive(currentIndex == maxIndex - 2);
	}

	protected override void PerformActionAfterCompleteTaskCountdown() {
		base.PerformActionAfterCompleteTaskCountdown();
		StopCountdown();
		var queryParamsUserID = new Dictionary<string, string> {
			{"jobpack_no", "80771772" },
			{"document_name", "Physical Isolation Certificate" }
			};
		ApiManager.instance.APICallGETWithParameters(
							APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobpackDocumentSingle],
							queryParamsUserID,
							() => {
								completePanel.SetActive(false);
								pdfPanel.SetActive(true);
								//pdfPanel.transform.position = valve2PanelPosition.position;
								completePanelButton.button.enabled = true;
							},
							AssignPDF
							);
	}

	#region Photo Taken
	void PhotoTaken(ActiveValidations parameters) {
		string jsonDataActiveValidation = JsonUtility.ToJson(parameters);
		Debug.Log($"PIC, Sending Data To OpenAI on step {currentIndex}: {pageManager.cameraPanel.currentPhotoMessageInput}");
		Debug.Log($"Result Photo: {parameters.base64_image}");
		StartCoroutine(
			ApiManager.instance.APICallPOST(APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.ActiveValidationsOpenAi],
			jsonDataActiveValidation,
			() => {
				Debug.Log("Sending API");
			},
			CheckPhoto
			));
	}

	void CheckPhoto(DownloadHandler returnedItems) {
		Debug.Log($"PIC, Result Received From OpenAI on step {currentIndex}: {returnedItems.text}");

		physicalIsolationActiveValidationReturnedData = JsonUtility.FromJson<PhysicalIsolationActiveValidationReturnedData>(returnedItems.text);
		if (physicalIsolationActiveValidationReturnedData != null) {
			if (CheckCondition(physicalIsolationActiveValidationReturnedData)) {
				AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.ActiveValidationSuccessSfx);
				StartCoroutine(StartCountdown(5, () => {
					switch (currentIndex) {
						case 2:
						startTrackingValvePosition = true;
						StartTrackingValve(2);
						stepByStepGuidePanelParentObj.SetActive(false);
						break;
						case 4:
						TaskCompleted();
						StartCountdownAfterCompleted();
						break;
						default:
						Debug.LogError($"Step {currentIndex} doesnt required active validation");
						break;
					}
				}));
			} else {
				AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.ActiveValidationFailSfx);
				StartCoroutine(StartCountdown(5, () => {
					pageManager.cameraPanel.AutoTakePhotoForActiveValidation();
				}));
			}
		} else {
			pageManager.aiValidationPanel.aiValidateStatusPanel.SetActive(true);
			pageManager.aiValidationPanel.aiValidateStatusText.text = "Validation Failed";
			pageManager.aiValidationPanel.aiValidateDetailsText.text = "No Data";
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.ActiveValidationFailSfx);
			StartCoroutine(StartCountdown(5, () => {
				pageManager.cameraPanel.AutoTakePhotoForActiveValidation();
			}));
		}
	}

	bool CheckCondition(PhysicalIsolationActiveValidationReturnedData itemToCheck) {
		pageManager.aiValidationPanel.aiValidateStatusPanel.SetActive(true);
		if (itemToCheck.is_Blue_Card) {
			pageManager.aiValidationPanel.aiValidateStatusText.text = "Validation Successful";
			pageManager.aiValidationPanel.aiValidateDetailsText.text = "Loto Detected";
			return true;
		} else {
			pageManager.aiValidationPanel.aiValidateStatusText.text = "Validation Failed";
			pageManager.aiValidationPanel.aiValidateDetailsText.text = "LOTO Not Detected";
			return false;
		}
	}
	#endregion

	public void QrScanned(string qrCodeString) {
		stepByStepGuidePanelParentObj.SetActive(true);
		startTrackingValvePosition = false;
		currentIndex = 1;
		DisplayPanel();
		UpdateButtonStates(currentIndex, eTaskInstructionsResponseWrapper.eTaskInstructionsResponses.Count);
		stepByStepGuidePanelParentObj.SetActive(true);
		//SwitchPanel(1);
	}

	public override void LogoutCalled() {
		base.LogoutCalled();
	}

	public override void UIInteractable(bool interactableStatus) {
		base.UIInteractable(interactableStatus);
	}

	public void StartTrackingValve(int valveNumber) {
		switch (valveNumber) {
			case 1:
			valve1PositionStandPoint.SetActive(true);
			valve1BoxCollider.enabled = true;
			break;
			case 2:
			valve2PositionStandPoint.SetActive(true);
			valve2BoxCollider.enabled = true;
			break;
			default:
			Debug.LogError("Valve doesnt exist");
			break;
		}
	}

	#region PDF reading
	void LoadPDFDocumentAsync(string url) {
		documentPromise = PDFDocument.LoadDocumentFromUrlAsync(url);
	}

	void OnDocumentLoaded(PDFDocument document) {
		pdfDocument = document;
		int pageCount = pdfDocument.GetPageCount();
		for (int i = 0; i < pageCount; i++) {
			PDFRenderer renderer = new PDFRenderer();
			Texture2D tex = renderer.RenderPageToTexture(pdfDocument.GetPage(i), 1024, 1024);
			tex.filterMode = FilterMode.Bilinear;
			tex.anisoLevel = 8;
			Sprite pageSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
			GameObject pageObj = GetPage();
			pageObj.GetComponent<Image>().sprite = pageSprite;
		}
	}

	void OnDocumentLoadFailed() {
		Debug.LogError("Failed to load PDF document.");
	}

	void AssignPDF(DownloadHandler returnedItems) {
		string wrappedJson = returnedItems.text;
		JobpackDocumentSingleResponse jobpackDocumentSingleResponse = JsonUtility.FromJson<JobpackDocumentSingleResponse>(wrappedJson);
		LoadPDFDocumentAsync(jobpackDocumentSingleResponse.blob_url);
	}
	#endregion
}

[Serializable]
public class PhysicalIsolationActiveValidationReturnedData {
	public bool is_Blue_Card;
	public bool is_Red_Card;
}
