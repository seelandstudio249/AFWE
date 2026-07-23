using UnityEngine;
using System.Collections;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using MixedReality.Toolkit.UX;
using System.Text;
using System.Collections.Generic;
using System.IO;

#region POST API
[Serializable]
public class UserCredentials {
	public string username;
	public string password;
	//  string jsonData = JsonUtility.ToJson(new LoginData(username, password));
	public UserCredentials(string username, string password) {
		this.username = username;
		this.password = password;
	}
}

[Serializable]
public class UserCredentialsResponse {
	public int login_id;
}

[Serializable]
public class ActiveValidations {
	//public float confidence;
	public string base64_image;
	public string prompt;

	//public ActiveValidations(float confidence, string base64_image) {
	public ActiveValidations(string base64_image) {
		//this.confidence = confidence;
		this.base64_image = base64_image;
	}

	public ActiveValidations(string base64_image, string prompt) {
		//this.confidence = confidence;
		this.base64_image = base64_image;
		this.prompt = prompt;
	}
}

[Serializable]
public class ActiveValidationsResponse {
	//public float probability;
	//public string tagId;
	//public string tagName;
	//public BoundingBox boundingBox;
	public string Validated;
}

[Serializable]
public class CreateHazard {
	public string hazard_type;
	public string description;
	public string control_measure;
	public string annotated_image_url;
	public DateTime date_time;

	public CreateHazard(string hazard_type, string description, string control_measure, string annotated_image_url, DateTime date_time) {
		this.hazard_type = hazard_type;
		this.description = description;
		this.control_measure = control_measure;
		this.annotated_image_url = annotated_image_url;
		this.date_time = date_time;
	}
}

[Serializable]
public class CreateHazardResponse {
	public string message;
	public int hazard_id;
}

[Serializable]
public class ActiveValidationsOpenaiMultiple {
	public string prompt;
	public string[] images;

	public ActiveValidationsOpenaiMultiple(string prompt, string[] images) {
		this.prompt = prompt;
		this.images = images;
	}
}

[Serializable]
public class HazardDataFrame {
	public ActiveHazard data;

	public HazardDataFrame(ActiveHazard data) {
		this.data = data;
	}
}

[Serializable]
public class ActiveHazard {
	public string device_id;
	public float posx;
	public float posy;
	public float posz;
	public float forwardx;
	public float forwardy;
	public float forwardz;
	public float rightx;
	public float righty;
	public float rightz;
	public float upx;
	public float upy;
	public float upz;
	public string image;

	public ActiveHazard(string device_id,
		float posx, float posy, float posz,
		float forwardx, float forwardy, float forwardz,
		float rightx, float righty, float rightz,
		float upx, float upy, float upz,
		string image) {
		this.device_id = device_id;
		this.posx = posx;
		this.posy = posy;
		this.posz = posz;
		this.forwardx = forwardx;
		this.forwardy = forwardy;
		this.forwardz = forwardz;
		this.rightx = rightx;
		this.righty = righty;
		this.rightz = rightz;
		this.upx = upx;
		this.upy = upy;
		this.upz = upz;
		this.image = image;
	}
}
#endregion

#region PUT API
[Serializable]
public class JobPackStatus {
	public string jobpack_no;
	public string new_status;
	//Completed, Not Started, Started.
	public JobPackStatus(string jobpack_no, string new_status) {
		this.jobpack_no = jobpack_no;
		this.new_status = new_status;
	}
}

[Serializable]
public class JobpackDocumentStatus {
	public string jobpack_no;
	public int assignee_id;
	public string document_name;
	public string new_status;

	public JobpackDocumentStatus(string jobpack_no, int assignee_id, string document_name, string new_status) {
		this.jobpack_no = jobpack_no;
		this.assignee_id = assignee_id;
		this.new_status = new_status;
		this.document_name = document_name;
	}
}

[Serializable]
public class ETaskStatus {
	public string task_no;
	public string new_status;
	public string order_num;

	public ETaskStatus(string task_no, string new_status, string order_num) {
		this.task_no = task_no;
		this.new_status = new_status;
		this.order_num = order_num;
	}
}
#endregion

#region GET API
[Serializable]
public class UserIDResponse {
	public int user_id;
}

[Serializable]
public class UserAttributesResponse {
	public int user_id;
	public string username;
	public string first_name;
	public string last_name;
	public string role;
	public string opu;
	public string container_name;
	public string blob_name;
	public string blob_url;
	public int etask_count;
	public string base64_img;
}

[Serializable]
public class JobPacksResponse {
	public string jobpack_no;
	public string description;
	public int job_order;
	public int assignee_id;
	public string job_status;
	public string start_date;
	public string end_date;
}

[Serializable]
public class JobpackDocumentsResponse {
	public string document_name;
	public string jobpack_no;
	public int assignee_id;
	public string document_filename;
	public int document_order;
	public string document_status;
	public string container_name;
	public string blob_name;
	public string blob_url;
}

[Serializable]
public class EquipmentIncidentsResponse {
	public int incident_id;
	public string incident_date;
	public string incident_description;
	public string equipment_id;
	public string container_name;
	public string blob_name;
	public string blob_url;
}

[Serializable]
public class ETaskResponse {
	public string task_no;
	public string etask_description;
	public string start_date;
	public string end_date;
	public int order_num;
	public string task_status;
	public int assignee_id;
}

[Serializable]
public class ETaskRequirementsResponse {
	public string task_no;
	public int order_num;
	public string etask_description;
	public string start_date;
	public string end_date;
	public string task_status;
	public int assignee_id;
}

[Serializable]
public class ETaskInstructionsResponse {
	public string task_no;
	public int order_no;
	public int etask_order;
	public string title;
	public string message;
	public string additional_message;
	public string electrician_id;
	public string container_name;
	public string blob_name;
	public string blob_url;
	public int require_validation;
	public string base64_img;
}

[Serializable]
public class EquipmentResponse {
	public string equipment_id;
	public string equipment_name;
	public string equipment_status;
	public string equipment_status_color;
	public string equipment_parameter_color;
	public string plant;
	public string unit;
	public string qr_picture;
	public int no_of_incidents;
	public int[] pressure;
	public string pressure_uom;
	public int[] temperature;
	public string temperature_uom;
	public int temperature_high;
	public int temperature_high_high;
	public int[] flowrate;
	public string flowrate_uom;
	public int[] motor_speed;
	public string motor_speed_uom;
	public string container_name;
	public string blob_name;
	public string blob_url;
	public string base64_img;
}

[Serializable]
public class DeviceCredentialsResponse {
	public string device_id;
	public string connection_string;
}

[Serializable]
public class ExtEptwResponse {
	public int eptw_no;
	public string region;
	public string equipment_id;
	public string work_area;
	public string work_desc;
}

[Serializable]
public class JobpackInstruction {
	public string jobpack_no;
	public int jobpack_order;
	public string title;
	public string message;
	public string container_name;
	public string blob_name;
	public string blob_url;
}

[Serializable]
public class ObjectResponse {
	public string RoomName;
	public Objects[] objects;
}

[Serializable]
public class Objects {
	public string name;
	public ObjectPosition position;
	public ObjectRotation rotation;
	public ObjectSize size;
}

[Serializable]
public class ObjectPosition {
	public float x;
	public float y;
	public float z;
}

[Serializable]
public class ObjectRotation {
	public float x;
	public float y;
	public float z;
	public float w;
}

[Serializable]
public class ObjectSize {
	public float x;
	public float y;
	public float z;
}

[Serializable]
public class JobpackDocumentSingleResponse {
	public string document_name;
	public string jobpack_no;
	public int assignee_id;
	public int document_order;
	public string document_status;
	public string container_name;
	public string blob_name;
	public string blob_url;
}
#endregion

#region Signal R
[Serializable]
public class SignalRNotificationResponse {
	public string device_id;
	public SignalRNotificationMessage message;
	public bool isResolved;
}

[Serializable]
public class SignalRNotificationMessage {
	public int hazard_id;
	public string device_id;
	public string hazard_type;
	public string description;
	public string control_measure;
	public string annotated_image_url;
	public string date_time;
	public float posx;
	public float posy;
	public float posz;
	public float forwardx;
	public float forwardy;
	public float forwardz;
	public float rightx;
	public float righty;
	public float rightz;
	public float upx;
	public float upy;
	public float upz;
	public bool isResolved;
	public bool isReportCreated;
	//public float xmin;
	//public float ymin;
	//public float xmax;
	//public float ymax;
	//public List<int> postionIndexesInPhoto;
	public ObjectPositionInPhoto[] objects;
	public List<GameObject> detectedHazardObjects;
}

[Serializable]
public class ObjectPositionInPhoto {
	public int class_index;
	public float xmin;
	public float ymin;
	public float xmax;
	public float ymax;
	public string object_id;
}
#endregion

public class ApiManager : ManagerBaseScript {
	//private string baseAPIUrl = "https://afwe-test-function-app-001.azurewebsites.net/api";
	private string baseAPIUrl = "https://ptsg-5afwe-apim.azure-api.net/ptsg5afwe-func1";

	public static ApiManager instance;

	[SerializeField] PageManager pageManager;

	protected override void Awake() {
		base.Awake();
		instance = this;
	}

	public void Start() { }

	private string BuildUrlWithParams(string baseUrl, Dictionary<string, string> queryParams) {
		StringBuilder url = new StringBuilder(baseUrl);
		if (queryParams != null && queryParams.Count > 0) {
			url.Append("?");
			foreach (var param in queryParams) {
				url.AppendFormat("{0}={1}&", param.Key, UnityWebRequest.EscapeURL(param.Value, Encoding.UTF8).Replace("+", "%20"));
			}
			url.Length--; // Remove the trailing "&"
		}

		return url.ToString();
	}

	#region POST API
	public void APICallPOSTWithParameters(string APIPostMiddlePath, Dictionary<string, string> qParams = null, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		string urlCombine = $"{baseAPIUrl.TrimEnd('/')}/{APIPostMiddlePath.TrimStart('/')}";
		string url = BuildUrlWithParams(urlCombine, qParams);
		StartCoroutine(APICallPOSTWithParam(url, APIPostMiddlePath, callBackNormal, callbackDH));
	}

	private IEnumerator APICallPOSTWithParam(string url, string APIPostMiddlePath, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Post Request From: {APIPostMiddlePath}");
		UnityWebRequest request = new UnityWebRequest(url, "POST");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		// Check for network errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError Post Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPostMiddlePath}");

			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
		} else {
			// Parse the response JSON if the status code is 200
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("POST Request successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"POST Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPostMiddlePath}");

				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}

	public IEnumerator APICallPOST(string APIPostMiddlePath, string jsonData, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Post Request From: {APIPostMiddlePath}");
		UnityWebRequest request = new UnityWebRequest(Path.Combine(baseAPIUrl, APIPostMiddlePath), "POST");
		byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
		request.uploadHandler = new UploadHandlerRaw(jsonToSend);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		// Check for network errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError Post Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPostMiddlePath}");
			Debug.LogError($"This is the data {jsonData}");
			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
		} else {
			// Parse the response JSON if the status code is 200
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("Post successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"POST Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPostMiddlePath}");
				Debug.LogError($"This is the data {jsonData}");
				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}

	public IEnumerator APICallPOSTWithoutLoadingPanel(string APIPostMiddlePath, string jsonData, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		Debug.Log($"Post Request Without Loading Panel From: {APIPostMiddlePath}");

		UnityWebRequest request = new UnityWebRequest(Path.Combine(baseAPIUrl, APIPostMiddlePath), "POST");
		byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
		request.uploadHandler = new UploadHandlerRaw(jsonToSend);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		yield return request.SendWebRequest();

		Debug.Log("Success sending SignalR data to EY API");

		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError Post Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPostMiddlePath}");
			Debug.LogError($"This is the data {jsonData}");
			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
		} else {
			if (request.responseCode >= 200 && request.responseCode < 300) {
				Debug.Log("Code" + request.responseCode);
				Debug.Log("Post successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"POST Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPostMiddlePath}");
				Debug.LogError($"This is the data {jsonData}");
				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}
	#endregion

	#region GET API
	public void APICallGETWithParameters(string APIGetMiddlePath, Dictionary<string, string> qParams = null, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		string urlCombine = $"{baseAPIUrl.TrimEnd('/')}/{APIGetMiddlePath.TrimStart('/')}";
		string url = BuildUrlWithParams(urlCombine, qParams);
		StartCoroutine(APICallGETUrl(url, APIGetMiddlePath, callBackNormal, callbackDH));
	}


	private IEnumerator APICallGETUrl(string url, string APIGetMiddlePath, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Get Request From: {APIGetMiddlePath}");
		UnityWebRequest request = new UnityWebRequest(url, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError GET Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIGetMiddlePath}");

			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
		} else {
			// Parse the response JSON if the status code is 200
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("GET Request successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"GET Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIGetMiddlePath}");

				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}

	public IEnumerator APICallGET(string APIGetMiddlePath, string jsonData, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Get Request From: {APIGetMiddlePath}");
		UnityWebRequest request = new UnityWebRequest(Path.Combine(baseAPIUrl, APIGetMiddlePath), "GET");
		byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
		request.uploadHandler = new UploadHandlerRaw(jsonToSend);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		// Check for network errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError GET Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIGetMiddlePath}");

			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
		} else {
			// Parse the response JSON if the status code is 200
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("GET successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"GET Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIGetMiddlePath}");
				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}
	#endregion

	#region PUT API
	public void APICallPUTWithParameters(string APIGetMiddlePath, Dictionary<string, string> qParams = null, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		string urlCombine = $"{baseAPIUrl.TrimEnd('/')}/{APIGetMiddlePath.TrimStart('/')}";
		string url = BuildUrlWithParams(urlCombine, qParams);
		StartCoroutine(APICallPUT(url, APIGetMiddlePath, callBackNormal, callbackDH));
	}


	private IEnumerator APICallPUT(string url, string APIPutMiddlePath, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Calling Put API From {APIPutMiddlePath}");
		UnityWebRequest request = new UnityWebRequest(url, "PUT");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		// Check for network errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError PUT Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPutMiddlePath}");
			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
		} else {
			// Parse the response JSON if the status code is 200
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("GET Request successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"PUT Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPutMiddlePath}");
				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}

	public IEnumerator APICallPut(string APIPutMiddlePath, string jsonData, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Calling Put API From {APIPutMiddlePath}");
		yield return new WaitForSeconds(1);
		UnityWebRequest request = new UnityWebRequest(Path.Combine(baseAPIUrl, APIPutMiddlePath), "PUT");
		if (!string.IsNullOrEmpty(jsonData)) {
			byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
			request.uploadHandler = new UploadHandlerRaw(jsonToSend);
			request.SetRequestHeader("Content-Type", "application/json");
		}
		request.downloadHandler = new DownloadHandlerBuffer();

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		// Check for network errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError PUT Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPutMiddlePath}");
			Debug.LogError($"This is the data {jsonData}");
			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
			yield return null;
		} else {
			// Parse the response JSON if the status code is 200
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("PUT successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"PUT Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIPutMiddlePath}");
				Debug.LogError($"This is the data {jsonData}");
				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}
	#endregion

	#region DELETE API
	public void APICallDELETEWithParameters(string APIDeleteMiddlePath, Dictionary<string, string> qParams = null, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		string urlCombine = $"{baseAPIUrl.TrimEnd('/')}/{APIDeleteMiddlePath.TrimStart('/')}";
		string url = BuildUrlWithParams(urlCombine, qParams);
		StartCoroutine(APICallGET(url, APIDeleteMiddlePath, callBackNormal, callbackDH));
	}

	private IEnumerator APICallDELETE(string url, string APIDeleteMiddlePath, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Calling Delete API From {APIDeleteMiddlePath}");
		UnityWebRequest request = new UnityWebRequest(url, "DELETE");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		// Check for network errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError DELETE Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIDeleteMiddlePath}");
			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
		} else {
			// Parse the response JSON if the status code is 200
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("DELETE Request successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				Debug.LogError($"DELETE Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIDeleteMiddlePath}");
				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}

	public IEnumerator APICallDelete(string APIDeleteMiddlePath, Action callBackNormal = null, Action<DownloadHandler> callbackDH = null) {
		//Debug.Log($"Calling Delete API From {APIDeleteMiddlePath}");
		yield return new WaitForSeconds(1);
		// Use "DELETE" method with the UnityWebRequest
		UnityWebRequest request = new UnityWebRequest($"{baseAPIUrl.TrimEnd('/')}/{APIDeleteMiddlePath.TrimStart('/')}", "DELETE");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		pageManager.PanelActivation(pageManager.loadingPanel.loadingPanel);
		yield return request.SendWebRequest();
		pageManager.CloseLoadingPanel();

		// Check for network errors
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
			Debug.LogError($"Connection Error or Protocol, \nError DELETE Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIDeleteMiddlePath}");
			pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
			pageManager.errorPanel.SetErrorText(request.error);
			yield return null;
		} else {
			// Parse the response if the status code indicates success (200 or 204)
			if (request.responseCode >= 200 && request.responseCode < 300) {
				//Debug.Log("DELETE successful: " + request.downloadHandler.text);
				if (callBackNormal != null) {
					callBackNormal.Invoke();
				}
				if (callbackDH != null) {
					callbackDH.Invoke(request.downloadHandler);
				}
			} else {
				// Handle other status codes if necessary
				Debug.LogError($"DELETE Request Error - Code: {request.responseCode}\nError: {request.error}\nMiddle Path: {APIDeleteMiddlePath}");
				pageManager.PanelActivation(pageManager.errorPanel.errorPanel);
				pageManager.errorPanel.SetErrorText(request.error);
			}
		}
	}
	#endregion
}

