using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogin_State : GameStateMachineBase {
    private static GameLogin_State instance;
    public static GameLogin_State Instance {
        get { return instance; }
    }
    public override void Awake() {
        base.Awake();
        instance = this;
    }

    public override void OnStateInit(GameStateMachineManager gameStateMachineManager) {
        GameStateMachineManager.Instance.currentGameState = State.Login;
        onInit_Event();
        OnStateStart(gameStateMachineManager);
    }

    public override void OnStateStart(GameStateMachineManager gameStateMachineManager) {
        onStart_Event();
    }
    public override void OnStateEnd(GameStateMachineManager gameStateMachineManager) {
        onEnd_Event();
    }
}
