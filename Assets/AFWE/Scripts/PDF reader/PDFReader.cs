using Paroxe.PdfRenderer;
using Paroxe.PdfRenderer.WebGL;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PDFReader : ManagerBaseScript {
	private PDFDocument pdfDocument;
	private PDFJS_Promise<PDFDocument> documentPromise;
	public int m_Page = 0;
	[SerializeField] MeshRenderer m_renderer;
	// Start is called before the first frame update
	void Start() {
		// Call the method to load the document
		LoadPDFDocumentAsync("https://www.open-std.org/jtc1/sc22/wg21/docs/papers/2013/n3690.pdf");
	}

	private void LoadPDFDocumentAsync(string url) {
		// Start loading the PDF document asynchronously
		documentPromise = PDFDocument.LoadDocumentFromUrlAsync(url);
	}

	private void Update() {
		// Check if the document promise has finished
		if (documentPromise != null) {
			// Check if the promise has finished
			if (documentPromise.HasFinished) {
				if (documentPromise.HasSucceeded) {
					OnDocumentLoaded(documentPromise.Result);
				} else {
					OnDocumentLoadFailed();
				}

				// Clear the promise to avoid repeated checks
				documentPromise = null;
			}
		}
	}

	private void OnDocumentLoaded(PDFDocument document) {
		// Document successfully loaded
		pdfDocument = document;
		//Debug.Log("PDF Document Loaded: " + pdfDocument.DocumentBuffer.Length + " bytes");

		int pageCount = pdfDocument.GetPageCount();

		PDFRenderer renderer = new PDFRenderer();
		Texture2D tex = renderer.RenderPageToTexture(pdfDocument.GetPage(m_Page % pageCount), 1024, 1024);
		//m_texture = renderer.RenderPageToTexture(pdfDocument.GetPage(1), 1024, 1024);

		tex.filterMode = FilterMode.Bilinear;
		tex.anisoLevel = 8;

		m_renderer.material.mainTexture = tex;
	}

	private void OnDocumentLoadFailed() {
		// Handle the error
		Debug.LogError("Failed to load PDF document.");
	}
}
