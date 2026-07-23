using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public enum LinkingType {
	Pumpshutdown,
	PhysicalIsolation
}

public class CallPanel : MonoBehaviour {
	public GameObject callPanel;
	public TMP_Text callPanelText;

	public LinkingType linkingType;

	[SerializeField] TMP_Text countdownText;

	[SerializeField] MRButtonClass confirmButton;

	[SerializeField] PageManager pageManager;

	private Coroutine countdownCoroutine;

	private void Awake() {
		confirmButton.button.OnClicked.AddListener(delegate {
			StopCountdown();
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			pageManager.PanelActivation(null);
			switch (linkingType) {
				case LinkingType.Pumpshutdown:
				pageManager.stepByStepGuidePumpShutdown.startTrackingStartingPoint = true;
				pageManager.stepByStepGuidePumpShutdown.guideStartingPoint.SetActive(true);
				pageManager.stepByStepGuidePumpShutdown.guideStartingPointCollider.enabled = true;
				break;
				case LinkingType.PhysicalIsolation:
				pageManager.stepByStepGuidePhysicalIsolation.startTrackingValvePosition = true;
				pageManager.stepByStepGuidePhysicalIsolation.StartTrackingValve(1);
				break;
			}
		});
	}

	public void StartCountdown() {
		if (countdownCoroutine != null) {
			StopCoroutine(countdownCoroutine);
		}
		countdownCoroutine = StartCoroutine(CountdownCoroutine(5));
	}

	private IEnumerator CountdownCoroutine(int duration) {
		int timer = duration;

		while (timer > 0) {
			countdownText.text = timer.ToString();
			Debug.Log($"Countdown: {timer:F1} seconds");
			yield return new WaitForSeconds(1f);
			timer -= 1;
		}
		countdownText.text = "0";
		Debug.Log("Countdown Complete! Performing action...");
		PerformAction();
	}

	private void PerformAction() {
		Debug.Log("Action Executed!");
		pageManager.PanelActivation(null);
		switch (linkingType) {
			case LinkingType.Pumpshutdown:
			pageManager.stepByStepGuidePumpShutdown.startTrackingStartingPoint = true;
			pageManager.stepByStepGuidePumpShutdown.guideStartingPoint.SetActive(true);
			pageManager.stepByStepGuidePumpShutdown.guideStartingPointCollider.enabled = true;
			break;
			case LinkingType.PhysicalIsolation:
			pageManager.stepByStepGuidePhysicalIsolation.startTrackingValvePosition = true;
			pageManager.stepByStepGuidePhysicalIsolation.StartTrackingValve(1);
			break;
		}
		// Place your action logic here
	}

	public void StopCountdown() {
		if (countdownCoroutine != null) {
			StopCoroutine(countdownCoroutine);
			countdownCoroutine = null;
			Debug.Log("Countdown Stopped!");
		}
	}
}
