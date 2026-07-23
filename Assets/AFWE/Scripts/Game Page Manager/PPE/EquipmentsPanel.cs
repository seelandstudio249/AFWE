using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class EquipmentButtonClass {
	public MRTKCustomizedButtonScript button;
	public string equipmentName;
	public bool equipmentStatus;
}

public class EquipmentsPanel : MonoBehaviour {
	#region Equipment Page
	[Header("Equipment Page Settings")]
	public GameObject equipmentPagePanel;
	[SerializeField] protected MRButtonClass returnFromEquipmentPageButton;
	[SerializeField] protected MRButtonClass confirmEquipmentButton, selectAllButton;
	[SerializeField] protected List<EquipmentButtonClass> buttons;
	[SerializeField] protected Sprite checkedSprite, uncheckedSprite;
	[SerializeField] protected GameObject errorText;
	#endregion

	protected PageManager pageManager;

	protected virtual void Awake() {
		confirmEquipmentButton.button.enabled = false;
		pageManager = GetComponent<PageManager>();

		returnFromEquipmentPageButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			ReturnFromEquipmentButton();
		});

		confirmEquipmentButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			CompleteButtonOnClicked();
		});
		EquipmentButtonOnClicked();

		selectAllButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			SelectAllButtonOnclicked();
		});
	}

	protected virtual void EquipmentButtonOnClicked() {
		foreach (EquipmentButtonClass item in buttons) {
			item.button.buttonClass.button.OnClicked.AddListener(delegate {
				AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
				item.equipmentStatus = !item.equipmentStatus;
				switch (item.equipmentStatus) {
					case true:
					item.button.buttonClass.sprite.GetComponent<Image>().sprite = checkedSprite;
					break;
					case false:
					item.button.buttonClass.sprite.GetComponent<Image>().sprite = uncheckedSprite;
					break;
				}
				CheckingEquipmentStatus();
			});
		}
	}

	protected virtual void CheckingEquipmentStatus() {
		bool status = true;

		foreach (EquipmentButtonClass item in buttons) {
			if (item.equipmentStatus == false) {
				status = false;
				break;
			}
		}
		errorText.SetActive(!status);
		confirmEquipmentButton.button.enabled = status;
	}

	protected virtual void CompleteButtonOnClicked() {
		LogoutCalled();
		//pageManager.PanelActivation(pageManager.callPanel.callPanel);
	}

	protected virtual void SelectAllButtonOnclicked() {

	}

	protected virtual void ReturnFromEquipmentButton() {
		
	}

	public virtual void LogoutCalled() {
		foreach (EquipmentButtonClass item in buttons) {
			item.equipmentStatus = false;
			item.button.buttonClass.sprite.GetComponent<Image>().sprite = uncheckedSprite;
			CheckingEquipmentStatus();
		}
	}

	public virtual void UIInteractable(bool interactableStatus) {
		returnFromEquipmentPageButton.button.enabled = interactableStatus;
		confirmEquipmentButton.button.enabled = interactableStatus;
		foreach (EquipmentButtonClass item in buttons) {
			item.button.enabled = interactableStatus;
		}
	}
}
