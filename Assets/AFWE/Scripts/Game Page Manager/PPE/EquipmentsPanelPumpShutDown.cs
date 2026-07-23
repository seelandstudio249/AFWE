using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentsPanelPumpShutDown : EquipmentsPanel
{
	protected override void ReturnFromEquipmentButton() {
		base.ReturnFromEquipmentButton();
		confirmEquipmentButton.button.enabled = false;

		foreach (EquipmentButtonClass item in buttons) {
			item.equipmentStatus = false;
			item.button.buttonClass.sprite.GetComponent<Image>().sprite = uncheckedSprite;
			CheckingEquipmentStatus();
		}
		pageManager.PanelActivation(pageManager.specificETaskPanel.specificETaskPanel);
		var queryParam = new Dictionary<string, string> {
						{ "task_no", pageManager.specificETaskPanel.taskNumber },
						{ "assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
					};
		ApiManager.instance.APICallGETWithParameters(
			APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETaskRequirements],
				queryParam,
				() => {
					//pageManager.PanelActivation(pageManager.equipmentsPanel.equipmentPagePanel);
				},
				pageManager.specificETaskPanel.AssignSpecificETask
				);
	}

	protected override void CheckingEquipmentStatus() {
		bool status = buttons.All(item => item.equipmentStatus == true);
		errorText.SetActive(!status);
		pageManager.stepByStepGuidePumpShutdown.PumpShutdownPPEStatusUpdate.Invoke(status);
	}

	protected override void SelectAllButtonOnclicked() {
		base.SelectAllButtonOnclicked();
		//CompleteButtonOnClicked();
		foreach (EquipmentButtonClass item in buttons) {
			item.equipmentStatus = true;
			item.button.buttonClass.sprite.GetComponent<Image>().sprite = checkedSprite;
		}
		confirmEquipmentButton.button.enabled = true;
		//pageManager.callPanel.callPanelText.text = "Please call the panel man to check if the Backup pump is up and running";
		//pageManager.PanelActivation(pageManager.callPanel.callPanel);
		//pageManager.callPanel.StartCountdown();
	}

	protected override void CompleteButtonOnClicked() {
		base.CompleteButtonOnClicked();
		pageManager.callPanel.callPanelText.text = "Please call the panel man to check if the Backup pump is up and running";
		pageManager.PanelActivation(pageManager.callPanel.callPanel);
		pageManager.callPanel.StartCountdown();
		confirmEquipmentButton.button.enabled = false;

		//pageManager.PanelActivation(null);
		//QRCodesManager.instance.currentQrCodeString = "P3-201C";
		//pageManager.managerControlScript.GetSpecificManagerScript<QRCodesManager>().StartQRTracking();
		//QRCodeScannerIndicator.instance.hintText.text = "Please Scan Pump QR Code To Start";
	}
}
