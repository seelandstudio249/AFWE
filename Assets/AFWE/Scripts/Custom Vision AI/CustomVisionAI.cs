using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class CustomVisionAI : MonoBehaviour {
	private string apiUrl = "https://testafwewcvai-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/a0c61038-08c9-4aee-a0fc-c251d736bc7e/detect/iterations/Iteration4/url";

	public GameObject quad; // Assign your quad here
	public Material lineMaterial; // Assign a material for the LineRenderer
	private Renderer quadRenderer;

	[SerializeField] PredictionResponse predictionsResponse;

	private void Start() {
		quadRenderer = quad.GetComponent<Renderer>();
		CallApi();
	}

	// Method to call the API
	public void CallApi() {
		StartCoroutine(PostRequest(apiUrl));
	}

	IEnumerator PostRequest(string url) {
		// Create a JSON string for the body
		string jsonBody = "{\"Url\": \"https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTtfpoCDSk8cNMUo98EHR-be8CljB7RxEnELQ&s\"}";

		// Convert the JSON string to a byte array
		byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);

		// Create the UnityWebRequest object and set it up as a POST request
		UnityWebRequest request = new UnityWebRequest(url, "POST");

		// Set the request body
		request.uploadHandler = new UploadHandlerRaw(jsonToSend);
		request.downloadHandler = new DownloadHandlerBuffer();

		// Set request headers
		request.SetRequestHeader("Prediction-Key", "996d5393c2674ed58a0d41b1be01fdec");
		request.SetRequestHeader("Content-Type", "application/json");

		// Send the request and wait for a response
		yield return request.SendWebRequest();

		// Check for errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError("Error: " + request.error);
		} else {
			// Check if response is null or empty
			if (string.IsNullOrEmpty(request.downloadHandler.text)) {
				Debug.LogError("Response is null or empty");
			} else {
				// Successfully received response
				// Step 1: Parse JSON response
				try {
					// Try to deserialize into PredictionResponse
					predictionsResponse = JsonUtility.FromJson<PredictionResponse>(request.downloadHandler.text);

					// Check if predictions are null or empty
					if (predictionsResponse == null || predictionsResponse.predictions == null) {
						Debug.LogError("Prediction response is null or does not contain predictions.");
					} else {
						// Step 2: Call DrawBoundingBoxes with parsed predictions
						DrawBoundingBoxes(predictionsResponse.predictions);
					}
				} catch (System.Exception ex) {
					Debug.LogError("Error in parsing JSON: " + ex.Message);
				}
			}
		}
	}

	// Method to draw bounding boxes on the quad
	public void DrawBoundingBoxes(Prediction[] predictions) {
		// Get the size of the quad in world space
		Vector3 quadSize = quadRenderer.bounds.size;

		// Loop through each prediction
		foreach (var prediction in predictions) {
			// Convert normalized bounding box to quad space
			Vector3[] corners = GetBoundingBoxCorners(prediction.boundingBox, quadSize);

			// Draw the bounding box using LineRenderer
			GameObject boundingBox = new GameObject("BoundingBox");
			LineRenderer lineRenderer = boundingBox.AddComponent<LineRenderer>();

			// Set LineRenderer properties
			lineRenderer.positionCount = 5; // 4 corners + 1 to close the box
			lineRenderer.startWidth = 0.001f; // Adjust width of the line
			lineRenderer.endWidth = 0.001f;
			lineRenderer.material = lineMaterial;

			// Set the corner points
			lineRenderer.SetPosition(0, corners[0]);
			lineRenderer.SetPosition(1, corners[1]);
			lineRenderer.SetPosition(2, corners[2]);
			lineRenderer.SetPosition(3, corners[3]);
			lineRenderer.SetPosition(4, corners[0]); // Close the box
			lineRenderer.transform.parent = quad.transform;
		}
	}

	// Helper method to get bounding box corners
	private Vector3[] GetBoundingBoxCorners(BoundingBox box, Vector3 quadSize) {
		// Convert normalized coordinates to world space on the quad
		float left = (box.left - 0.5f) * quadSize.x; // X-axis offset
		float top = (0.5f - box.top) * quadSize.y;   // Y-axis offset (flip Y)
		float right = left + (box.width * quadSize.x);
		float bottom = top - (box.height * quadSize.y);

		// Return the 4 corners of the bounding box in quad space
		return new Vector3[]
		{
			new Vector3(left, top, quad.transform.position.z),      // Top-left
            new Vector3(right, top, quad.transform.position.z),     // Top-right
            new Vector3(right, bottom, quad.transform.position.z),  // Bottom-right
            new Vector3(left, bottom, quad.transform.position.z)    // Bottom-left
        };
	}
}

// PredictionResponse class to match the structure of the API response
[System.Serializable]
public class PredictionResponse {
	public string id;
	public string project;
	public string iteration;
	public string created;
	public Prediction[] predictions;
}

// Prediction class for individual predictions
[System.Serializable]
public class Prediction {
	public float probability;
	public string tagId;
	public string tagName;
	public BoundingBox boundingBox;
}

// BoundingBox class for bounding box data
[System.Serializable]
public class BoundingBox {
	public float left;
	public float top;
	public float width;
	public float height;
}
