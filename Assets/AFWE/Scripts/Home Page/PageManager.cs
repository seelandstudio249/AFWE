using DG.Tweening.Core.Easing;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageManager : ManagerBaseScript {
	public HomePage homePagePanel;
	public JobPackPanel jobPackPanel;
	public ETaskManager eTaskPanel;
	public SpecificETask specificETaskPanel;
	public EquipmentsPanel equipmentsPanel;

	public ManagersControl managerControlScript;
	[SerializeField] MRButtonClass pinButton;

	protected override void Awake() {
		base.Awake();

		AfterLogin += delegate {
			ManagerActivation(true);
			PanelActivation(homePagePanel.homePanel);
			jobPackPanel.AssignDummyJobPack();
		};

		Follow followScript = GetComponent<Follow>();
		pinButton.button.OnClicked.AddListener(delegate {
			followScript.enabled = !followScript.enabled;
			switch (followScript.enabled) {
				case true:
				pinButton.buttonText.text = "Following";
				break;
				case false:
				pinButton.buttonText.text = "Not Following";
				break;
			}
		});
	}

	public void PanelActivation(GameObject TargetPanel) {
		homePagePanel.homePanel.SetActive(false);
		jobPackPanel.jobPackPanel.SetActive(false);
		eTaskPanel.eTaskPanel.SetActive(false);
		specificETaskPanel.specificETaskPanel.SetActive(false);
		equipmentsPanel.equipmentPagePanel.SetActive(false);
		TargetPanel.SetActive(true);
	}
}
