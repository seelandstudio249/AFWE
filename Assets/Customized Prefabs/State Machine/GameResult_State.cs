using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResult_State : GameStateMachineBase
{
    private static GameResult_State instance;
    public static GameResult_State Instance
    {
        get { return instance; }
    }
    public override void Awake() {
        base.Awake();
        instance = this;
    }

    public override void OnStateInit(GameStateMachineManager gameStateMachineManager)
    {
        GameStateMachineManager.Instance.currentGameState = State.Result;
        onInit_Event();
        OnStateStart(gameStateMachineManager);
    }

    public override void OnStateStart(GameStateMachineManager gameStateMachineManager)
    {
        onStart_Event();
    }
    public override void OnStateEnd(GameStateMachineManager gameStateMachineManager)
    {
        onEnd_Event();
        gameStateMachineManager.OnChangeState(gameStateMachineManager.gameEnd_State);
    }
}
