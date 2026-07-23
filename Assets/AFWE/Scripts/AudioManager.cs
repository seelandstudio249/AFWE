using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public static AudioManager instance;

	public AudioSource audioSource;

	public AudioClip buttonClickedSfx;
	public AudioClip hazardDetectedSfx;
	public AudioClip ActiveValidationSuccessSfx;
	public AudioClip ActiveValidationFailSfx;

	private void Awake() {
		instance = this;
	}
}
