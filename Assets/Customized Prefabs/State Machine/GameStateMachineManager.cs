using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;
public enum State
{
    Init,
    Login,
    Ready,
    Start,
    Result,
    End,
    Disconnected
}
public class GameStateMachineManager : MonoBehaviour
{
    public State currentGameState;
    private static GameStateMachineManager instance;
    public static GameStateMachineManager Instance
    {
        get { return instance; }
    }
    [SerializeField]
    private GameStateMachineBase currentState;
    public GameInit_State gameInit_State;
    public GameLogin_State gameLogin_State;
    public GameReady_State gameReady_State;
    public GameStart_State gameStart_State;
    public GameResult_State gameResult_State;
    public GameEnd_State gameEnd_State; 
    public GameDisconnected_State gameDisconnected_State;

    private void Awake()
    {
        instance = this;
        CheckAndReasignMissingState();
    }
    private void CheckAndReasignMissingState()
    {
        if (gameInit_State == null)
        {
            gameInit_State = gameObject.GetComponent<GameInit_State>();
        }
        if (gameLogin_State == null)
        {
            gameLogin_State = gameObject.GetComponent<GameLogin_State>();
        }
        if (gameReady_State == null)
        {
            gameReady_State = gameObject.GetComponent<GameReady_State>();
        }
        if (gameStart_State == null)
        {
            gameStart_State = gameObject.GetComponent<GameStart_State>();
        }
        if (gameResult_State == null)
        {
            gameResult_State = gameObject.GetComponent<GameResult_State>();
        }
        if (gameEnd_State == null)
        {
            gameEnd_State = gameObject.GetComponent<GameEnd_State>();
        }
        if(gameDisconnected_State == null) {
            gameDisconnected_State = gameObject.GetComponent<GameDisconnected_State>();
        }
    }
    private void OnEnable()
    {
        
        
    }
    public void OnChangeState(GameStateMachineBase newState)
    {
        if (currentState != null)
        {
            currentState.OnStateEnd(this);
        }
        currentState = newState;
        currentState.OnStateInit(this);
    }

}
