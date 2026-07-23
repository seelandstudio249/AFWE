using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolsPanelLubeOilChange : EquipmentsPanel {
	protected override void CompleteButtonOnClicked() {
		base.CompleteButtonOnClicked();
		equipmentPagePanel.SetActive(false);
		//pageManager.stepByStepGuideElectricianIsolation.ElectricianIsolationPPEStatusUpdate.Invoke();
		//pageManager.stepByStepGuideLubOilChange.stepByStepGuidePanel.SetActive(true);
		//QRCodesManager.instance.StartQRTracking();
		pageManager.stepByStepGuideLubOilChange.startTrackingStartingPoint = true;
		pageManager.stepByStepGuideLubOilChange.guideStartingPoint.SetActive(true);
		pageManager.stepByStepGuideLubOilChange.guideStartingPointCollider.enabled = true;
		confirmEquipmentButton.button.enabled = false;

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
		if (status) pageManager.stepByStepGuideLubOilChange.startTrackingStartingPoint = true;
		if (status) pageManager.stepByStepGuideLubOilChange.guideStartingPoint.SetActive(true);
		if (status) pageManager.stepByStepGuideLubOilChange.guideStartingPointCollider.enabled = true;
		//confirmEquipmentButton.button.gameObject.SetActive(status);
		//pageManager.stepByStepGuideElectricianIsolation.ElectricianIsolationPPEStatusUpdate.Invoke(status);
	}
}
