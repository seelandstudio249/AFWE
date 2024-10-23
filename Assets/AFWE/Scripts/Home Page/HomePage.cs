using TMPro;
using UnityEngine;

public class HomePage : MonoBehaviour {
    #region Home Page
    [Header("Home Page Settings")]
    public GameObject homePanel;
    [SerializeField] TMP_Text userName;
    [SerializeField] MRButtonClass taskButton;
	#endregion

	PageManager pageManager;
	

    void Awake() {
		pageManager = GetComponent<PageManager>();

		pageManager.managerControlScript.loginScript.showHomePageUserDetails += delegate {
			userName.text = "Username: " + pageManager.managerControlScript.loginScript.playerType.ToString();
			pageManager.ManagerActivation(true);
		};

		taskButton.button.OnClicked.AddListener(delegate {
			pageManager.PanelActivation(pageManager.jobPackPanel.jobPackPanel);
		});
	}
}
