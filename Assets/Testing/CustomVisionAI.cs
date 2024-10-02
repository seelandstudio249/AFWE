using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomVisionAI : MonoBehaviour
{
    private string apiUrl = "https://testafwewcvai-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/a0c61038-08c9-4aee-a0fc-c251d736bc7e/detect/iterations/Iteration1/url";

    private void Start() {
        CallApi();
    }

    // Example method to call API
    public void CallApi() {
        StartCoroutine(PostRequest(apiUrl));
    }

    IEnumerator PostRequest(string url) {
        // Create a JSON string for the body
        string jsonBody = "{\"Url\": \"https://hobbylink.tv/wp-content/uploads/2010/11/img_5497-1024x683.jpg\"}";

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
            // Successfully received response
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}
