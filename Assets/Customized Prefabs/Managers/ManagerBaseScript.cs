using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum GamePlayType {
    Singleplayer, Multiplayer
}

public class ManagerBaseScript : MonoBehaviour {
    protected GamePlayType gameMode = GamePlayType.Singleplayer;

    [SerializeField] protected ManagerNetworkingBaseScript networkingScript;
    [SerializeField] GameObject managerUI;

    public Action<GamePlayType> AssignGameMode;
    public Action AfterLogin;

    protected virtual void Awake() {
        ManagerActivation(false);
        AssignGameMode += AssignGamePlayType;
		AfterLogin += AfterLoginFunction;

	}

    protected virtual void AssignGamePlayType(GamePlayType gameMode) {
        this.gameMode = gameMode;
    }

    public void ManagerActivation(bool status) {
        if (managerUI) managerUI.SetActive(status);
    }

	protected virtual void AfterLoginFunction() {

	}
}
