using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditableObject {
    public ObjectManipulator objectManipulatior;
    public GameObject obj;
}

public class EditManager : MonoBehaviour
{
    [SerializeField] EditableObject[] moveableObjects;
    public void EnterEditMode() {
        foreach(EditableObject item in moveableObjects) {
            item.objectManipulatior.enabled = true;
            item.obj.SetActive(true);
        }
	}

    public void ExitEditMode() {
		foreach (EditableObject item in moveableObjects) {
			item.objectManipulatior.enabled = false;
			item.obj.SetActive(false);
		}
	}
}
