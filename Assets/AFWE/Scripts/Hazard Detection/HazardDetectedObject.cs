using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HazardDetectedObject : MonoBehaviour {
	[SerializeField] TMP_Text nameTag;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		//nameTag.text = transform.position.ToString();
		gameObject.transform.LookAt(Camera.main.transform);
		gameObject.transform.Rotate(Vector3.up, 180f);
	}

	public void SetName(string name) {
		nameTag.text = name;
	}
}
