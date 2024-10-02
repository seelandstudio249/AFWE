using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HomePage : ManagerBaseScript {
    #region Home Page
    [Header("Home Page Settings")]
    [SerializeField] GameObject homePanel;
    [SerializeField] TMP_Text userName;
    [SerializeField] MRButtonClass taskButton, pinButton;
    #endregion

    #region Tasks Page
    [Header("Task Page Settings")]
    [SerializeField] GameObject taskPanel;
    [SerializeField] Transform tasksHolder;
    [SerializeField] GameObject taskItemButtonPrefab;
    [SerializeField] MRButtonClass returnFromTaskButton;
    #endregion

    #region Specific Task Page
    [Header("Specific Task Page Settings")]
    [SerializeField] GameObject specificTaskPanel;
    [SerializeField] MRButtonClass returnFromSpecificTaskButton;
    [SerializeField] MRButtonClass confirmButton;
    #endregion

    protected override void Awake() {
        base.Awake();

        Follow followScript = GetComponent<Follow>();

        taskButton.button.OnClicked.AddListener(delegate {
            PanelActivation(taskPanel);
        });

        AssignGameMode += delegate {
            userName.text = "Username: " + Login.instance.playerType.ToString();
        };

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

        returnFromTaskButton.button.OnClicked.AddListener(delegate {
            PanelActivation(homePanel);
        });

        confirmButton.button.OnClicked.AddListener(delegate {
            // Send Signal To Server/Backend
            PanelActivation(taskPanel);
        });

        returnFromSpecificTaskButton.button.OnClicked.AddListener(delegate {
            PanelActivation(taskPanel);
        });
    }

    void PanelActivation(GameObject TargetPanel) {
        homePanel.SetActive(false);
        taskPanel.SetActive(false);
        TargetPanel.SetActive(true);
    }
}
