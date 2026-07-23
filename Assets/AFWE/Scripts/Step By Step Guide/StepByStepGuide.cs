using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Windows.WebCam;

[Serializable]
public class ETaskInstructionsResponseWrapper {
	public List<ETaskInstructionsResponse> eTaskInstructionsResponses = new List<ETaskInstructionsResponse>();
}

[Serializable]
public class JobPackInstructionsResponseWrapper {
	public List<JobpackInstruction> jobPackInstructionsResponses = new List<JobpackInstruction>();
}

[Serializable]
public class EquipmentResponseWrapper {
	public List<EquipmentResponse> equipmentResponse = new List<EquipmentResponse>();
}

[Serializable]
public class ActiveValidationsResponseWrapper {
	public List<ActiveValidationsResponse> activeValidationsResponses = new List<ActiveValidationsResponse>();
}


public class StepByStepGuide : MonoBehaviour {
	#region Data
	[Header("Datas")]
	[SerializeField] protected ETaskInstructionsResponseWrapper eTaskInstructionsResponseWrapper;
	[SerializeField] protected JobPackInstructionsResponseWrapper jobPackInstructionsResponseWrapper;
	[SerializeField] protected EquipmentResponseWrapper equipmentResponseWrapper;
	[SerializeField] protected ActiveValidationsResponse activeValidationResponse;
	#endregion

	#region Starting Point
	public GameObject guideStartingPoint;
	public BoxCollider guideStartingPointCollider;
	public bool startTrackingStartingPoint = false;
	public LayerMask startingPointLayer;

    protected bool isStepInForTheFirstTime = false;
	#endregion

	[Space(10), Header("Warning Panel For User Positioning")]
	[SerializeField] protected GameObject outOfPointWarningPanel;
	[SerializeField] protected TMP_Text outOfPointWarningText;

	[Space(10), Header("Step by Step Panels Parent Object")]
	[SerializeField] protected GameObject stepByStepGuidePanelParentObj;

	[Space(10), Header("Step By Step Guide Text Only Panel")]
	[SerializeField] protected GameObject stepByStepGuideTextPanel;
	[SerializeField] protected TMP_Text stepByStepGuideTextOnly_HeaderTextField, stepByStepGuideTextOnly_TextField;
	[SerializeField] protected MRButtonClass nextStepButtonTextPanel, previousStepButtonTextPanel, completeButtonTextPanel;

	[Space(10), Header("Step By Step Guide Image And Text Panel")]
	[SerializeField] protected GameObject stepByStepGuidePanelWithImage;
	[SerializeField] protected RawImage imageDisplay;
	[SerializeField] protected TMP_Text stepByStepGuideImageText_HeaderTextField, stepByStepGuideImageText_TextField;
	[SerializeField] protected MRButtonClass nextStepButtonImageTextPanel, previousStepButtonImageTextPanel, completeButtonImageTextPanel;

	[Space(10), Header("Step By Step Guide Video And Text Panel")]
	[SerializeField] protected GameObject stepByStepGuidePanelWithVideo;
	[SerializeField] protected VideoPlayer videoPlayer;
	[SerializeField] protected TMP_Text stepByStepGuideVideoText_HeaderTextField, stepByStepGuideVideoText_TextField;
	[SerializeField] protected MRButtonClass nextStepButtonVideoTextPanel, previousStepButtonVideoTextPanel, completeButtonVideoTextPanel;

	[Space(10), Header("Step By Step Guide Scroll View")]
	[SerializeField] protected GameObject stepByStepGuidePanelWithScrollView;
	[SerializeField] protected Transform guideItemHolder;
	[SerializeField] protected GameObject stepByStepGuideTextItemPrefab, StepByStepGuideTextAndVideoItemPrefab;
	[SerializeField] protected TMP_Text stepByStepGuideScrollView_HeaderTextField;
	[SerializeField] protected MRButtonClass completeButtonScrollViewPanel;

	[Space(10), Header("Complete Panel")]
	[SerializeField] protected GameObject completePanel;
	[SerializeField] protected MRButtonClass completePanelButton;
	[SerializeField] protected TMP_Text countdownCompleteText;

	[SerializeField] protected PageManager pageManager;

	protected Coroutine countdownCoroutine;

	void Awake() {

	}

	public virtual void AssignETaskInstruction(DownloadHandler returnedItems) {
		string wrappedJson = "{ \"eTaskInstructionsResponses\": " + returnedItems.text + " }";
		ETaskInstructionsResponseWrapper wrapper = JsonUtility.FromJson<ETaskInstructionsResponseWrapper>(wrappedJson);

		if (wrapper != null && wrapper.eTaskInstructionsResponses != null) {
			wrapper.eTaskInstructionsResponses = wrapper.eTaskInstructionsResponses.OrderBy(item => item.etask_order).ToList();
			eTaskInstructionsResponseWrapper = wrapper;
		}
	}

	public virtual void LogoutCalled() {
		stepByStepGuidePanelParentObj.SetActive(false);

		stepByStepGuideTextPanel.SetActive(false);
		stepByStepGuideTextOnly_HeaderTextField.text = "";
		stepByStepGuideTextOnly_TextField.text = "";

		stepByStepGuidePanelWithImage.SetActive(false);
		stepByStepGuideImageText_HeaderTextField.text = "";
		stepByStepGuideImageText_TextField.text = "";
		imageDisplay.texture = null;

		stepByStepGuidePanelWithVideo.SetActive(false);
		videoPlayer.url = null;
		videoPlayer.Stop();
		stepByStepGuideVideoText_HeaderTextField.text = "";
		stepByStepGuideVideoText_TextField.text = "";

		completePanel.SetActive(false);

		startTrackingStartingPoint = false;
		guideStartingPoint.SetActive(false);
		guideStartingPointCollider.enabled = false;
	}

	public virtual void UIInteractable(bool interactableStatus) {
		nextStepButtonTextPanel.button.enabled = interactableStatus;
		previousStepButtonTextPanel.button.enabled = interactableStatus;
		completeButtonTextPanel.button.enabled = interactableStatus;
		nextStepButtonImageTextPanel.button.enabled = interactableStatus;
		previousStepButtonImageTextPanel.button.enabled = interactableStatus;
		completeButtonImageTextPanel.button.enabled = interactableStatus;
		nextStepButtonVideoTextPanel.button.enabled = interactableStatus;
		previousStepButtonVideoTextPanel.button.enabled = interactableStatus;
		completeButtonVideoTextPanel.button.enabled = interactableStatus;
		completePanelButton.button.enabled = interactableStatus;
	}

	protected virtual void UpdateButtonStates(int currentIndex, int maxIndex) {
		//// Disable the previous button if we're on the first image
		previousStepButtonTextPanel.button.gameObject.SetActive(currentIndex > 0);
		previousStepButtonImageTextPanel.button.gameObject.SetActive(currentIndex > 0);
		previousStepButtonVideoTextPanel.button.gameObject.SetActive(currentIndex > 0);

		//// Disable the next button if we're on the last image
		nextStepButtonTextPanel.button.gameObject.SetActive(currentIndex < maxIndex - 1);
		nextStepButtonImageTextPanel.button.gameObject.SetActive(currentIndex < maxIndex - 1);
		nextStepButtonVideoTextPanel.button.gameObject.SetActive(currentIndex < maxIndex - 1);
		completeButtonTextPanel.button.gameObject.SetActive(currentIndex == maxIndex - 1);
		completeButtonImageTextPanel.button.gameObject.SetActive(currentIndex == maxIndex - 1);
		completeButtonVideoTextPanel.button.gameObject.SetActive(currentIndex == maxIndex - 1);
	}

	protected string ShowProperText(string text) {
		string extractedText = text.Substring(text.IndexOf(":") + 2);

		return extractedText.Replace("\\", "");
	}

	protected IEnumerator StartCountdown(float duration, Action nextAction = null) {
		yield return new WaitForSeconds(duration);
		pageManager.aiValidationPanel.aiValidateStatusPanel.SetActive(false);
		nextAction?.Invoke();
	}

	protected void StartCountdownAfterCompleted() {
		if (countdownCoroutine != null) {
			StopCoroutine(countdownCoroutine);
		}
		countdownCoroutine = StartCoroutine(CountdownCoroutine(5));
	}

	private IEnumerator CountdownCoroutine(int duration) {
		int timer = duration;

		while (timer > 0) {
			if(countdownCompleteText != null) countdownCompleteText.text = timer.ToString();
			Debug.Log($"Countdown: {timer:F1} seconds");
			yield return new WaitForSeconds(1f);
			timer -= 1;
		}
		if (countdownCompleteText != null) countdownCompleteText.text = "0";
		Debug.Log("Countdown Complete! Performing action...");
		PerformActionAfterCompleteTaskCountdown();
	}

	protected virtual void PerformActionAfterCompleteTaskCountdown() {
		Debug.Log("Action Executed!");
	}

	protected void StopCountdown() {
		if (countdownCoroutine != null) {
			StopCoroutine(countdownCoroutine);
			countdownCoroutine = null;
			Debug.Log("Countdown Stopped!");
		}
	}
}