using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomData {
    public string RoomName;
    public List<ObjectData> objects = new List<ObjectData>();

    public RoomData() {
        RoomName = "Default Room";
        objects = new List<ObjectData>();
    }

    public RoomData(string name) {
        RoomName = name;
        objects = new List<ObjectData>();
    }
}

[Serializable]
public class ObjectData {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 size;
    public int ObjectIndex;

    public ObjectData() {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        size = Vector3.one;
        ObjectIndex = -1;
    }

    public ObjectData(Vector3 pos, Quaternion rot, Vector3 scale, int index) {
        position = pos;
        rotation = rot;
        size = scale;
        ObjectIndex = index;
    }
}