using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Connections;
using UnityEngine.Events;
using UnityEditor;

public class SignalRConnector {
	public Action<string> OnMessageReceived;
	private HubConnection _connection = null;
	public HubConnection Connection => _connection;
	public async Task InitAsync(string url, string accessToken, string handlerMethod) {
		try {
			//Debug.Log($"Initializing SignalR connection to URL: {url}");
			//Debug.Log($"Initializing SignalR connection to URL with Access Token: {accessToken}");
			_connection = new HubConnectionBuilder()
				.WithUrl(url, options => {
					options.AccessTokenProvider = () => Task.FromResult(accessToken);
					options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;

				})
				.WithAutomaticReconnect() // Enable automatic reconnection
				.Build();

			//_connection = new HubConnectionBuilder()
			//.WithUrl("http://192.168.11.32:3002/negotiate") //, { withCredentials: true }) // Replace with server IP
			//.Build();

			//Debug.Log("SignalR connection object created.");

			//_connection.Closed += async (error) => {
			//	DebugLogger.LogInfo($"Connection closed. Error: {error?.Message}");
			//	DebugLogger.LogInfo("SignalR connection closed, attempting to reconnect.");
			//	//await Task.Delay(5000); // Optional: wait before retrying
			//	await StartConnectionAsync(); // Retry connection
			//};

			//_connection.Reconnecting += (error) => {
			//	DebugLogger.LogInfo($"Reconnecting to SignalR. Error: {error?.Message}");
			//	return Task.CompletedTask;
			//};
			//_connection.Reconnected += (connectionId) => {
			//	DebugLogger.LogInfo($"Reconnected to SignalR. New Connection ID: {connectionId}");
			//	return Task.CompletedTask;
			//};

			// Subscribe to SignalR messages
			_connection.On<string>(handlerMethod, (message) => {
				//Debug.Log($"Message received: {message}");
				OnMessageReceived?.Invoke(message);
			});
			await StartConnectionAsync();
		} catch (Exception ex) {
			Debug.LogException(ex);
		}
	}

	public async Task SendMessageAsync(string message) {
		try {
			//Debug.Log($"Sending message: {JsonUtility.ToJson(message)}");
			await _connection.InvokeAsync("SendMessage", message);
			//Debug.Log("Message sent successfully.");
		} catch (Exception ex) {
			Debug.LogException(ex);
		}
	}

	private async Task StartConnectionAsync() {
		try {
			//Debug.Log("Starting SignalR connection...");
			await _connection.StartAsync();
			//Debug.Log($"SignalR connected successfully. Connection ID: {_connection.ConnectionId}");
		} catch (Exception ex) {
			Debug.LogException(ex);
		}
	}
}

public class SignalRScript : MonoBehaviour {
	public static SignalRScript Instance { get; private set; }
	[SerializeField] string negotiateUrl = "https://ptsg-5afwe-apim.azure-api.net/ptsg5afwe-func1/notification/negotiate";
	[SerializeField] NegotiateResponse signalrResponse;
	private readonly SignalRConnector _connector = new();
	Func<Task> urlAndAccessTokenReady;
	bool hasSendRequest = false;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy(gameObject); // Prevent duplicates
		}
		urlAndAccessTokenReady += ConnectToHub;
	}

	void Start() {
		//StartCoroutine(CheckTLS());
		StartCoroutine(APICallPOSTWithParam());
	}

	private IEnumerator APICallPOSTWithParam() {
		//Debug.Log($"Starting POST request to: {negotiateUrl}");
		UnityWebRequest request = new UnityWebRequest(negotiateUrl, "POST");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("userId", "test_hololens_00B");

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			string errorLog = $"POST Request Error: {request.error}";
			//DebugLogger.LogInfo(errorLog);
			Debug.LogError(errorLog);
		} else {
			if (request.responseCode == 200) {
				//Debug.Log("POST Request successful: " + request.downloadHandler.text);
				signalrResponse = JsonUtility.FromJson<NegotiateResponse>(request.downloadHandler.text);
				// Connect to SignalR hub
				urlAndAccessTokenReady.Invoke();
			} else {
				string errorLog = $"POST Request Error: {request.responseCode}";
				//DebugLogger.LogError(errorLog);
				Debug.LogError(errorLog);
			}
		}
	}

	async Task ConnectToHub() {
		if (hasSendRequest) return;
		hasSendRequest = true;
		await _connector.InitAsync(signalrResponse.url, signalrResponse.accessToken, "ReceiveMessage");
	}


	[Serializable]
	private class NegotiateResponse {
		public string url;
		public string accessToken;
	}
}
