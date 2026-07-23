using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentsPanelLubeOilChange : EquipmentsPanel {
	protected override void CompleteButtonOnClicked() {
		base.CompleteButtonOnClicked();
		equipmentPagePanel.SetActive(false);
		pageManager.toolsPanelLubeOilChange.equipmentPagePanel.SetActive(true);
		confirmEquipmentButton.button.enabled = false;

		//pageManager.stepByStepGuideElectricianIsolation.ElectricianIsolationPPEStatusUpdate.Invoke();
	}

	protected override void SelectAllButtonOnclicked() {
		base.SelectAllButtonOnclicked();
		//CompleteButtonOnClicked();
		foreach (EquipmentButtonClass item in buttons) {
			item.equipmentStatus = true;
			item.button.buttonClass.sprite.GetComponent<Image>().sprite = checkedSprite;
		}
		confirmEquipmentButton.button.enabled = true;
	}

	protected override void CheckingEquipmentStatus() {
		bool status = true;

		foreach (EquipmentButtonClass item in buttons) {
			if (item.equipmentStatus == false) {
				status = false;
				break;
			}
		}
		errorText.SetActive(!status);
		if (status) equipmentPagePanel.SetActive(false);
		if (status) pageManager.toolsPanelLubeOilChange.equipmentPagePanel.SetActive(true);
		//confirmEquipmentButton.button.gameObject.SetActive(status);
		//pageManager.stepByStepGuideElectricianIsolation.ElectricianIsolationPPEStatusUpdate.Invoke(status);
	}
}
