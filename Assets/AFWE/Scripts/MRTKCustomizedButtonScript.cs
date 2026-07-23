using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TaskButtonClass : MRButtonClass {
    public GameObject sprite;
}

public class MRTKCustomizedButtonScript : MonoBehaviour
{
    public TaskButtonClass buttonClass;

    public void buttonClassIconActivation() {
        buttonClass.sprite.SetActive(!buttonClass.sprite.active);
    }
}
