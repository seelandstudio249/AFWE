using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SpecificETaskItemButtonClass : MRButtonClass {
	public GameObject unactivatedButton;
	public GameObject activatedButton;
	public GameObject completedButton;
	public RawImage buttonBGImage;
}

public class SpecificETaskItem : MonoBehaviour
{
	public SpecificETaskItemButtonClass buttonClass;
}
