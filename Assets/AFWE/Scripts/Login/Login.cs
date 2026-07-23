//using FishNet;
//using FishNet.Discovery;
//using FishNet.Managing;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR.Client;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class NegotiateInfo {
	public string url;
	public string accessToken;
}

[Serializable]
public class UserAccountDetails {
	//public LoginFromSQLServerResponse userLoginFromSqlServerData;
	public int login_id;
	public int user_id;
	public string username;
	public string first_name;
	public string last_name;
	public string role;
	public string opu;
	public int currentTaskCount;
	public string userProfilePic;
	public string connectionID;
	public string deviceID;
}

[RequireComponent(typeof(ManagersControl))]
public class Login : ManagerBaseScript {
	#region Settings
	[SerializeField] bool isActiveSignalR;
	#endregion

	#region UI
	[Header("Login Panel")]
	[SerializeField] GameObject loginPanel;
	[SerializeField] MRButtonClass maintenanceTechnician, fieldOperator, electrician;
	[SerializeField] PressableButton usernameButton, passwordButton;
	[SerializeField] MRTKTMPInputField usernameInputField;
	[SerializeField] MRTKTMPInputField passwordInputField;
	[SerializeField] TMP_Text errorText;
	#endregion

	[Header("Networking Setup")]
	[SerializeField] bool isServer = false;
	[SerializeField] bool isDirectJoinServer = false;
	[Space(15)]

	[Header("Game Mode")]
	public GamePlayType gameModeLocal;

	[Header("Player Type")]
	public PlayerType playerType;

	[SerializeField] ManagersControl managersControl;

	bool alrdyStartTakingPhoto = false;

	#region Login Actions
	public Action showHomePageUserDetails;
	#endregion

	#region Signal R
	public string negotiateUrl = "https://ptsg-5afwe-apim.azure-api.net/ptsg5afwe-func1/notification/negotiate";
	private static HubConnection connection;
	[SerializeField] NegotiateInfo signalrResponse;
	bool hasSubcribeSignalR = false;
	#endregion

	public UserAccountDetails userAccountDetails;

	protected override void Awake() {
		maintenanceTechnician.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			string jsonDataLogin = JsonUtility.ToJson(new UserCredentials("789", "789"));
			ApiManager.instance.StartCoroutine(ApiManager.instance.APICallPOST(
				APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.UserCredentials],
				jsonDataLogin,
				() => { }
				, LoginSuccess));
		});
		fieldOperator.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			string jsonDataLogin = JsonUtility.ToJson(new UserCredentials("123", "123"));
			ApiManager.instance.StartCoroutine(ApiManager.instance.APICallPOST(
				APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.UserCredentials],
				jsonDataLogin,
				() => { }
				, LoginSuccess));
		});
		electrician.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			string jsonDataLogin = JsonUtility.ToJson(new UserCredentials("456", "456"));
			ApiManager.instance.StartCoroutine(ApiManager.instance.APICallPOST(
				APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.UserCredentials],
				jsonDataLogin,
				() => { }
				, LoginSuccess));
		});
	}

	private void Start() {
		InitialSetup();
	}

	private void LoginSuccess(DownloadHandler returnedID) {
		UserCredentialsResponse loginFromSQLServerResponse = JsonUtility.FromJson<UserCredentialsResponse>(returnedID.text);

		userAccountDetails.login_id = loginFromSQLServerResponse.login_id;

		var queryParamsGetUserID = new Dictionary<string, string>
		{
			{ "login_id", userAccountDetails.login_id.ToString() }
		};

		ApiManager.instance.APICallGETWithParameters(
			APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.UserID],
			queryParamsGetUserID,
			() => { },
			ProcessGetUserIDResponse);
	}

	private void ProcessGetUserIDResponse(DownloadHandler downloadHandler) {
		UserIDResponse getUserIDResponse = JsonUtility.FromJson<UserIDResponse>(downloadHandler.text);
		userAccountDetails.user_id = getUserIDResponse.user_id;

		var queryParamsGetUserAttributes = new Dictionary<string, string> {
			{"user_id",  userAccountDetails.user_id.ToString() }
		};
		ApiManager.instance.APICallGETWithParameters(
			APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.UserAttributes],
			queryParamsGetUserAttributes,
			() => { },
			ProcessGetUserAttributesResponse);
	}

	public void ProcessGetUserAttributesResponse(DownloadHandler downloadHandler) {
		UserAttributesResponse getUserIDResponse = JsonUtility.FromJson<UserAttributesResponse>(downloadHandler.text);
		userAccountDetails.username = getUserIDResponse.username;
		userAccountDetails.first_name = getUserIDResponse.first_name;
		userAccountDetails.last_name = getUserIDResponse.last_name;
		userAccountDetails.role = getUserIDResponse.role;
		userAccountDetails.currentTaskCount = getUserIDResponse.etask_count;
		userAccountDetails.userProfilePic = getUserIDResponse.base64_img;
		switch (userAccountDetails.role) {
			case "Field Operator":
			playerType = PlayerType.FO;
			break;
			case "Maintenance Technician":
			playerType = PlayerType.MT;
			break;
			case "Electrician":
			playerType = PlayerType.E;
			break;
		}
		userAccountDetails.opu = getUserIDResponse.opu;

		showHomePageUserDetails?.Invoke();
		PanelActivation(null);
		managersControl.AfterLogin();

		if (!hasSubcribeSignalR) {
			StartCoroutine(APICallPOSTWithParam());
		}
	}

	void InitialSetup() {
		gameModeLocal = GamePlayType.Singleplayer;
	}

	void PanelActivation(GameObject panel = null, bool activationStatus = true) {
		loginPanel.SetActive(false);
		errorText.text = "";
		if (panel) panel.SetActive(activationStatus);
	}

	public void ProceedLogout() {
		usernameInputField.text = "";
		passwordInputField.text = "";
		loginPanel.SetActive(true);
		userAccountDetails.login_id = 0;
		userAccountDetails.user_id = 0;
		userAccountDetails.username = "";
		userAccountDetails.last_name = "";
		userAccountDetails.role = "";
		userAccountDetails.opu = "";
		PageManager pageManager = managersControl.GetSpecificManagerScript<PageManager>();
		pageManager.LogoutCalled();
		//loginButton.button.gameObject.SetActive(true);
		QRCodesManager.instance.LogoutCalled();
		//UnityEngine.SceneManagement.SceneManager.LoadScene("Actual Scene");
	}

	public void UIInteractable(bool interactableStatus) {
		//loginButton.button.enabled = interactableStatus;
		maintenanceTechnician.button.enabled = interactableStatus;
		fieldOperator.button.enabled = interactableStatus;
		electrician.button.enabled = interactableStatus;
		usernameInputField.enabled = interactableStatus;
		usernameButton.enabled = interactableStatus;
		passwordInputField.enabled = interactableStatus;
		passwordButton.enabled = interactableStatus;
	}

	private IEnumerator APICallPOSTWithParam() {
		UnityWebRequest request = new UnityWebRequest(negotiateUrl, "POST");
		request.downloadHandler = new DownloadHandlerBuffer();
#if ENABLE_WINMD_SUPPORT
		var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
		//Debug.Log("Device ID: " + deviceInfo.Id.ToString());
		request.SetRequestHeader("userId", deviceInfo.Id.ToString());
		userAccountDetails.deviceID = deviceInfo.Id.ToString();
#else
		request.SetRequestHeader("userId", "Brandon Laptop");
		userAccountDetails.deviceID = "Brandon Laptop";
#endif
		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			//Debug.LogError($"POST Request Error: {request.error}");
		} else {
			if (request.responseCode == 200) {
				//Debug.Log("POST Request successful for SignalR: " + request.downloadHandler.text);
				signalrResponse = JsonUtility.FromJson<NegotiateInfo>(request.downloadHandler.text);

				// Connect to SignalR hub
				if (isActiveSignalR) {
					StartCoroutine(StartConnect(signalrResponse.url, signalrResponse.accessToken));
				} else {
					if (!alrdyStartTakingPhoto) {
						//managersControl.pageManager.cameraPanel.StartTakingPhotoEverySecond();
						managersControl.pageManager.cameraPanel.canDoHazardDetection = true;
						StartCoroutine(managersControl.pageManager.cameraPanel.InteractWithSpatialMapRepeatedly());
						alrdyStartTakingPhoto = true;
						hasSubcribeSignalR = true;
					}
				}
			} else {
				//Debug.LogError($"POST Request Error: {request.responseCode}");
			}
		}
	}
	IEnumerator StartConnect(string url, string accessToken) {
		//Debug.Log("Initializing connection setup...");
		Task startTask;

		try {
			connection = new HubConnectionBuilder()
				.WithUrl(url, options => {
					options.AccessTokenProvider = () => Task.FromResult(accessToken);
				})
				.WithAutomaticReconnect()
				.Build();
			//Debug.Log("SignalR connection object built.");
			connection.On<string>("ReceiveMessage", (message) => {
				//Debug.Log($"Message received: {message}");
				managersControl.pageManager.hazardDetectionManager.ReceivedNotification.Invoke(message);
			});
			//Debug.Log("Event listener set up for 'ReceiveMessage'.");
			startTask = connection.StartAsync();

		} catch (Exception ex) {
			Debug.LogException(ex);
			yield break;
		}
		yield return new WaitUntil(() => {
			return startTask.IsCompleted;
		});

		//Debug.Log("Waiting for connection to complete...");
		//DebugLogger.Log("Waiting for connection to complete...");
		if (startTask.Exception != null) {
			Debug.LogError($"Error connecting to SignalR: {startTask.Exception.Message}");
			yield break;
		}
		//Debug.Log("SignalR connection successfully established!");
		if (!alrdyStartTakingPhoto) {
			//managersControl.pageManager.cameraPanel.StartTakingPhotoEverySecond();
			managersControl.pageManager.cameraPanel.canDoHazardDetection = true;
			StartCoroutine(managersControl.pageManager.cameraPanel.InteractWithSpatialMapRepeatedly());
			alrdyStartTakingPhoto = true;
			hasSubcribeSignalR = true;
		}
	}
}
