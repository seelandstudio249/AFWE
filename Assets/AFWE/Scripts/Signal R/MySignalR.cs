//using Microsoft.AspNetCore.SignalR.Client;
//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.Networking;


//public class MySignalR : MonoBehaviour {
//	public string negotiateUrl = "https://ptsg-5afwe-apim.azure-api.net/ptsg5afwe-func1/notification/negotiate";
//	private static HubConnection connection;
//	[SerializeField] NegotiateInfo signalrResponse;

//	void Start() {
//		StartCoroutine(APICallPOSTWithParam());
//	}

//	private IEnumerator APICallPOSTWithParam() {
//		UnityWebRequest request = new UnityWebRequest(negotiateUrl, "POST");
//		request.downloadHandler = new DownloadHandlerBuffer();
//#if ENABLE_WINMD_SUPPORT
//		var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
//		Debug.Log("Device ID: " + deviceInfo.Id.ToString());
//		request.SetRequestHeader("userId", deviceInfo.Id.ToString());
//#else
//		request.SetRequestHeader("userId", "test_hololens_001");
//#endif
//		yield return request.SendWebRequest();

//		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
//			Debug.LogError($"POST Request Error: {request.error}");
//		} else {
//			if (request.responseCode == 200) {
//				Debug.Log("POST Request successful for SignalR: " + request.downloadHandler.text);
//				signalrResponse = JsonUtility.FromJson<NegotiateInfo>(request.downloadHandler.text);

//				// Connect to SignalR hub
//				StartCoroutine(StartConnect(signalrResponse.url, signalrResponse.accessToken));
//			} else {
//				Debug.LogError($"POST Request Error: {request.responseCode}");
//			}
//		}
//	}
//	IEnumerator StartConnect(string url, string accessToken) {
//		Debug.Log("Initializing connection setup...");
//		Task startTask;

//		try {
//			connection = new HubConnectionBuilder()
//				.WithUrl(url, options => {
//					options.AccessTokenProvider = () => Task.FromResult(accessToken);
//				})
//				.WithAutomaticReconnect()
//				.Build();
//			Debug.Log("SignalR connection object built.");
//			connection.On<string>("ReceiveMessage", (message) => {
//				Debug.Log($"Message received: {message}");
//			});
//			Debug.Log("Event listener set up for 'ReceiveMessage'.");
//			startTask = connection.StartAsync();

//		} catch (Exception ex) {
//			Debug.LogError($"Error during SignalR setup: {ex.Message}");
//			yield break;
//		}
//		yield return new WaitUntil(() => {
//			return startTask.IsCompleted;
//		});

//		DebugLogger.LogInfo("Waiting for connection to complete...");
//		//Debug.Log("Waiting for connection to complete...");
//		if (startTask.Exception != null) {
//			Debug.LogError($"Error connecting to SignalR: {startTask.Exception.Message}");
//			yield break;
//		}
//		Debug.Log("SignalR connection successfully established!");
//		//finally
//		//{
//		//    if (connection != null)
//		//    {
//		//        await connection.DisposeAsync();
//		//    }
//		//}
//	}
//}

//[Serializable]
//public class NegotiateInfo {
//	public string url;
//	public string accessToken;
//}

