using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentsPanelElectricianIsolation : EquipmentsPanel {
	protected override void CompleteButtonOnClicked() {
		base.CompleteButtonOnClicked();
		confirmEquipmentButton.button.enabled = false;
		pageManager.equipmentsPanelElectricianIsolation.equipmentPagePanel.SetActive(false);
		pageManager.stepByStepGuideElectricianIsolation.ElectricianIsolationPPEStatusUpdate.Invoke(true);
	}

	protected override void CheckingEquipmentStatus() {
		bool status = buttons.All(item => item.equipmentStatus == true);
		errorText.SetActive(!status);
		pageManager.stepByStepGuideElectricianIsolation.ElectricianIsolationPPEStatusUpdate.Invoke(status);
	}

	protected override void SelectAllButtonOnclicked() {
		base.SelectAllButtonOnclicked();
		//CompleteButtonOnClicked();
		foreach(EquipmentButtonClass item in buttons) {
			item.equipmentStatus = true;
			item.button.buttonClass.sprite.GetComponent<Image>().sprite = checkedSprite;
		}
		confirmEquipmentButton.button.enabled = true;
		//pageManager.equipmentsPanelElectricianIsolation.equipmentPagePanel.SetActive(false);
		//pageManager.stepByStepGuideElectricianIsolation.ElectricianIsolationPPEStatusUpdate.Invoke(true);
	}

	public void ResetPPE() {
		foreach (EquipmentButtonClass item in buttons) {
			item.equipmentStatus = false;
			//switch (item.equipmentStatus) {
			//	case true:
			//	item.button.buttonClass.sprite.GetComponent<Image>().sprite = checkedSprite;
			//	break;
			//	case false:
			item.button.buttonClass.sprite.GetComponent<Image>().sprite = uncheckedSprite;
			//break;
			//}
		}
		confirmEquipmentButton.button.enabled = false;
	}
}
