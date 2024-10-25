using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.WebCam;

[Serializable]
public class NotificationPanel {
	public GameObject panel;
	public TMP_Text title;
}

public class HazardDetectionManager : ManagerBaseScript
{
	PhotoCapture photoCaptureObject = null;
	Texture2D targetTexture = null;

	#region Notifications Settings
	[Header("Notification Settings")]
	[SerializeField] List<NotificationPanel> notificationsList;
	[SerializeField] List<string> sampleNotification = new List<string>();
	#endregion

	#region Actions
	Action ReceivedNotification;
	#endregion

	protected override void Awake() {
		base.Awake();
		ReceivedNotification += delegate {
			TestingReceiveData();
			PromptNotification();
		};
	}

	// Start is called before the first frame update
	void Start()
    {
		//Uncomment this to run the function
		//InvokeRepeating("TakePhoto", 1f, 1f);
		InvokeRepeating("TestingReceivingData", 1f, 1f);
	}

	void TakePhoto() {
		Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
		targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
		PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
			photoCaptureObject = captureObject;
			CameraParameters cameraParameters = new CameraParameters();
			cameraParameters.hologramOpacity = 0.0f;
			cameraParameters.cameraResolutionWidth = cameraResolution.width;
			cameraParameters.cameraResolutionHeight = cameraResolution.height;
			cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
			photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
				photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
			});
		});
	}

	Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight) {
		Texture2D newTexture = new Texture2D(newWidth, newHeight, source.format, false);

		for (int y = 0; y < newHeight; y++) {
			for (int x = 0; x < newWidth; x++) {
				// Calculate the original position
				float u = x / (float)newWidth;
				float v = y / (float)newHeight;
				newTexture.SetPixel(x, y, source.GetPixelBilinear(u, v));
			}
		}

		newTexture.Apply();
		return newTexture;
	}

	void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
		//Texture2D resizedTexture = ResizeTexture(targetTexture, newWidth, newHeight);
		byte[] imageBytes = resizedTexture.EncodeToJPG();
		string base64Image = Convert.ToBase64String(imageBytes);
		Debug.Log("Base64 Encoded Image: " + base64Image);

		// Send API
	}
	
	void TestingReceivingData() {
		ReceivedNotification.Invoke();
	}

	void TestingReceiveData() {
		sampleNotification.Add((sampleNotification.Count + 1).ToString());
	}
	void PromptNotification() {
		int amount = 0;
		if(sampleNotification.Count > notificationsList.Count) {
			amount = notificationsList.Count;
		} else {
			amount = sampleNotification.Count;
		}

		foreach(NotificationPanel notification in notificationsList) {
			notification.panel.SetActive(false);
		}
			
		for(int i=0; i < amount; i++) {
			notificationsList[i].panel.SetActive(true);
			notificationsList[i].title.text = sampleNotification[sampleNotification.Count - 1 - i];
		}
	}

	protected override void AfterLoginFunction() {
		base.AfterLoginFunction();
		ManagerActivation(true);
	}
}
