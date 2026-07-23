using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

[Serializable]
public class NotificationPanel {
	public GameObject panel;
	public TMP_Text title;
}

[Serializable]
public class HazardDetectionHeaderButton : MRButtonClass {
	public GameObject buttonParent;
	public TMP_Text hazardAmountTextCount;
}

[Serializable]
public class HazardDetectionDetailsPanelButton : MRButtonClass {
	public GameObject selectedSprite;
}

[Serializable]
public enum HazardType {
	ForeignObjects,
	SlipperyFloor,
	ProtrudingObjects,
	UnevenSurface
}

[Serializable]
public class HazardReport {
	public int hazard_id;

	public HazardReport(int hazard_id) {
		this.hazard_id = hazard_id;
	}
}

public class HazardDetectionManager : ManagerBaseScript {
    [SerializeField] PageManager pageManager;

	#region Panel Settings
	public GameObject detailsHazardPanel;
	[SerializeField] string foreignObject, slipperyFloor, protrudingObject, unevenSurface;
	[SerializeField] HazardDetectionHeaderButton hazardDetectionHeaderButton;
	[SerializeField] MRButtonClass closeDetailsHazardPanelButton;
	[SerializeField] HazardDetectionDetailsPanelButton detailsPanelForiegnObjectButton, detailsPanelSlipperyFloorButton, detailsPanelProtrudingObjectButton, detailsPanelUnevenSurfaceButton;
	[SerializeField] MRButtonClass nextHazardDetails, previousHazardDetails, resolvedHazard, createHazardReport;
	[SerializeField] TMP_Text foriegnObjectCountText, slipperryFloorCountText, protrudingObjectCountText, unevenSurfaceCountText, contentText;
	[SerializeField] RawImage imageDisplay;
	[SerializeField] Texture2D texture;
	[SerializeField] GameObject hazardReportStatus;
	#endregion
	[SerializeField] HazardType currentHazardType;
	#region Actions
	public Action<string> ReceivedNotification;
	#endregion

	#region Pin Point Object in Real life
	CameraTransform cameraTransform;
	[SerializeField] GameObject hazardDetectedPrefab;
	#endregion

	[SerializeField] List<SignalRNotificationMessage> foreignObjectHazardsList = new List<SignalRNotificationMessage>();
	[SerializeField] List<SignalRNotificationMessage> slipperyFloorHazardsList = new List<SignalRNotificationMessage>();
	[SerializeField] List<SignalRNotificationMessage> ProtrudingObjectHazardsList = new List<SignalRNotificationMessage>();
	[SerializeField] List<SignalRNotificationMessage> unevenSurfaceHazardsList = new List<SignalRNotificationMessage>();
	[SerializeField] SignalRNotificationMessage currentSelectedHazard;

	bool currentlyInHazardDetectionDetailsPanel = false;

	protected override void Awake() {
		base.Awake();
		ReceivedNotification += AddingNotificationIntoHazardDetectionList;

		hazardDetectionHeaderButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			ShowDetailsHazardDetectionPanel(true);
		});

		detailsPanelForiegnObjectButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			currentSelectedHazard = foreignObjectHazardsList.Where(hazard => hazard.hazard_type == foreignObject && hazard.isResolved == false).FirstOrDefault();
			currentHazardType = HazardType.ForeignObjects;
			ShowDetailsHazardDetectionPanel(false);

		});

		detailsPanelSlipperyFloorButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			currentSelectedHazard = slipperyFloorHazardsList.Where(hazard => hazard.hazard_type == slipperyFloor && hazard.isResolved == false).FirstOrDefault();
			currentHazardType = HazardType.SlipperyFloor;
			ShowDetailsHazardDetectionPanel(false);
		});

		detailsPanelProtrudingObjectButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			currentSelectedHazard = ProtrudingObjectHazardsList.Where(hazard => hazard.hazard_type == protrudingObject && hazard.isResolved == false).FirstOrDefault();
			currentHazardType = HazardType.ProtrudingObjects;
			ShowDetailsHazardDetectionPanel(false);
		});

		detailsPanelUnevenSurfaceButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			currentSelectedHazard = unevenSurfaceHazardsList.Where(hazard => hazard.hazard_type == unevenSurface && hazard.isResolved == false).FirstOrDefault();
			currentHazardType = HazardType.UnevenSurface;
			ShowDetailsHazardDetectionPanel(false);
		});

		closeDetailsHazardPanelButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			currentlyInHazardDetectionDetailsPanel = false;
			CloseDetailsHazardDetectionPanel();
		});

		nextHazardDetails.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchHazardDetails(1);

		});

		previousHazardDetails.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SwitchHazardDetails(-1);
		});

		resolvedHazard.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			UpdateHazardList();
		});

		createHazardReport.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			string jsonHazardData = JsonUtility.ToJson(new HazardReport(currentSelectedHazard.hazard_id));
			ApiManager.instance.StartCoroutine(ApiManager.instance.APICallPOST(
				APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.ActiveHazardUaucReportCreate],
				jsonHazardData, () => {
					currentSelectedHazard.isReportCreated = true;
					SwitchHazardDetails(0);
					StartCoroutine(PromptHazardReportCreated());
				}));
		});
	}

	void Start() {

	}

	public void LogoutCalled() {
		ManagerActivation(false);
		hazardDetectionHeaderButton.buttonParent.gameObject.SetActive(false);
		detailsHazardPanel.SetActive(false);
	}

	public void UIInteractable(bool status) {
		hazardDetectionHeaderButton.button.enabled = status;
		closeDetailsHazardPanelButton.button.enabled = status;
		nextHazardDetails.button.enabled = status;
		previousHazardDetails.button.enabled = status;
		resolvedHazard.button.enabled = status;
	}

	void AddingNotificationIntoHazardDetectionList(string message) {
		Debug.Log($"Hazard Detected: {message}");
		try {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.hazardDetectedSfx);
			SignalRNotificationMessage notification = JsonUtility.FromJson<SignalRNotificationMessage>(message);
			if (notification == null) return;

			notification.annotated_image_url = notification.annotated_image_url.Replace("+", "%2B");
			notification.isResolved = false;
			notification.detectedHazardObjects = new List<GameObject>();

			foreach(ObjectPositionInPhoto item in notification.objects) {
				cameraTransform = new CameraTransform {
					Position = new Vector3(notification.posx, notification.posy, notification.posz),
					Forward = new Vector3(notification.forwardx, notification.forwardy, notification.forwardz),
					Right = new Vector3(notification.rightx, notification.righty, notification.rightz),
					Up = new Vector3(notification.upx, notification.upy, notification.upz)
				};
				Vector3? spawnPosition = PositionCalculator.CalculatePointInSpace(new Vector2((item.xmin + item.xmax) / 2, (item.ymin + item.ymax) / 2), cameraTransform);

				if(spawnPosition != null) {
					Vector3 spawnPoint = spawnPosition.Value;
					GameObject spawnedHazardDetectedPrefab = Instantiate(hazardDetectedPrefab, spawnPoint, Quaternion.identity);
					HazardDetectedObject hazardObjectScript = spawnedHazardDetectedPrefab.GetComponent<HazardDetectedObject>();
					hazardObjectScript.SetName(notification.hazard_type);
					notification.detectedHazardObjects.Add(spawnedHazardDetectedPrefab);
				}
			}

			// Categorize hazard type
			if (notification.hazard_type == foreignObject) {
				foreignObjectHazardsList.Add(notification);
				currentHazardType = HazardType.ForeignObjects;
			} else if (notification.hazard_type == slipperyFloor) {
				slipperyFloorHazardsList.Add(notification);
				currentHazardType = HazardType.SlipperyFloor;
			} else if (notification.hazard_type == protrudingObject) {
				ProtrudingObjectHazardsList.Add(notification);
				currentHazardType = HazardType.ProtrudingObjects;
			} else if (notification.hazard_type == unevenSurface) {
				unevenSurfaceHazardsList.Add(notification);
				currentHazardType = HazardType.UnevenSurface;
			}

			// UI updates
			hazardDetectionHeaderButton.buttonText.text = $"Hazard Detection ({notification.hazard_type})";
			if (!currentlyInHazardDetectionDetailsPanel) currentSelectedHazard = notification;
			UpdateButtonsHazardList();
		} catch (JsonException jsonEx) {
			Debug.LogException(jsonEx);
		} catch (Exception ex) {
			Debug.LogException(ex);
		}
	}

	void UpdateHazardList() {
		// Mark current hazard as resolved
		if (currentSelectedHazard != null) {
			currentSelectedHazard.isResolved = true;
			foreach(GameObject obj in currentSelectedHazard.detectedHazardObjects) {
				obj.SetActive(false);
			}
		}
		// Update counts in UI
		UpdateHazardCounts();

		// Try to find the next unresolved hazard in the same type
		currentSelectedHazard = GetNextUnresolvedHazard(currentHazardType);

		if (currentSelectedHazard == null) {
			// If no unresolved hazards in the current type, switch to the next type
			bool hasUnresolvedHazards = SwitchToNextAvailableHazardType();

			if (!hasUnresolvedHazards) {
				// If all hazards are resolved, close the panel
				CloseDetailsHazardDetectionPanel();
				return;
			}
		}

		// Update UI to show the next hazard
		ShowContent();
	}

	void UpdateHazardCounts() {
		foriegnObjectCountText.text = foreignObjectHazardsList.Count(h => !h.isResolved).ToString();
		slipperryFloorCountText.text = slipperyFloorHazardsList.Count(h => !h.isResolved).ToString();
		protrudingObjectCountText.text = ProtrudingObjectHazardsList.Count(h => !h.isResolved).ToString();
		unevenSurfaceCountText.text = unevenSurfaceHazardsList.Count(h => !h.isResolved).ToString();
	}

	SignalRNotificationMessage GetNextUnresolvedHazard(HazardType hazardType) {
		switch (hazardType) {
			case HazardType.ForeignObjects:
			return foreignObjectHazardsList.FirstOrDefault(h => !h.isResolved);
			case HazardType.SlipperyFloor:
			return slipperyFloorHazardsList.FirstOrDefault(h => !h.isResolved);
			case HazardType.ProtrudingObjects:
			return ProtrudingObjectHazardsList.FirstOrDefault(h => !h.isResolved);
			case HazardType.UnevenSurface:
			return unevenSurfaceHazardsList.FirstOrDefault(h => !h.isResolved);
			default:
			return null;
		}
	}

	bool SwitchToNextAvailableHazardType() {
		// Define hazard types in order
		var hazardTypes = new[] {
		HazardType.ForeignObjects,
		HazardType.SlipperyFloor,
		HazardType.ProtrudingObjects,
		HazardType.UnevenSurface
	};

		// Iterate over hazard types starting from the current type
		int currentIndex = Array.IndexOf(hazardTypes, currentHazardType);
		for (int i = 0; i < hazardTypes.Length; i++) {
			int nextIndex = (currentIndex + 1 + i) % hazardTypes.Length; // Loop back to the start
			var nextType = hazardTypes[nextIndex];

			var nextHazard = GetNextUnresolvedHazard(nextType);
			if (nextHazard != null) {
				// Switch to the next unresolved hazard
				currentHazardType = nextType;
				currentSelectedHazard = nextHazard;
				hazardDetectionHeaderButton.buttonText.text = $"Hazard Detection ({currentHazardType})";
				return true; // Found an unresolved hazard
			}
		}

		// No unresolved hazards in any type
		return false;
	}

	void UpdateButtonsHazardList() {
		var hazardCounts = new Dictionary<HazardType, (int count, MRButtonClass button, TMP_Text countText)> {
			{ HazardType.ForeignObjects, (foreignObjectHazardsList.Count(h => !h.isResolved), detailsPanelForiegnObjectButton, foriegnObjectCountText) },
			{ HazardType.SlipperyFloor, (slipperyFloorHazardsList.Count(h => !h.isResolved), detailsPanelSlipperyFloorButton, slipperryFloorCountText) },
			{ HazardType.ProtrudingObjects, (ProtrudingObjectHazardsList.Count(h => !h.isResolved), detailsPanelProtrudingObjectButton, protrudingObjectCountText) },
			{ HazardType.UnevenSurface, (unevenSurfaceHazardsList.Count(h => !h.isResolved), detailsPanelUnevenSurfaceButton, unevenSurfaceCountText) }
		};
		foreach (var hazard in hazardCounts) {
			UpdateButtonUI(hazard.Value.count, hazard.Value.button, hazard.Value.countText);
		}
		int totalHazardDetected = hazardCounts.Values.Sum(h => h.count);
		hazardDetectionHeaderButton.hazardAmountTextCount.text = totalHazardDetected.ToString();
		hazardDetectionHeaderButton.buttonParent.SetActive(totalHazardDetected > 0);
		UpdateNavigationButtons(hazardCounts[currentHazardType].count);
	}

	void UpdateButtonUI(int count, MRButtonClass button, TMP_Text countText) {
		button.button.enabled = count > 0;
		countText.text = count.ToString();
	}

	void UpdateNavigationButtons(int currentHazardCount) {
		bool multipleHazards = currentHazardCount > 1;
		nextHazardDetails.button.gameObject.SetActive(multipleHazards);
		previousHazardDetails.button.gameObject.SetActive(multipleHazards);
	}

	void SwitchHazardDetails(int direction) {
		List<SignalRNotificationMessage> currentHazardsList = null;
		switch (currentHazardType) {
			case HazardType.ForeignObjects:
			currentHazardsList = foreignObjectHazardsList;
			break;
			case HazardType.SlipperyFloor:
			currentHazardsList = slipperyFloorHazardsList;
			break;
			case HazardType.ProtrudingObjects:
			currentHazardsList = ProtrudingObjectHazardsList;
			break;
			case HazardType.UnevenSurface:
			currentHazardsList = unevenSurfaceHazardsList;
			break;
		}
		if (currentHazardsList == null || currentHazardsList.Count == 0) return;
		var unresolvedHazards = currentHazardsList.Where(hazard => !hazard.isResolved).ToList();
		if (unresolvedHazards.Count == 0) {
			contentText.text = "All hazards in this category have been resolved.";
			currentSelectedHazard = null;
			return;
		}
		int currentIndex = unresolvedHazards.IndexOf(currentSelectedHazard);
		int nextIndex = (currentIndex + direction) % unresolvedHazards.Count;
		if (nextIndex < 0) {
			nextIndex = unresolvedHazards.Count - 1;
		}
		currentSelectedHazard = unresolvedHazards[nextIndex];
		if (currentSelectedHazard.isReportCreated) {
			createHazardReport.button.enabled = false;
		} else {
			createHazardReport.button.enabled = true;
		}
		ShowContent();
	}

	void ShowDetailsHazardDetectionPanel(bool reposition) {
		currentlyInHazardDetectionDetailsPanel = true;
		if (reposition) {
			Vector3 cameraPosition = Camera.main.transform.position;
			Vector3 cameraForward = Camera.main.transform.forward;
			Vector3 spawnPosition = cameraPosition + cameraForward * .5f;
			Quaternion spawnRotation = Camera.main.transform.rotation;
			detailsHazardPanel.transform.position = spawnPosition;
			detailsHazardPanel.transform.localRotation = spawnRotation;
		}
		pageManager.ManagerActivation(false);
		ManagerActivation(true);
		detailsHazardPanel.SetActive(true);
		//ShowContent();
		SwitchHazardDetails(0);
		hazardDetectionHeaderButton.buttonParent.gameObject.SetActive(false);
	}

	void ShowContent() {
		if (currentSelectedHazard != null) {
			contentText.text = $"{currentSelectedHazard.control_measure}";
			StartCoroutine(LoadImageFromURL(currentSelectedHazard.annotated_image_url));
			CurrentSelectedSpriteActive();
		} else {
			contentText.text = "All hazards in this category had been resolved, either close this panel or proceed to next category";
			imageDisplay.texture = null;
		}
	}

	void CurrentSelectedSpriteActive() {
		detailsPanelForiegnObjectButton.selectedSprite.SetActive(false);
		detailsPanelSlipperyFloorButton.selectedSprite.SetActive(false);
		detailsPanelProtrudingObjectButton.selectedSprite.SetActive(false);
		detailsPanelUnevenSurfaceButton.selectedSprite.SetActive(false);
		switch (currentHazardType) {
			case HazardType.ForeignObjects:
			detailsPanelForiegnObjectButton.selectedSprite.SetActive(true);
			break;
			case HazardType.SlipperyFloor:
			detailsPanelSlipperyFloorButton.selectedSprite.SetActive(true);
			break;
			case HazardType.ProtrudingObjects:
			detailsPanelProtrudingObjectButton.selectedSprite.SetActive(true);
			break;
			case HazardType.UnevenSurface:
			detailsPanelUnevenSurfaceButton.selectedSprite.SetActive(true);
			break;
		}
	}

	IEnumerator LoadImageFromURL(string url) {
		using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url)) {
			yield return webRequest.SendWebRequest();
			if (webRequest.result == UnityWebRequest.Result.Success) {
				texture = DownloadHandlerTexture.GetContent(webRequest);
				imageDisplay.texture = texture; // Assign the texture to RawImage
			}
		}
	}

	void CloseDetailsHazardDetectionPanel() {
		pageManager.ManagerActivation(true);
		detailsHazardPanel.SetActive(false);
		UpdateButtonsHazardList();
	}

	IEnumerator PromptHazardReportCreated() {
		hazardReportStatus.SetActive(true);
		yield return new WaitForSeconds(3);
		hazardReportStatus.SetActive(false);
	}

	//Vector3 CalculateCenter(List<Vector3> points) {
	//	if (points == null || points.Count == 0) {
	//		Debug.LogWarning("No points provided to calculate the center.");
	//		return Vector3.zero;
	//	}

	//	Vector3 sum = Vector3.zero;
	//	foreach (Vector3 point in points) {
	//		sum += point;
	//	}

	//	return sum / points.Count;
	//}

	//public Vector3 CalculatePointInSpace(SignalRNotificationMessage notification) {
	//	Vector2 positionInImage = ScaleBack(new Vector2(
	//		(notification.xmin + notification.xmax) / 2.0f,
	//		(notification.ymin + notification.ymax) / 2.0f));

	//	return GetPositionInSpace(notification, positionInImage);
	//	//Vector3 positionInSpace = GetPositionInSpace(notification, positionInImage);
	//	//return CastOnSpatialMap(positionInSpace, notification);
	//}

	//private Vector2 ScaleBack(Vector2 detectedPosition) {
	//	int cameraResolutionX = (int)Parameters.ActualCameraSize.x;
	//	int cameraResolutionY = (int)Parameters.ActualCameraSize.y;

	//	// Convert detected position to normalized viewport space
	//	float viewportX = detectedPosition.x / cameraResolutionX;
	//	float viewportY = 1-(detectedPosition.y / cameraResolutionY); // Flip Y

	//	return new Vector2(viewportX, viewportY);
	//}

	//public Vector3 GetPositionInSpace(SignalRNotificationMessage notification, Vector2 positionInImage) {
	//	Vector3 castOrigin = new Vector3(notification.posx, notification.posy, notification.posz);
	//	Vector3 forwardPosition = new Vector3(notification.forwardx, notification.forwardy, notification.forwardz);
	//	Vector3 castDirection = (forwardPosition - castOrigin).normalized;
	//	Vector3 p = Camera.main.ViewportToWorldPoint(new Vector3(positionInImage.x, positionInImage.y, Camera.main.nearClipPlane));
 //       int layerMask = 1 << LayerMask.NameToLayer("Spatial Mesh");

 //       lineRenderer.startWidth = 0.1f;
 //       lineRenderer.endWidth = 0.1f;
 //       lineRenderer.positionCount = 2;
 //       lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
 //       lineRenderer.startColor = Color.red;
 //       lineRenderer.endColor = Color.red;

 //       lineRenderer2.startWidth = 0.1f;
 //       lineRenderer2.endWidth = 0.1f;
 //       lineRenderer2.positionCount = 2;
 //       lineRenderer2.material = new Material(Shader.Find("Sprites/Default")); // Basic material
 //       lineRenderer2.startColor = Color.blue;
 //       lineRenderer2.endColor = Color.blue;

 //       Debug.DrawRay(p, castDirection, Color.red, 500.0f);
 //       lineRenderer.SetPosition(0, castOrigin);
 //       lineRenderer2.SetPosition(0, castOrigin);
 //       lineRenderer.SetPosition(1, p + castDirection);
 //       if (Physics.Raycast(castOrigin, p+castDirection, out RaycastHit hitInfo, 10.0f, layerMask)) {
	//		Debug.Log($"[HIT] Raycast hit at {hitInfo.point}");
 //           lineRenderer2.SetPosition(1, hitInfo.point);

 //           Instantiate(hazardDetectedPrefab, hitInfo.point, Quaternion.identity);
	//		return hitInfo.point;
	//	} else {
	//		Debug.Log("Hit Nothing");
	//		return Vector3.zero;
	//	}

	//	//return castOrigin + Parameters.HeightOffset * Vector3.up + castDirection +
	//	//	   Vector3.right * (positionInImage.x * Parameters.VirtualProjectionPlane.x) -
	//	//	   Vector3.up * (positionInImage.y * Parameters.VirtualProjectionPlane.y);
	//}

	//private Vector3 CastOnSpatialMap(Vector3 positionInSpace, SignalRNotificationMessage notification) {
	//	Vector3 castOrigin = new Vector3(notification.posx, notification.posy, notification.posz);
	//	Vector3 direction = (positionInSpace - castOrigin).normalized; // Normalize to ensure consistent scaling

	//	// Ensure direction is not pointing backward
	//	if (Vector3.Dot(direction, (positionInSpace - castOrigin)) < 0) {
	//		direction = -direction;
	//	}

	//	// Debugging: Visualize ray direction
	//	Debug.DrawRay(castOrigin + Parameters.SphereCastOffset * Vector3.up, direction * 5.0f, Color.red, 5.0f);

	//	// SphereCast to check if it hits a spatial mesh
	//	if (Physics.SphereCast(castOrigin + Parameters.SphereCastOffset * Vector3.up,
	//						   Parameters.SphereCastRadius, direction, out RaycastHit hitInfo, 10.0f, LayerMask.GetMask("Spatial Mesh"))) {
	//		Debug.Log($"[HIT] Raycast hit at {hitInfo.point}");
	//		return hitInfo.point;
	//	}

	//	// If no hit, apply estimated depth dynamically
	//	float estimatedDepth = 1.5f + ((notification.xmax - notification.xmin) / Parameters.ActualCameraSize.x) * 3.0f;
	//	Vector3 fallbackPosition = castOrigin + direction * estimatedDepth;
	//	Debug.Log($"[FALLBACK] No hit, estimating position at {fallbackPosition}");

	//	return fallbackPosition;
	//}
}
