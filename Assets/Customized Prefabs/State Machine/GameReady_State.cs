using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameReady_State : GameStateMachineBase
{
    private static GameReady_State instance;
    public static GameReady_State Instance
    {
        get { return instance; }
    }
    public override void Awake() {
        base.Awake();
        instance = this;
    }

    public override void OnStateInit(GameStateMachineManager gameStateMachineManager)
    {
        GameStateMachineManager.Instance.currentGameState = State.Ready;
        OnStateStart(gameStateMachineManager);
    }

    public override void OnStateStart(GameStateMachineManager gameStateMachineManager)
    {
        onStart_Event();
    }
    public override void OnStateEnd(GameStateMachineManager gameStateMachineManager)
    {
        onEnd_Event();
        gameStateMachineManager.OnChangeState(gameStateMachineManager.gameStart_State);
    }
}
