using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorPanel : MonoBehaviour
{
    public GameObject errorPanel;
	public TMP_Text errorText;
    [SerializeField] MRButtonClass confirmButton;
    [SerializeField] PageManager pageManager;

	private void Awake() {
		confirmButton.button.OnClicked.AddListener(delegate {
			errorPanel.SetActive(false);
			pageManager.ErrorTriggered(true);
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
		});
	}

	public void SetErrorText(string errorMessage) {
		pageManager.ErrorTriggered(false);
        errorText.text = errorMessage;
    }
}
