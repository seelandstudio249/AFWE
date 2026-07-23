using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditableObject {
	public ObjectManipulator objectManipulatior;
	public BoxCollider boxCollider;
	public GameObject[] obj;
}

[Serializable]
public class EditorModeItemsBelonging {
	public MRButtonClass button;
	public GameObject[] items;
}

public class EditManager : MonoBehaviour {
	[SerializeField] EditableObject[] moveableObjects;
	[SerializeField] GameObject[] thingsToActivateDeactivateInEditMode;
	[SerializeField] GameObject[] thingsToActivateActivateInEditMode;

	[SerializeField] EditorModeItemsBelonging pumpShutdown, electricianIsolation, lubeOilChange, physicalIsolation;

	private void Awake() {
		pumpShutdown.button.button.OnClicked.AddListener(delegate {
			MoveAllItemToUser(pumpShutdown);
		});

		electricianIsolation.button.button.OnClicked.AddListener(delegate {
			MoveAllItemToUser(electricianIsolation);
		});

		lubeOilChange.button.button.OnClicked.AddListener(delegate {
			MoveAllItemToUser(lubeOilChange);
		});

		physicalIsolation.button.button.OnClicked.AddListener(delegate {
			MoveAllItemToUser(physicalIsolation);
		});
	}

	public void EnterEditMode() {
		foreach (GameObject item in thingsToActivateDeactivateInEditMode) {
			item.SetActive(false);
		}

		foreach (GameObject item in thingsToActivateActivateInEditMode) {
			item.SetActive(true);
		}

		foreach (EditableObject item in moveableObjects) {
			item.objectManipulatior.enabled = true;
			item.boxCollider.enabled = true;
			foreach (GameObject obj in item.obj) {
				obj.SetActive(true);
			}
		}
	}

	public void ExitEditMode() {
		foreach (GameObject item in thingsToActivateDeactivateInEditMode) {
			item.SetActive(true);
		}

		foreach (GameObject item in thingsToActivateActivateInEditMode) {
			item.SetActive(false);
		}

		foreach (EditableObject item in moveableObjects) {
			item.objectManipulatior.enabled = false;
			item.boxCollider.enabled = false;
			foreach (GameObject obj in item.obj) {
				obj.SetActive(false);
			}
		}
	}

	void MoveAllItemToUser(EditorModeItemsBelonging itemsBelongins) {
		Vector3 cameraPosition = Camera.main.transform.position;
		Vector3 cameraForward = Camera.main.transform.forward;
		Vector3 spawnPosition = cameraPosition + cameraForward * .5f;
		foreach(GameObject item in itemsBelongins.items) {
			item.transform.position = spawnPosition;
		}
	}
}
