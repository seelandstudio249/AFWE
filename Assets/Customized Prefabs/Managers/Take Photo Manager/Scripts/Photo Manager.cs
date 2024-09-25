using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;
using MixedReality.Toolkit;
using System;

public class PhotoManager : ManagerBaseScript {
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;

    [SerializeField] MRButtonClass activeCameraButton;
    [SerializeField] MRButtonClass takePhotoButton;

    [SerializeField] GameObject photoFrameIndicatorPrefab;
    [SerializeField] GameObject quad;
    [SerializeField] Shader photoShader;
    private GameObject photoFrame;

    void Start() {
        activeCameraButton.button.OnClicked.AddListener(delegate {
            takePhotoButton.button.gameObject.SetActive(true);
            quad.SetActive(false);
            ActiveCamera();
        });

        takePhotoButton.button.OnClicked.AddListener(delegate {
            TakePhoto();
        });
    }

    void ActiveCamera() {
        if (!photoFrame) {
            photoFrame = Instantiate(photoFrameIndicatorPrefab, Camera.main.transform);
        } else {
            photoFrame.SetActive(true);
        }
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
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        quad.SetActive(true);
        Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        quad.transform.parent = this.transform;
        quad.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);
        quadRenderer.material.SetTexture("_MainTex", targetTexture);

        byte[] imageBytes = targetTexture.EncodeToJPG();
        string base64Image = Convert.ToBase64String(imageBytes);
        Debug.Log("Base64 Encoded Image: " + base64Image);

        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result) {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        photoFrame.SetActive(false);
        takePhotoButton.button.gameObject.SetActive(false);
    }
}
