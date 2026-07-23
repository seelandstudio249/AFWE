using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomePage : MonoBehaviour {
	#region Home Page
	[Header("Home Page Settings")]
	public GameObject homePanel;
	[SerializeField] TMP_Text userName, userID, userRole, companyName, taskCount;
	[SerializeField] MRButtonClass taskButton;
	[SerializeField] RawImage userProfilePic;
	#endregion

	PageManager pageManager;

	public Action ActivateHomePage;
	public Action UpdateTaskCount;

	void Awake() {
		pageManager = GetComponent<PageManager>();

		pageManager.managerControlScript.loginScript.showHomePageUserDetails += delegate {
			userName.text = pageManager.managerControlScript.loginScript.playerType.ToString();
			//pageManager.ManagerActivation(true);
		};

		taskButton.button.OnClicked.AddListener(delegate {
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

		ActivateHomePage += delegate {
			userName.text = pageManager.managerControlScript.loginScript.userAccountDetails.first_name + " " + pageManager.managerControlScript.loginScript.userAccountDetails.last_name;
			userID.text = pageManager.managerControlScript.loginScript.userAccountDetails.user_id.ToString();
			userRole.text = pageManager.managerControlScript.loginScript.userAccountDetails.role;
			companyName.text = pageManager.managerControlScript.loginScript.userAccountDetails.opu;
			taskCount.text = pageManager.managerControlScript.loginScript.userAccountDetails.currentTaskCount.ToString();
			if (pageManager.managerControlScript.loginScript.userAccountDetails.userProfilePic != null && pageManager.managerControlScript.loginScript.userAccountDetails.userProfilePic != "") {
				byte[] imageBytes = Convert.FromBase64String(pageManager.managerControlScript.loginScript.userAccountDetails.userProfilePic);
				Texture2D texture = new Texture2D(2, 2);
				if (texture.LoadImage(imageBytes)) {
					//Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width/2, texture.height/2));
					//userProfilePic.sprite = sprite;
					userProfilePic.texture = texture;
				} else {
					Debug.LogError("Failed to load image from base64 string.");
				}
			}
		};
		UpdateTaskCount += delegate {
			taskCount.text = pageManager.managerControlScript.loginScript.userAccountDetails.currentTaskCount.ToString();
		};
	}

	public void UIInteractable(bool interactableStatus) {
		taskButton.button.enabled = interactableStatus;
	}
}
