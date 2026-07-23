using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class JobTaskItemWPendingButtonClass : MRButtonClass {
	public GameObject sprite;
	public GameObject pendingEIC;
	public GameObject pendingPIC;
	public TMP_Text eicStatusText, picStatusText;
}

public class JobTaskItemWPending : MonoBehaviour
{
	public JobTaskItemWPendingButtonClass buttonClass;
}
