using Paroxe.PdfRenderer;
using Paroxe.PdfRenderer.Internal.Viewer;
using Paroxe.PdfRenderer.WebGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class EquipmentIncidentsResponseWrapper {
	public List<EquipmentIncidentsResponse> equipmentIncidentsResponses = new List<EquipmentIncidentsResponse>();
}

public class CautionPanel : MonoBehaviour {
	[SerializeField] EquipmentIncidentsResponseWrapper equipmentIncidentsResponseWrapper;

	[SerializeField] MRButtonClass cautionIconButton;

	public GameObject cautionPanel;
	[SerializeField] GameObject cautionContentItemPrefab;
	[SerializeField] ScrollRect scrollViewCautionPanel;

	#region PDF Reading
	[SerializeField] GameObject pdfPanel;
	[SerializeField] MRButtonClass backFromPdfButton;
	private PDFDocument pdfDocument;
	private PDFJS_Promise<PDFDocument> documentPromise;
	public int m_Page = 0;
	[SerializeField] ScrollRect pdfScrollView;
	[SerializeField] GameObject pagePrefab;
	public bool isStartedScrollBar = false;
	public bool isScrolled = false;
	#endregion

	#region Object Pooling 
	private Queue<GameObject> pdfPool = new Queue<GameObject>();
	private Queue<GameObject> buttonfPool = new Queue<GameObject>();

	public GameObject GetPage() {
		if (pdfPool.Count > 0) {
			GameObject pooledPage = pdfPool.Dequeue();
			pooledPage.SetActive(true);
			return pooledPage;
		} else {
			return Instantiate(pagePrefab, pdfScrollView.content);
		}
	}

	public void ReturnPage() {
		pdfPool.Clear();

		foreach (Transform child in pdfScrollView.content) {
			GameObject pooledPage = child.gameObject;
			pooledPage.SetActive(false);
			pdfPool.Enqueue(pooledPage);
		}
	}

	public GameObject GetButton() {
		if (buttonfPool.Count > 0) {
			GameObject pooledPage = buttonfPool.Dequeue();
			pooledPage.SetActive(true);
			return pooledPage;
		} else {
			return Instantiate(cautionContentItemPrefab, scrollViewCautionPanel.content);
		}
	}

	public void ReturnButton() {
		buttonfPool.Clear();

		foreach (Transform child in scrollViewCautionPanel.content) {
			GameObject pooledPage = child.gameObject;
			pooledPage.SetActive(false);
			pdfPool.Enqueue(pooledPage);
		}
	}
	#endregion

	private void Awake() {
		cautionIconButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			if (scrollViewCautionPanel.content.gameObject.active) {
				ReturnButton();
				scrollViewCautionPanel.content.gameObject.SetActive(false);
				pdfPanel.SetActive(false);
			} else {
				scrollViewCautionPanel.content.gameObject.SetActive(true);
				foreach (EquipmentIncidentsResponse item in equipmentIncidentsResponseWrapper.equipmentIncidentsResponses) {
					GameObject itemButton = GetButton();
					MRTKCustomizedButtonScript buttonScript = itemButton.GetComponent<MRTKCustomizedButtonScript>();
					buttonScript.buttonClass.buttonText.text = item.incident_description;
					buttonScript.buttonClass.button.OnClicked.AddListener(delegate {
						AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
						pdfPanel.SetActive(true);
						ReturnPage();
						LoadPDFDocumentAsync(item.blob_url);
						pdfScrollView.verticalScrollbar.value = 1;
					});
				}
			}
		});
		backFromPdfButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			ReturnPage();
			pdfPanel.SetActive(false);
		});
	}

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

	public void AssignequipmentIncidents(DownloadHandler returnedItems) {
		string wrappedJson = "{ \"equipmentIncidentsResponses\": " + returnedItems.text + " }";
		EquipmentIncidentsResponseWrapper wrapper = JsonUtility.FromJson<EquipmentIncidentsResponseWrapper>(wrappedJson);

		if (wrapper != null && wrapper.equipmentIncidentsResponses != null) {
			equipmentIncidentsResponseWrapper = wrapper;
		}
		cautionPanel.SetActive(true);
	}

	IEnumerator Countdown(float duration) {
		yield return new WaitForSeconds(duration);
		cautionPanel.SetActive(false);
	}

	public void LogoutCalled() {
		ReturnButton();
		ReturnPage();
		cautionPanel.SetActive(false);
		pdfPanel.SetActive(false);
	}

	public void UIInteractable(bool status) {
		foreach (Transform child in pdfScrollView.content) {
			MRTKCustomizedButtonScript buttonScript = child.GetComponent<MRTKCustomizedButtonScript>();
			buttonScript.buttonClass.button.enabled = status;
		}
		backFromPdfButton.button.enabled = status;
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
			Texture2D tex = renderer.RenderPageToTexture(pdfDocument.GetPage(i), 1024, 1024);
			tex.filterMode = FilterMode.Bilinear;
			tex.anisoLevel = 8;
			Sprite pageSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
			GameObject pageObj = GetPage();
			pageObj.GetComponent<Image>().sprite = pageSprite;
		}
	}

	private void OnDocumentLoadFailed() {
		// Handle the error
		Debug.LogError("Failed to load PDF document.");
	}
	#endregion
}
