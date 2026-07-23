using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ETaskItemButtonClass : MRButtonClass {
	public TMP_Text task_no;
	public TMP_Text etask_description;
	public TMP_Text start_date;
	public TMP_Text end_date;
	public RawImage buttonBGImage;
	public GameObject completedButton;
}

public class ETaskPanelItems : MonoBehaviour
{
	public ETaskItemButtonClass buttonClass;
}
