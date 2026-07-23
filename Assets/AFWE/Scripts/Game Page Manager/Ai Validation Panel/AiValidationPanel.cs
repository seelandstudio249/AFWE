using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AiValidationPanel : MonoBehaviour
{
	[Header("AI Validate Status Panel")]
	public GameObject aiValidateStatusPanel;
	public RawImage aiValidateImage;
	public TMP_Text aiValidateStatusText, aiValidateDetailsText;
	[SerializeField] PageManager pageManager;

	public void UIInteractable(bool interactableStatus) {
		
	}

	public void LogoutCalled() {
		aiValidateStatusPanel.SetActive(false);
		aiValidateImage.texture = null;
		aiValidateStatusText.text = "";
	}
}
