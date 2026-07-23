using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using Assets.Scripts;

public class CameraPanel : MonoBehaviour {
	#region Camera
	[Header("Camera Section")]
	public GameObject cameraPanel;
	[SerializeField] Image cameraLoadingFrame;
	public MRButtonClass takePhotoButton;
	public TMP_Text cameraText, countdownText;
	public string currentPhotoMessageInput;

	PhotoCapture photoCaptureObject = null;
	Texture2D targetTexture = null;
	bool hasSetupCameraResolution = false;
	UnityEngine.Windows.WebCam.CameraParameters cameraParameters;
	bool hasSetupCameraParameters = false;

	public Action<ActiveValidations> PhotoTakenSendAPIPumpShutdown;
	public Action<ActiveValidations> PhotoTakenSendAPIElectricianIsolation;
	public Action<ActiveValidations> PhotoTakenSendAPILubOilChange;
	public Action<ActiveValidations> PhotoTakenSendAPIPhysicalIsolation;

	[SerializeField] PageManager pageManager;
	[SerializeField] float activeValidationWaitingDuration = 3;
	bool isLeftHandOut = false;
	bool isRightHandOut = false;
	HandsAggregatorSubsystem aggregator;
	bool canTakePhotoForActiveValidation;
	public bool canDoHazardDetection = false;
	bool isResetingCamera = false;

	Coroutine CapturePhotoCoroutineVariable;
	#endregion

	#region Probably New Take Photo Function
	[SerializeField] Material ShaderForScaling;
	RenderTexture intermediateRenderTexture;
	WebCamTexture webCamTexture;
	Texture2D tex;
	int rayCount = 30;
	float rayLength = 10f;
	public List<Vector3> meshDetectList = new List<Vector3>();
	private static WebCamTextureAccess WebCamTextureAccess => WebCamTextureAccess.Instance;
	//[SerializeField] RawImage imagePanel;
	#endregion

	private void Awake() {
		//if (!hasSetupCameraResolution) {
		//	Debug.Log("Has Set Up Camera Resolution");
		//	hasSetupCameraResolution = true;
		//	targetTexture = new Texture2D(1080, 720);
		//}

		//if (!hasSetupCameraParameters) {
		//	Debug.Log("Has Set Up Camera Parameters");
		//	cameraParameters = new UnityEngine.Windows.WebCam.CameraParameters {
		//		hologramOpacity = 0.0f,
		//		cameraResolutionWidth = 1080,
		//		cameraResolutionHeight = 720,
		//		pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32
		//	};
		//	hasSetupCameraParameters = true;
		//}

		#region Probably New Take Photo Function
		webCamTexture = new WebCamTexture(WebCamTextureAccess.requestedCameraResolution.x, WebCamTextureAccess.requestedCameraResolution.y, 4);
		webCamTexture.Play();
		intermediateRenderTexture = new RenderTexture(Parameters.ModelImageResolution.x, Parameters.ModelImageResolution.y, 24);
		tex = new Texture2D(intermediateRenderTexture.width, intermediateRenderTexture.height, TextureFormat.RGB24, false);
		//ShaderForScaling.SetFloat("_Aspect",
		//		(float)WebCamTextureAccess.ActualCameraSize.x / WebCamTextureAccess.ActualCameraSize.y * Parameters.ModelImageResolution.y / Parameters.ModelImageResolution.x);

		// Hardcode to 16:9
		//ShaderForScaling.SetFloat("_Aspect", 1.777f);
		ShaderForScaling.SetFloat("_Aspect", 1);
		#endregion

		aggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();
	}

	private void Update() {
		isLeftHandOut = aggregator.TryGetEntireHand(XRNode.LeftHand, out IReadOnlyList<HandJointPose> jointsLeftHand);
		isRightHandOut = aggregator.TryGetEntireHand(XRNode.RightHand, out IReadOnlyList<HandJointPose> jointsRightHand);
	}

	public void UIInteractable(bool interactableStatus) {
		takePhotoButton.button.enabled = interactableStatus;
	}

	public void LogoutCalled() {
		cameraPanel.SetActive(false);
		countdownText.text = "";
	}

	public void AutoTakePhotoForActiveValidation() {
		cameraLoadingFrame.fillAmount = 0;
		countdownText.text = "";
		cameraPanel.SetActive(true);
		canDoHazardDetection = false;
		StartCoroutine(StartCountdown(activeValidationWaitingDuration));
	}

	public void StopAutoTakePhotoForActiveValidation() {
		cameraPanel.SetActive(false);
		canDoHazardDetection = true;
		canTakePhotoForActiveValidation = false;
	}

	IEnumerator StartCountdown(float duration) {
		float elapsedTime = 0f;
		float startValue = 0f;
		float endValue = 100f;
		while (elapsedTime < duration) {
			float currentValue;
			if (!isResetingCamera) {
				if (!isLeftHandOut && !isRightHandOut) {
					currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
					countdownText.text = Mathf.RoundToInt(currentValue).ToString() + "%";
					cameraLoadingFrame.fillAmount = currentValue / 100;
					elapsedTime += Time.deltaTime;
				} else {
					currentValue = 0;
					countdownText.text = "";
					cameraLoadingFrame.fillAmount = 0;
					elapsedTime = 0f;
				}
			} else {
				currentValue = 0;
				cameraLoadingFrame.fillAmount = 0;
				elapsedTime = 0f;
			}

			yield return null;
		}

		try {
			cameraLoadingFrame.fillAmount = 1;
			countdownText.text = Mathf.RoundToInt(100f).ToString() + "%";
			canTakePhotoForActiveValidation = true;
			pageManager.loadingPanel.loadingPanel.SetActive(true);
		} catch (Exception ex) {
			Debug.LogException(ex);
		}
	}

	#region Probably New Take Photo Function
	public IEnumerator InteractWithSpatialMapRepeatedly() {
		Graphics.Blit(webCamTexture, this.intermediateRenderTexture, this.ShaderForScaling);
		if (canDoHazardDetection) {
			RenderTexture currentTexture = intermediateRenderTexture;
			RenderTexture.active = currentTexture;
			tex.ReadPixels(new Rect(0, 0, intermediateRenderTexture.width, intermediateRenderTexture.height), 0, 0);
			//imagePanel.texture = tex;
			tex.Apply();
			RenderTexture.active = null;
			byte[] imageBytes = tex.EncodeToJPG();
			string base64Image = Convert.ToBase64String(imageBytes);
			//Debug.Log(base64Image);
			Vector3 cameraPosition = Camera.main.transform.position;
			Vector3 cameraFacingDiretion = Camera.main.transform.forward;
			Vector3 cameraRightDirection = Camera.main.transform.right;
			Vector3 cameraUpDirection = Camera.main.transform.up;
			//Vector3 targetPosition = cameraPosition + cameraFacingDiretion * 1;
			ActiveHazard activeHazard = new ActiveHazard(
				pageManager.managerControlScript.loginScript.userAccountDetails.deviceID,
				cameraPosition.x, cameraPosition.y, cameraPosition.z,
				cameraFacingDiretion.x, cameraFacingDiretion.y, cameraFacingDiretion.z,
				cameraRightDirection.x, cameraRightDirection.y, cameraRightDirection.z,
				cameraUpDirection.x, cameraUpDirection.y, cameraUpDirection.z,
				base64Image);
			HazardDataFrame hazardDataFrame = new HazardDataFrame(activeHazard);
			string jsonDataHazardDetection = JsonUtility.ToJson(hazardDataFrame);
			ApiManager.instance.StartCoroutine(ApiManager.instance.APICallPOSTWithoutLoadingPanel(
				APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.ActiveHazard],
				jsonDataHazardDetection));
		}
		if (canTakePhotoForActiveValidation) {
			RenderTexture currentTexture = intermediateRenderTexture;
			RenderTexture.active = currentTexture;
			Texture2D snapshotTexture = new Texture2D(currentTexture.width, currentTexture.height, TextureFormat.RGB24, false);
			snapshotTexture.ReadPixels(new Rect(0, 0, currentTexture.width, currentTexture.height), 0, 0);
			snapshotTexture.Apply();
			RenderTexture.active = null;
			byte[] imageBytes = snapshotTexture.EncodeToJPG();
			string base64Image = Convert.ToBase64String(imageBytes);
			cameraPanel.SetActive(false);
			pageManager.aiValidationPanel.aiValidateImage.texture = snapshotTexture;
			if (pageManager.managerControlScript.loginScript.playerType == PlayerType.FO) {
				if (pageManager.specificETaskPanel.taskOrderNumber == "1") {
					PhotoTakenSendAPIPumpShutdown.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
				} else if (pageManager.specificETaskPanel.taskOrderNumber == "2") {
					PhotoTakenSendAPIPhysicalIsolation.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
				}
			} else if (pageManager.managerControlScript.loginScript.playerType == PlayerType.E) {
				PhotoTakenSendAPIElectricianIsolation.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
			} else if (pageManager.managerControlScript.loginScript.playerType == PlayerType.MT) {
				PhotoTakenSendAPILubOilChange.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
			}
			canTakePhotoForActiveValidation = false;
		}
		yield return new WaitForSeconds(1f);
		StartCoroutine(InteractWithSpatialMapRepeatedly());
	}
	#endregion

	#region Take Photo
	//public void StartTakingPhotoEverySecond() {
	//	if (photoCaptureObject == null) {
	//		PhotoCapture.CreateAsync(false, captureObject => {
	//			if (captureObject == null) {
	//				Debug.LogError("PhotoCapture.CreateAsync failed to initialize.");
	//				return;
	//			}
	//			Debug.Log($"Creating Async For Photo Taking on thread: {Thread.CurrentThread.ManagedThreadId}");
	//			photoCaptureObject = captureObject;
	//			photoCaptureObject.StartPhotoModeAsync(cameraParameters, result => {
	//				if (result.success) {
	//					Debug.Log("Async Created Completed");
	//					CapturePhotoCoroutineVariable = StartCoroutine(CapturePhotoCoroutine());
	//					canDoHazardDetection = true;
	//				} else {
	//					Debug.LogError("Failed to start photo mode.");
	//					StopPhotoMode();
	//				}
	//			});
	//		});
	//	}
	//}

	//private IEnumerator CapturePhotoCoroutine() {
	//	TakePhotoAsync();
	//	yield return new WaitForSeconds(1f);
	//	StartCoroutine(CapturePhotoCoroutine());
	//}

	//void TakePhotoAsync() {
	//	if (photoCaptureObject != null && !isResetingCamera) {
	//		Debug.Log("Take photo Async: photoCaptureObject is not null");
	//		photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
	//	}
	//}

	//void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
	//	Debug.Log("Photo taken...");
	//	try {
	//		if (result.success && photoCaptureFrame != null) {
	//			Debug.Log("Photo taken successfully");
	//			if (canDoHazardDetection) {
	//				photoCaptureFrame.UploadImageDataToTexture(targetTexture);
	//				byte[] imageBytes = targetTexture.EncodeToJPG();
	//				string base64Image = Convert.ToBase64String(imageBytes);
	//				Vector3 cameraPosition = Camera.main.transform.position;
	//				Vector3 cameraFacingDiretion = Camera.main.transform.forward;
	//				Vector3 targetPosition = cameraPosition + cameraFacingDiretion * 5;
	//				ActiveHazard activeHazard = new ActiveHazard(
	//					pageManager.managerControlScript.loginScript.userAccountDetails.deviceID,
	//					cameraPosition.x, cameraPosition.y, cameraPosition.z,
	//					targetPosition.x, targetPosition.y, targetPosition.z,
	//					base64Image);
	//				HazardDataFrame hazardDataFrame = new HazardDataFrame(activeHazard);
	//				string jsonDataHazardDetection = JsonUtility.ToJson(hazardDataFrame);
	//				ApiManager.instance.StartCoroutine(ApiManager.instance.APICallPOSTWithoutLoadingPanel(
	//					APIPostMiddlePath.APIPostMiddlePathList[(int)APIPostMiddlePathEnum.ActiveHazard],
	//					jsonDataHazardDetection));
	//			}

	//			if (canTakePhotoForActiveValidation) {
	//				Debug.Log("Active Validation Triggered");
	//				photoCaptureFrame.UploadImageDataToTexture(targetTexture);
	//				byte[] imageBytes = targetTexture.EncodeToJPG();
	//				string base64Image = Convert.ToBase64String(imageBytes);
	//				cameraPanel.SetActive(false);
	//				pageManager.aiValidationPanel.aiValidateImage.texture = targetTexture;

	//				if (pageManager.managerControlScript.loginScript.playerType == PlayerType.FO) {
	//					if (pageManager.specificETaskPanel.taskOrderNumber == "1") {
	//						Debug.Log("Its Pump Shutdown");
	//						PhotoTakenSendAPIPumpShutdown.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
	//					} else if (pageManager.specificETaskPanel.taskOrderNumber == "2") {
	//						Debug.Log("Its Physical Isolation");
	//						PhotoTakenSendAPIPhysicalIsolation.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
	//					}
	//				} else if (pageManager.managerControlScript.loginScript.playerType == PlayerType.E) {
	//					Debug.Log("Its Electrician Isolation");
	//					PhotoTakenSendAPIElectricianIsolation.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
	//				} else if (pageManager.managerControlScript.loginScript.playerType == PlayerType.MT) {
	//					Debug.Log("Its Lube Oil Change");
	//					PhotoTakenSendAPILubOilChange.Invoke(new ActiveValidations(base64Image, currentPhotoMessageInput));
	//				}
	//				canTakePhotoForActiveValidation = false;
	//			}
	//		} else {
	//			Debug.LogError("Failed to capture photo or photo frame is null.");
	//		}
	//	} catch (Exception ex) {
	//		Debug.LogException(ex);
	//	}
	//}

	//void StopPhotoMode() {
	//	Debug.Log("Stopping photo mode");
	//	if (CapturePhotoCoroutineVariable != null) {
	//		Debug.Log("Stopping coroutine");
	//		StopCoroutine(CapturePhotoCoroutineVariable);
	//		CapturePhotoCoroutineVariable = null;
	//		Debug.Log("Coroutine stopped");
	//	}
	//	if (photoCaptureObject != null) {
	//		Debug.Log("Stopping photo capture object");
	//		photoCaptureObject.StopPhotoModeAsync(
	//			OnStoppedPhotoMode
	//		);
	//	} else {
	//		Debug.LogWarning("Photo capture object is null. No need to stop.");
	//	}
	//}

	//void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result) {
	//	photoCaptureObject.Dispose();
	//	photoCaptureObject = null;
	//	Debug.Log("Photo capture stopped and resources released.");
	//}

	//private int restartAttempts = 0;
	//private const int maxRestartAttempts = 3;

	//public void RestartCamera() {
	//	StartCoroutine(RestartCameraCoroutine());
	//}

	//IEnumerator RestartCameraCoroutine() {
	//	countdownText.text = "Restarting Camera";
	//	isResetingCamera = true;
	//	if (restartAttempts >= maxRestartAttempts) {
	//		countdownText.text = "Max restart attempts reached. Camera will not restart.";
	//		Debug.LogError("Max restart attempts reached. Camera will not restart.");
	//		yield break;
	//	}
	//	restartAttempts++;
	//	Debug.Log($"Restarting Camera... Attempt {restartAttempts}/{maxRestartAttempts}");
	//	countdownText.text = $"Restarting Camera... Attempt {restartAttempts}/{maxRestartAttempts}";
	//	if (CapturePhotoCoroutineVariable != null) {
	//		countdownText.text = "Stopping coroutine";
	//		Debug.Log("Stopping coroutine");
	//		StopCoroutine(CapturePhotoCoroutineVariable);
	//		CapturePhotoCoroutineVariable = null;
	//		Debug.Log("Coroutine stopped");
	//	}
	//	if (photoCaptureObject != null) {
	//		Debug.Log($"Checking photo capture object status: {photoCaptureObject != null}");
	//		var isStopped = false;
	//		countdownText.text = "Stopping photo mode";
	//		photoCaptureObject.StopPhotoModeAsync(result => {
	//			Debug.Log("Photo mode stopped.");
	//			isStopped = true;
	//		});
	//		yield return new WaitUntil(() => isStopped);
	//		photoCaptureObject.Dispose();
	//		photoCaptureObject = null;
	//		countdownText.text = "Photo mode stop";
	//	}
	//	yield return new WaitForSeconds(1);
	//	countdownText.text = $" Create a new PhotoCapture object and start photo mode";
	//	PhotoCapture.CreateAsync(false, captureObject => {
	//		if (captureObject == null) {
	//			Debug.LogError("Failed to recreate PhotoCapture object.");
	//			return;
	//		}
	//		photoCaptureObject = captureObject;
	//		photoCaptureObject.StartPhotoModeAsync(cameraParameters, result => {
	//			if (result.success) {
	//				Debug.Log("Photo mode restarted successfully.");
	//				CapturePhotoCoroutineVariable = StartCoroutine(CapturePhotoCoroutine());
	//				isResetingCamera = false;
	//				restartAttempts = 0;
	//			} else {
	//				Debug.LogError("Failed to restart photo mode.");
	//				StartCoroutine(RestartCameraCoroutine());
	//			}
	//		});
	//	});
	//}
	#endregion
}