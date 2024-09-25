using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit_State : GameStateMachineBase {
    private static GameInit_State instance;
    public static GameInit_State Instance {
        get { return instance; }
    }

    public override void Awake() {
        base.Awake();
        instance = this;
    }

    public override void OnStateInit(GameStateMachineManager gameStateMachineManager) {
        GameStateMachineManager.Instance.currentGameState = State.Init;
        OnStateStart(gameStateMachineManager);
    }

    public override void OnStateStart(GameStateMachineManager gameStateMachineManager) {

    }

    public override void OnStateEnd(GameStateMachineManager gameStateMachineManager) {

    }
}
