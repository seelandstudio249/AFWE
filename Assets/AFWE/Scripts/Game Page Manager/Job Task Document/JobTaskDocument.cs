using Paroxe.PdfRenderer;
using Paroxe.PdfRenderer.WebGL;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JobTaskDocument : MonoBehaviour {
	#region Job Task Document
	[Header("Job Task Document Settings")]
	public GameObject jobTaskDocumentPanel;
	[SerializeField] MRButtonClass returnFromjobTaskDocumentButton;
	[SerializeField] MRButtonClass refreshButton;
	[SerializeField] MRButtonClass confirmjobTaskDocumentButton;
	public TMP_Text jobPackDocumentTitle;
	#endregion

	#region PDF Reader
	private PDFDocument pdfDocument;
	private PDFJS_Promise<PDFDocument> documentPromise;
	public int m_Page = 0;
	[SerializeField] ScrollRect scrollView;
	[SerializeField] GameObject pagePrefab;
	public bool isStartedScrollBar = false;
	public bool isScrolled = false;
	#endregion

	public JobpackDocumentStatus jobPackDocumentStatus;
	public string pdfURL = "";

	PageManager pageManager;

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

	private void Awake() {
		pageManager = GetComponent<PageManager>();
		returnFromjobTaskDocumentButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParam = new Dictionary<string, string> {
					{ "jobpack_no", jobPackDocumentStatus.jobpack_no },
					{ "assignee_id" , pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
				};
			ApiManager.instance.APICallGETWithParameters(
				APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobPackDocuments],
					queryParam,
					() => {
						pageManager.PanelActivation(pageManager.jobTaskPanel.jobTaskPanel);
						ReSetScorllBar();
						ReturnPage();
					},
					pageManager.jobTaskPanel.AssignJobDocuments
					);
		});

		confirmjobTaskDocumentButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			if (jobPackDocumentStatus.new_status != "Reviewed") {
				StartCoroutine(ConfirmButtonPressed());
			} else {
				pageManager.PanelActivation(pageManager.jobTaskPanel.jobTaskPanel);
				ReSetScorllBar();
				ReturnPage();
			}
		});

		ReSetScorllBar();

		if (scrollView != null && scrollView.verticalScrollbar != null) {
			scrollView.verticalScrollbar.onValueChanged.AddListener(OnVerticalScrollChanged);
		}

		refreshButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			LoadPDFDocumentAsync(pdfURL);
			ReSetScorllBar();
			ReturnPage();
		});
	}

	IEnumerator ConfirmButtonPressed() {
		if (jobPackDocumentStatus.document_name != "Permit To Work") {
			jobPackDocumentStatus.new_status = "Reviewed";
		} else {
			jobPackDocumentStatus.new_status = "Approved";
		}
		jobPackDocumentStatus.assignee_id = pageManager.managerControlScript.loginScript.userAccountDetails.user_id;
		string passInParams = JsonUtility.ToJson(jobPackDocumentStatus);
		yield return StartCoroutine(ApiManager.instance.APICallPut(APIPutMiddlePath.APIPutMiddlePathList[(int)APIPutMiddlePathEnum.JobPackDocumentStatus], passInParams));


		var queryParam = new Dictionary<string, string> {
					{ "jobpack_no", jobPackDocumentStatus.jobpack_no },
					{ "assignee_id" , pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
				};
		ApiManager.instance.APICallGETWithParameters(
			APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobPackDocuments],
				queryParam,
				() => {
					pageManager.PanelActivation(pageManager.jobTaskPanel.jobTaskPanel);
				},
				(DownloadHandler downloadHandler) => {
					pageManager.jobTaskPanel.AssignJobDocuments(downloadHandler);
				}
				);
		ReSetScorllBar();
		ReturnPage();
	}

	#region Scroll Bar
	private void ReSetScorllBar() {
		scrollView.verticalScrollbar.value = 1;
		confirmjobTaskDocumentButton.button.enabled = false;
		if (jobPackDocumentStatus.new_status == "Reviewed") {
			isScrolled = true;
		} else {
			isScrolled = false;
		}

		isStartedScrollBar = false;
	}
	private void StartScorllBar() {
		ReSetScorllBar();
		isStartedScrollBar = true;
	}

	public void OnVerticalScrollChanged(float value) {
		if (isStartedScrollBar) {
			if (value <= 0) {
				confirmjobTaskDocumentButton.button.enabled = true;
				isScrolled = true;
			} else {
				if (!isScrolled) {
					confirmjobTaskDocumentButton.button.enabled = false;
				} else {
					confirmjobTaskDocumentButton.button.enabled = true;
				}
			}
		}
	}
	#endregion

	private void Update() {
		// Check if the document promise has finished
		if (documentPromise != null) {
			// Check if the promise has finished
			if (documentPromise.HasFinished) {
				if (documentPromise.HasSucceeded) {
					OnDocumentLoaded(documentPromise.Result);
					//confirmjobTaskDocumentButton.button.gameObject.SetActive(true);
				} else {
					OnDocumentLoadFailed();
				}

				// Clear the promise to avoid repeated checks
				documentPromise = null;
			}
		}
	}

	#region PDF Reading
	public void LoadPDFDocumentAsync(string url) {
		// Start loading the PDF document asynchronously
		documentPromise = PDFDocument.LoadDocumentFromUrlAsync(url);
	}

	private void OnDocumentLoaded(PDFDocument document) {
		// Document successfully loaded
		pdfDocument = document;
		//Debug.Log("PDF Document Loaded: " + pdfDocument.DocumentBuffer.Length + " bytes");
		
		int pageCount = pdfDocument.GetPageCount();

		for (int i = 0; i < pageCount; i++) {
			//for (int i = 0; i < 5; i++) {
			PDFRenderer renderer = new PDFRenderer();
			Vector2 pdfSize = pdfDocument.GetPageSize(i);
			Texture2D tex = renderer.RenderPageToTexture(pdfDocument.GetPage(i), 1024, 1449);
			tex.filterMode = FilterMode.Bilinear;
			tex.anisoLevel = 8;
			Sprite pageSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
			GameObject pageObj = GetPage();
			pageObj.GetComponent<Image>().sprite = pageSprite;
		}
		StartScorllBar();
	}

	private void OnDocumentLoadFailed() {
		// Handle the error
		Debug.LogError("Failed to load PDF document.");
	}
	#endregion

	public void LogoutCalled() {
		ReturnPage();

		jobPackDocumentStatus.jobpack_no = "";
		jobPackDocumentStatus.assignee_id = 0;
		jobPackDocumentStatus.document_name = "";
		jobPackDocumentStatus.new_status = "";
		pdfURL = "";
	}

	public void UIInteractable(bool interactableStatus) {
		returnFromjobTaskDocumentButton.button.enabled = interactableStatus;
		refreshButton.button.enabled = interactableStatus;
		confirmjobTaskDocumentButton.button.enabled = interactableStatus;
		scrollView.enabled = interactableStatus;
	}
}
