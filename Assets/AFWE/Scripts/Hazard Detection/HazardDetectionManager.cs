using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class HazardDetectionManager : ManagerBaseScript
{
	PhotoCapture photoCaptureObject = null;
	Texture2D targetTexture = null;

	// Start is called before the first frame update
	void Start()
    {
		//Uncomment this to run the function
		//InvokeRepeating("TakePhoto", 1f, 1f);
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

	void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame) {
		byte[] imageBytes = targetTexture.EncodeToJPG();
		string base64Image = Convert.ToBase64String(imageBytes);
		Debug.Log("Base64 Encoded Image: " + base64Image);

		// Send API
	}
}
