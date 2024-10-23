using FishNet;
using FishNet.Discovery;
using FishNet.Managing;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(ManagersControl))]
public class Login : ManagerBaseScript {
	List<string> _endPoints = new List<string>();
	List<List<string>> splittedEndPoints = new List<List<string>>();

	#region UI
	[Header("Login Panel")]
	[SerializeField] GameObject loginPanel;
	[SerializeField] MRButtonClass loginButton;
	[SerializeField] MRTKTMPInputField usernameInputField;
	[SerializeField] TMP_Text errorText;
	#endregion

	int currentPage = 0;

	[Header("Networking Setup")]
	[SerializeField] bool isServer = false;
	[SerializeField] bool isDirectJoinServer = false;
	#region For Network Discovery
	[SerializeField] NetworkDiscovery networkDiscovery;
	[SerializeField] ushort port;
	public ushort GetPort() {
		return port;
	}
	[SerializeField] float searchTimeout;
	public float GetSearchTimeout() {
		return searchTimeout;
	}
	[SerializeField] string secret;
	public string GetSecret() {
		return secret;
	}
	#endregion
	[Space(15)]

	[Header("Game Mode")]
	public GamePlayType gameModeLocal;

	[Header("Player Type")]
	public PlayerType playerType;

	[SerializeField] ManagersControl managersControl;
	QRCodesManager qrCodeManager;

	#region Login Actions
	public Action showHomePageUserDetails;
	#endregion

	protected override void Awake() {
		qrCodeManager = managersControl.GetSpecificManagerScript<QRCodesManager>();

		networkDiscovery.ServerFoundCallback += AddingEndPoint;
		loginButton.button.OnClicked.AddListener(delegate {
			errorText.text = "";
			loginButton.button.gameObject.SetActive(false);

			switch (usernameInputField.text.ToUpper()) {
				case "MT":
				playerType = PlayerType.MT;
				showHomePageUserDetails.Invoke();
				PanelActivation(null);
				managersControl.AfterLogin();
				break;
				case "FO":
				playerType = PlayerType.FO;
				showHomePageUserDetails.Invoke();
				PanelActivation(null);
				managersControl.AfterLogin();
				break;
				case "E":
				playerType = PlayerType.E;
				showHomePageUserDetails.Invoke();
				PanelActivation(null);
				managersControl.AfterLogin();
				break;
				case "Edit":
				playerType = PlayerType.Editor;
				//showHomePageUserDetails.Invoke();
				PanelActivation(null);
				managersControl.AfterLogin();
				break;
				default:
				errorText.text = "User doesn't exist";
				loginButton.button.gameObject.SetActive(true);
				break;
			}
		});
	}

	private void Start() {
		InitialSetup();
	}

	#region Mode Selection
	void InitialSetup() {
		if (isServer) {
			StartServer();
		} else {
			SearchServer();
		}
	}

	void StartServer() {
		_endPoints.Clear();
		splittedEndPoints.Clear();
		InstanceFinder.ServerManager.StartConnection(port);
		networkDiscovery.AdvertiseServer();
		managersControl.AssignGameMode(GamePlayType.Multiplayer);
	}

	void SearchServer() {
		networkDiscovery.SearchForServers();
		errorText.text = "Loading...";
		_endPoints.Clear();
		splittedEndPoints.Clear();
		gameModeLocal = GamePlayType.Multiplayer;
	}

	public IEnumerator FailedServerSearch() {
		errorText.text = "Failed To Connect To Server";
		//loginButton.button.gameObject.SetActive(true);
		yield return new WaitForSeconds(2);
		SearchServer();
	}

	void PanelActivation(GameObject panel = null, bool activationStatus = true) {
		loginPanel.SetActive(false);
		errorText.text = "";
		if (panel) panel.SetActive(activationStatus);
	}

	void AddingEndPoint(IPEndPoint endPoint) {
		if (_endPoints.Contains(endPoint.Address.ToString())) return;
		_endPoints.Add(endPoint.Address.ToString());
		if (isDirectJoinServer) {
			if (_endPoints[0] != null) JoinServer(_endPoints[0]);
			PanelActivation(null);
		} else {
			StartCoroutine(AssigningButton(0));
		}
	}

	IEnumerator AssigningButton(float secondsToWait) {
		yield return new WaitForSeconds(secondsToWait);
		if (splittedEndPoints.Count > 0 && splittedEndPoints[currentPage].Count > 0) {
			for (int i = 0; i < splittedEndPoints[currentPage].Count; i++) {
				string ipAddress = splittedEndPoints[currentPage][i];
			}
		}
	}

	void JoinServer(string ipAddress) {
		InstanceFinder.ClientManager.StartConnection(ipAddress, port);
		networkDiscovery.isIpButtonPressed = true;
		networkDiscovery.StopSearchingOrAdvertising();
		managersControl.AssignGameMode(GamePlayType.Multiplayer);
		qrCodeManager.StartQRTracking();
	}
	#endregion
}
