using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnumData {
    public string enumName;
    public List<string> itemName = new List<string>();
}

public class EnumGenerator : MonoBehaviour {
    public EnumData items = new EnumData();
}
