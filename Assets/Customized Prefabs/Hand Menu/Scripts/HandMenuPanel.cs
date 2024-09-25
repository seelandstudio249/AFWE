using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMenuPanel : MonoBehaviour {
    [Serializable]
    public class PanelDetails {
        public MRButtonClass mrButton;
        public GameObject panel;
    }

    PalmUpChecker palmStatus;
    HandConstraintPalmUp mrtkPalmUp;
    [SerializeField] PanelDetails[] panels;

    private void Awake() {
        palmStatus = GetComponent<PalmUpChecker>();
        mrtkPalmUp = GetComponent<HandConstraintPalmUp>();
        foreach (PanelDetails panel in panels) {
            panel.mrButton.button.OnClicked.AddListener(delegate {
                DeactivateAllPanels();
                ActivatePanel(panel.panel);
            });
        }
        mrtkPalmUp.OnLastHandLost.AddListener(delegate {
            DeactivateAllPanels();
        });
    }

    void ActivatePanel(GameObject panel) {
        panel.SetActive(true);
        if (palmStatus.leftHandUp) {
            panel.transform.localPosition = new Vector3(0.1f,0,0);
        } else if (palmStatus.rightHandUp) {
            panel.transform.localPosition = new Vector3(-0.1f,0,0);
        }
    }
    
    void DeactivateAllPanels() {
        foreach (PanelDetails panel in panels) {
            panel.panel.SetActive(false);
        }
    }
}
