using DG.Tweening.Core.Easing;
using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LeftRightSettings {
	public SetDetail moreOptionDetails;
	public SetDetail panelDetails;
	public SidePanel[] sidePanelsDetails;
}

[Serializable]
public class SetDetail {
	public RectTransform panelObject;
	public Vector3 panelLeftHandPosition, panelRightHandPosition;
}

[Serializable]
public class SidePanel {
	public Transform panelObject;
	public Vector3 panelLeftHandPosition, panelRightHandPosition;
}

[Serializable]
public class HandMenuSideButton : MRButtonClass {
	public Image buttonImage;
	public Sprite activeSprite;
	public Sprite hoverSprite;
	public Sprite unactiveSprite;
}

[Serializable]
public class PinButton : MRButtonClass {
	public SpriteRenderer buttonImage;
	public Sprite activeSprite;
	public Sprite unactiveSprite;
}

public class HandMenuPanelsManager : MonoBehaviour {
	PalmUpChecker palmStatus;

	public bool stillCanShowMoreOptionDetails;

	[SerializeField] HandMenuSideButton moreOptionsButton, homePageButton, taskPageButton, settingButton;
	[SerializeField] GameObject settingPanel;

	[SerializeField] LeftRightSettings _SetDetails;
	[SerializeField] Transform _PanelsParent, _UnpinnedPanelParent, _PanelsHolder;
	[SerializeField] Follow _UnpinnedPanelHolder;
	[SerializeField] PinButton[] pinButton;

	[SerializeField] bool isPinned = false;
	[SerializeField] BoxCollider boxCollider;
	[SerializeField] ObjectManipulator objManipulator;
	[SerializeField] PageManager pageManager;

	[Serializable]
	enum CurrentSelectedPage {
		Null,
		Home,
		Task,
		Setting
	}

	[SerializeField] CurrentSelectedPage currentSelectedPage;

	private void Awake() {
		foreach (PinButton button in pinButton) {
			button.button.OnClicked.AddListener(delegate {
				AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
				PinningPanel();
			});
		}
		palmStatus = GetComponent<PalmUpChecker>();
		moreOptionsButton.button.OnClicked.AddListener(delegate {
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			isPinned = true;
			PinningPanel();
			moreOptionsButton.button.transform.parent.gameObject.SetActive(false);
			if (stillCanShowMoreOptionDetails) {
				currentSelectedPage = CurrentSelectedPage.Null;
				_SetDetails.moreOptionDetails.panelObject.gameObject.SetActive(true);
				taskPageButton.buttonImage.sprite = taskPageButton.unactiveSprite;
				homePageButton.buttonImage.sprite = homePageButton.unactiveSprite;
				settingButton.buttonImage.sprite = settingButton.unactiveSprite;
				settingPanel.SetActive(false);
				pageManager.homePagePanel.homePanel.SetActive(false);
				pageManager.jobPackPanel.jobPackPanel.SetActive(false);
				pageManager.jobTaskPanel.jobTaskPanel.SetActive(false);
				pageManager.jobTaskDocument.jobTaskDocumentPanel.SetActive(false);
				pageManager.eTaskPanel.eTaskPanel.SetActive(false);
				pageManager.specificETaskPanel.specificETaskPanel.SetActive(false);
				homePageButton.button.enabled = true;
				taskPageButton.button.enabled = true;
				settingButton.button.enabled = true;
				_SetDetails.panelDetails.panelObject.gameObject.SetActive(false);
			} else {
				_SetDetails.panelDetails.panelObject.gameObject.SetActive(true);
				_SetDetails.moreOptionDetails.panelObject.gameObject.SetActive(false);
			}
		});

		homePageButton.button.OnClicked.AddListener(delegate {
			settingPanel.SetActive(false);
			_SetDetails.panelDetails.panelObject.gameObject.SetActive(true);
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			pageManager.homePagePanel.ActivateHomePage.Invoke();
			pageManager.homePagePanel.UpdateTaskCount.Invoke();
			pageManager.PanelActivation(pageManager.homePagePanel.homePanel);
			taskPageButton.buttonImage.sprite = taskPageButton.unactiveSprite;
			//homePageButton.buttonImage.sprite = homePageButton.activeSprite;
			settingButton.buttonImage.sprite = settingButton.unactiveSprite;
			homePageButton.button.enabled = false;
			settingButton.button.enabled = true;
			taskPageButton.button.enabled = true;
			currentSelectedPage = CurrentSelectedPage.Home;
		});
		homePageButton.button.firstHoverEntered.AddListener(delegate {
			if(currentSelectedPage != CurrentSelectedPage.Home) homePageButton.buttonImage.sprite = homePageButton.hoverSprite;
		});
		
		homePageButton.button.lastHoverExited.AddListener(delegate {
			if (currentSelectedPage != CurrentSelectedPage.Home) homePageButton.buttonImage.sprite = homePageButton.unactiveSprite; else homePageButton.buttonImage.sprite = homePageButton.activeSprite;
		});

		taskPageButton.button.firstHoverEntered.AddListener(delegate {
			if (currentSelectedPage != CurrentSelectedPage.Task) taskPageButton.buttonImage.sprite = taskPageButton.hoverSprite;
		});
		taskPageButton.button.lastHoverExited.AddListener(delegate {
			if (currentSelectedPage != CurrentSelectedPage.Task) taskPageButton.buttonImage.sprite = taskPageButton.unactiveSprite; else taskPageButton.buttonImage.sprite = taskPageButton.activeSprite;
		});
		taskPageButton.button.OnClicked.AddListener(delegate {
			currentSelectedPage = CurrentSelectedPage.Task;
			settingPanel.SetActive(false);
			homePageButton.button.enabled = true;
			taskPageButton.button.enabled = false;
			settingButton.button.enabled = true;
			//taskPageButton.buttonImage.sprite = taskPageButton.activeSprite;
			homePageButton.buttonImage.sprite = homePageButton.unactiveSprite;
			settingButton.buttonImage.sprite = settingButton.unactiveSprite;
			_SetDetails.panelDetails.panelObject.gameObject.SetActive(true);
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
			var queryParamsUserID = new Dictionary<string, string> {
			{
					"assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() }
			};
			if (pageManager.managerControlScript.loginScript.playerType == PlayerType.MT) {

				ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.JobPack],
					queryParamsUserID,
					() => {
						pageManager.PanelActivation(pageManager.jobPackPanel.jobPackPanel);
					},
					pageManager.jobPackPanel.AssignJobPack
					);
			} else if (pageManager.managerControlScript.loginScript.playerType == PlayerType.FO) {
				var queryParams = new Dictionary<string, string> {
					{"assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() },
					{"order_num", "0" }
				};
				pageManager.specificETaskPanel.ETaskOrderNumber = "1";
				pageManager.eTaskPanel.ETaskOrderNumber = "0";
				ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETask],
					queryParams,
					() => {
						pageManager.PanelActivation(pageManager.eTaskPanel.eTaskPanel);
					},
					pageManager.eTaskPanel.AssignETask
					);
			} else if (pageManager.managerControlScript.loginScript.playerType == PlayerType.E) {
				var queryParams = new Dictionary<string, string> {
					{"assignee_id", pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString() },
					{"order_num", "3" }
				};
				pageManager.specificETaskPanel.ETaskOrderNumber = "3";
				pageManager.eTaskPanel.ETaskOrderNumber = "3";

				ApiManager.instance.APICallGETWithParameters(
					APIGetMiddlePath.APIGetMiddlePathList[(int)APIGetMiddlePathEnum.ETask],
					queryParams,
					() => {
						pageManager.PanelActivation(pageManager.eTaskPanel.eTaskPanel);

					},
					pageManager.eTaskPanel.AssignETask
					);
			}
		});

		settingButton.button.OnClicked.AddListener(delegate {
			pageManager.homePagePanel.homePanel.SetActive(false);
			pageManager.jobPackPanel.jobPackPanel.SetActive(false);
			pageManager.jobTaskPanel.jobTaskPanel.SetActive(false);
			pageManager.jobTaskDocument.jobTaskDocumentPanel.SetActive(false);
			pageManager.eTaskPanel.eTaskPanel.SetActive(false);
			pageManager.specificETaskPanel.specificETaskPanel.SetActive(false);
			_SetDetails.panelDetails.panelObject.gameObject.SetActive(true);
			currentSelectedPage = CurrentSelectedPage.Setting;
			homePageButton.button.enabled = true;
			taskPageButton.button.enabled = true;
			settingButton.button.enabled = false;
			taskPageButton.buttonImage.sprite = taskPageButton.unactiveSprite;
			homePageButton.buttonImage.sprite = homePageButton.unactiveSprite;
			//settingButton.buttonImage.sprite = settingButton.activeSprite;
			settingPanel.SetActive(true);
			AudioManager.instance.audioSource.PlayOneShot(AudioManager.instance.buttonClickedSfx);
		});
		settingButton.button.firstHoverEntered.AddListener(delegate {
			if (currentSelectedPage != CurrentSelectedPage.Setting) settingButton.buttonImage.sprite = settingButton.hoverSprite;
		});
		settingButton.button.lastHoverExited.AddListener(delegate {
			if (currentSelectedPage != CurrentSelectedPage.Setting) settingButton.buttonImage.sprite = settingButton.unactiveSprite; else settingButton.buttonImage.sprite = settingButton.activeSprite;
		});
	}

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {
		if (!isPinned) {
			if (palmStatus.leftHandUp) {
				_SetDetails.panelDetails.panelObject.localPosition = _SetDetails.panelDetails.panelLeftHandPosition;
				foreach (SidePanel panel in _SetDetails.sidePanelsDetails) {
					panel.panelObject.localPosition = panel.panelLeftHandPosition;
				}
			} else {
				_SetDetails.panelDetails.panelObject.localPosition = _SetDetails.panelDetails.panelRightHandPosition;
				foreach (SidePanel panel in _SetDetails.sidePanelsDetails) {
					panel.panelObject.localPosition = panel.panelRightHandPosition;
				}
			}
		}

		if (stillCanShowMoreOptionDetails) {
			//_SetDetails.moreOptionDetails.panelObject.gameObject.SetActive(true);
		} else {
			_SetDetails.moreOptionDetails.panelObject.gameObject.SetActive(false);
		}

		switch (currentSelectedPage) {
			case CurrentSelectedPage.Setting:
			//taskPageButton.buttonImage.sprite = taskPageButton.unactiveSprite;
			//homePageButton.buttonImage.sprite = homePageButton.unactiveSprite;
			settingButton.buttonImage.sprite = settingButton.activeSprite;
			break;
			case CurrentSelectedPage.Task:
			taskPageButton.buttonImage.sprite = taskPageButton.activeSprite;
			//homePageButton.buttonImage.sprite = homePageButton.unactiveSprite;
			//settingButton.buttonImage.sprite = settingButton.unactiveSprite;
			break;
			case CurrentSelectedPage.Home:
			//taskPageButton.buttonImage.sprite = taskPageButton.unactiveSprite;
			homePageButton.buttonImage.sprite = homePageButton.activeSprite;
			//settingButton.buttonImage.sprite = settingButton.unactiveSprite;
			break;
			case CurrentSelectedPage.Null:
			//taskPageButton.buttonImage.sprite = taskPageButton.unactiveSprite;
			//homePageButton.buttonImage.sprite = homePageButton.unactiveSprite;
			//settingButton.buttonImage.sprite = settingButton.unactiveSprite;
			break;
		}
	}

	public void PinningPanel() {
		if (isPinned) {
			isPinned = false;
			_UnpinnedPanelHolder.enabled = false;
		} else {
			isPinned = true;
			_UnpinnedPanelHolder.enabled = !isPinned;
		}
		switch (isPinned) {
			case true:
			_PanelsHolder.SetParent(_UnpinnedPanelParent);
			boxCollider.enabled = true;
			objManipulator.enabled = true;
			//_PanelsHolder.parent = _UnpinnedPanelParent;
			foreach (PinButton sprite in pinButton) {
				sprite.buttonImage.sprite = sprite.unactiveSprite;
			}
			break;
			case false:
			_PanelsHolder.SetParent(_PanelsParent);
			boxCollider.enabled = false;
			objManipulator.enabled = false;
			//_PanelsHolder.parent = _PanelsParent;
			_PanelsHolder.localPosition = new Vector3(0, 0, 0);
			_PanelsHolder.localRotation = Quaternion.identity;
			_PanelsHolder.localScale = new Vector3(1, 1, 1);
			foreach (PinButton sprite in pinButton) {
				sprite.buttonImage.sprite = sprite.activeSprite;
			}
			_SetDetails.panelDetails.panelObject.localRotation = Quaternion.identity;
			break;
		}
	}
}
