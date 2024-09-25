using FishNet;
using FishNet.Managing.Scened;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : ManagerBaseScript {
    [Serializable]
    public class SceneDetails {
        public MRButtonClass buttonClass;
        public string nextSceneName;
        public string currentSceneName;
    }

    #region UI
    [Header("UI")]
    [SerializeField] TMP_Text currentSceneText;
    [SerializeField] SceneDetails nextSceneButton;
    #endregion

    protected override void Awake() {
        base.Awake();
        nextSceneButton.buttonClass.button.OnClicked.AddListener(delegate {
            if (gameMode == GamePlayType.Singleplayer) {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneButton.nextSceneName);
            } else {
                ((SceneManagerNetworking)networkingScript).LoadSceneServer(nextSceneButton.nextSceneName);
                ((SceneManagerNetworking)networkingScript).UnloadSceneServer(nextSceneButton.currentSceneName);
            }
        });
    }
}
