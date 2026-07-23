using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class SpatialDetection : MonoBehaviour {
	[SerializeField] GameObject cubePrefab;
	private static readonly int LayerMask = 31;
	float interactionInterval = 1;
	private string logFilePath;

	//TensorFloat

	//private TensorFloat inputTensor;
	//private TensorFloat outputTensor;
	[SerializeField] Material ShaderForScaling;
	RenderTexture intermediateRenderTexture;
	//TextureTransform textureTransform;
	public RawImage displayUI;
	WebCamTexture webCamTexture;

	private void Awake() {
		logFilePath = Path.Combine(Application.persistentDataPath, "Logs_File.txt");
		Application.logMessageReceived += HandleLog;
		webCamTexture = new WebCamTexture(896, 504, 4);
		webCamTexture.Play();
		intermediateRenderTexture = new RenderTexture(640, 640, 24);
		this.ShaderForScaling.SetFloat("_Aspect",
				(float)896 / 540 * 640 / 640);
	}
	private void Start() {
		StartCoroutine(InteractWithSpatialMapRepeatedly());
	}

	private void Update() {
		
	}

	private IEnumerator InteractWithSpatialMapRepeatedly() {
		while (true) {
			RaycastHit hitInfo;
			if (SphereCastOnSpatialMesh(transform.position, transform.forward, out hitInfo)) {
				//inputTensor?.Dispose();
				cubePrefab.transform.position = hitInfo.transform.position;
				Debug.Log($"Hit Something at: {hitInfo.transform.position}");
				
				//inputTensor = TextureConverter.ToTensor(this.intermediateRenderTexture, this.textureTransform);
			}

			yield return new WaitForSeconds(interactionInterval); // Wait for the specified interval
		}
	}

	private void HandleLog(string logString, string stackTrace, LogType type) {
		string logEntry = $"[{System.DateTime.Now}] {type}:\nMessage: {logString}\n";

		if (!string.IsNullOrEmpty(stackTrace)) {
			logEntry += $"StackTrace:\n{stackTrace}\n";

			// Extract the line number and file from the stack trace if possible
			string[] stackLines = stackTrace.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
			if (stackLines.Length > 0) {
				string firstRelevantLine = stackLines[0].Trim();
				logEntry += $"Possible Cause: {firstRelevantLine}\n";
			}
		}

		logEntry += "\n"; // Separate logs visually
		File.AppendAllText(logFilePath, logEntry);
	}

	void OnDestroy() {
		Application.logMessageReceived -= HandleLog;
	}

	#region Object Detection
	public static Vector3? CalculatePointInSpace(Vector2 yoloItem) {
		Vector2 positionInImage = ScaleBack(new Vector2(yoloItem.x, yoloItem.y));
		//Vector2 positionInImage = ScaleBack(new Vector2(yoloItem.Center.x, yoloItem.Center.y));
		Vector3 positionInSpace = GetPositionInSpace(positionInImage);
		return CastOnSpatialMap(positionInSpace);
	}

	//public static Vector3[] CalculateCornerPoints(YoloItem yoloItem, CameraTransform cameraTransform) {
	//public static Vector3[] CalculateCornerPoints(Vector2 yoloItem) {
	//		Vector3[] cornerPoints = new Vector3[4];
	//	int i = 0;
	//	Vector2 topRight = new Vector2(yoloItem.x, yoloItem.y) + new Vector2(yoloItem.Size.x, 0);
	//	Vector2 bottomLeft = new Vector2(yoloItem.x, yoloItem.y) - new Vector2(yoloItem.Size.x, 0);
	//	foreach (Vector2 cornerPoint in new[] { yoloItem.TopLeft, topRight, yoloItem.BottomRight, bottomLeft }) {
	//		Vector2 scaled = ScaleBack(cornerPoint);
	//		Vector3 posInSpace = GetPositionInSpace(cameraTransform, scaled);
	//		cornerPoints[i++] = posInSpace;
	//	}

	//	return cornerPoints;
	//}

	private static Vector2 ScaleBack(Vector2 detectedPosition) {
		int cameraResolutionX = 896;
		int cameraResolutionY = 504;
		return new Vector2(
			(detectedPosition.x / 640 * cameraResolutionX - (float)cameraResolutionX / 2) / cameraResolutionX,
			(detectedPosition.y / 640 * cameraResolutionY - (float)cameraResolutionY / 2) / cameraResolutionY
		);
	}

	private static Vector3 GetPositionInSpace(Vector2 positionInImage) {
		return Camera.main.transform.position + Camera.main.transform.up * -0.06f + Camera.main.transform.forward +
			   Camera.main.transform.right * (positionInImage.x * 1.3f) -
			   Camera.main.transform.up * (positionInImage.y * 1);
	}

	private static Vector3? CastOnSpatialMap(Vector3 positionInSpace) {
		Vector3 sphereCastOrigin = Camera.main.transform.position + 0.15f * Camera.main.transform.up;
		Vector3 direction = positionInSpace - sphereCastOrigin;

		if (SphereCastOnSpatialMesh(sphereCastOrigin, direction, out RaycastHit hitInfo)) {
			return hitInfo.point;
		}

		return null;
	}

	private static bool SphereCastOnSpatialMesh(Vector3 origin, Vector3 direction, out RaycastHit hitInfo) {
#if UNITY_EDITOR
		hitInfo = new RaycastHit {
			point = direction + origin
		};
		return true;
#else
            return Physics.SphereCast(origin, 0.05f, direction, out hitInfo, 10, LayerMask);
#endif
	}

	public static bool IsObjectInCameraView(Vector3 position) {
		Vector3 viewPos = Camera.main.WorldToViewportPoint(position);
		return viewPos.x is <= 1f and >= 0f && viewPos.y is <= 1 and >= 0 && viewPos.z >= 0;
	}
	#endregion
}
