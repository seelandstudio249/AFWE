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
	public string object_name;
	public float object_position_x;
	public float object_position_y;
	public float object_position_z;
	public float object_rotation_x;
	public float object_rotation_y;
	public float object_rotation_z;
	public float object_rotation_w;
	public float object_size_x;
	public float object_size_y;
	public float object_size_z;
	//public Vector3 position;
	//public Quaternion rotation;
	//public Vector3 size;
	//public int ObjectIndex;

	public ObjectData() {
		//position = Vector3.zero;
		//rotation = Quaternion.identity;
		//size = Vector3.one;
		//ObjectIndex = -1;
		object_name = string.Empty;
		object_position_x = 0;
		object_position_y = 0;
		object_position_z = 0;
		object_rotation_x = 0;
		object_rotation_y = 0;
		object_rotation_z = 0;
		object_rotation_w = 0;
		object_size_x = 0;
		object_size_y = 0;
		object_size_z = 0;
	}

	//public ObjectData(Vector3 pos, Quaternion rot, Vector3 scale, int index) {
	public ObjectData(string name, Vector3 pos, Quaternion rot, Vector3 scale) {
		object_name = name;
		object_position_x = pos.x;
		object_position_y = pos.y;
		object_position_z = pos.z;
		object_rotation_x = rot.eulerAngles.x;
		object_rotation_y = rot.eulerAngles.y;
		object_rotation_z = rot.eulerAngles.z;
		//object_rotation_w = rot.w;
		object_size_x = scale.x;
		object_size_y = scale.y;
		object_size_z = scale.z;
	//public ObjectData(Vector3 pos, Quaternion rot, Vector3 scale) {
		//position = pos;
		//rotation = rot;
		//size = scale;
		//ObjectIndex = index;
	}
}