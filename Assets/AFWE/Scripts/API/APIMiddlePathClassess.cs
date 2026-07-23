using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public static class APIPostMiddlePath {
	public static string[] APIPostMiddlePathList = {
		"user/credentials",
		"active/validations/openai",
		"objects/create",
		"active/validations/customvision",
		"notification/send",
		"createHazard",
		"active/validations/openai/multiple",
		"active/hazard",
		"active/hazard/uauc/report/create"
	};

}
public enum APIPostMiddlePathEnum {
	UserCredentials,
	ActiveValidationsOpenAi,
	ObjectsCreate,
	ActiveValidationCustomVision,
	NotificationSend,
	CreateHazard,
	ActiveValidationsOpenaiMultiple,
	ActiveHazard,
	ActiveHazardUaucReportCreate
}

[Serializable]
public static class APIPutMiddlePath {
	public static string[] APIPutMiddlePathList = {
		"jobpack/status",
		"jobpack/document/status",
		"etask/status",
		"master/reset",
		"master/criteria/pic"
	};
}
public enum APIPutMiddlePathEnum {
	JobpackStatus,
	JobPackDocumentStatus,
	ETaskStatus,
	MasterReset,
	MasterCriteriaPIC
}

[Serializable]
public static class APIGetMiddlePath {
	public static string[] APIGetMiddlePathList = {
		"user/id",
		"user/attributes",
		"jobpack",
		"jobpack/documents",
		"equipment/incidents",
		"etask",
		"etask/instructions",
		"equipment",
		"device/credentials",
		"objects",
		"ext/eptw",
		"jobpack/instructions",
		"etask/requirements",
		"jobpack/documents/single"
	};
}
public enum APIGetMiddlePathEnum {
	UserID,
	UserAttributes,
	JobPack,
	JobPackDocuments,
	EquipmentIncidents,
	ETask,
	ETaskInstructions,
	Equipment,
	DeviceCredentials,
	Objects,
	ExtEptw,
	JobpackInstructions,
	ETaskRequirements,
	JobpackDocumentSingle
}

[Serializable]
public static class APIDeleteMiddlePath {
	public static string[] APIDeleteMiddlePathList = {
		"objects/clear"
	};
}

public enum APIDeletePathEnum {
	ObjectDelete
}