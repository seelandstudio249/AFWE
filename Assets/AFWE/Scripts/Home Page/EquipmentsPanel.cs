using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentsPanel : MonoBehaviour
{
	#region Equipment Page
	[Header("Equipment Page Settings")]
	public GameObject equipmentPagePanel;
	[SerializeField] MRButtonClass returnFromEquipmentPageButton;
	[SerializeField] MRButtonClass confirmEquipmentButton;
	#endregion

	PageManager pageManager;

	private void Awake() {
		pageManager = GetComponent<PageManager>();

		returnFromEquipmentPageButton.button.OnClicked.AddListener(delegate {
			pageManager.PanelActivation(pageManager.specificETaskPanel.specificETaskPanel);
		});

		confirmEquipmentButton.button.OnClicked.AddListener(delegate {
			// Need to send API
			pageManager.PanelActivation(pageManager.specificETaskPanel.specificETaskPanel);
		});
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
