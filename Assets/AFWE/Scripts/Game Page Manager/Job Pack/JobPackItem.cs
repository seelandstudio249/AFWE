using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class JobPackItemButtonClass : MRButtonClass {
	public TMP_Text jobpack_noText;
	public TMP_Text descriptionText;
	public bool job_status;
	public TMP_Text start_dateText;
	public TMP_Text end_dateText;
	public GameObject completedButton;
}

public class JobPackItem : MonoBehaviour
{
	public JobPackItemButtonClass buttonClass;
}
