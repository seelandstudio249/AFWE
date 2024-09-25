using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    #region Conditions
    #region Init Conditions
    public bool initCondition1;
    #endregion

    #region Login Conditions
    public bool loginCondition1;
    #endregion

    #region Ready Conditions
    public bool readyCondition1;
    #endregion

    #region Start Conditions
    public bool startCondition1;
    #endregion

    #region Result Conditions
    public bool resultCondition1;
    #endregion

    #region End Conditions
    public bool endCondition1;
    #endregion
    #endregion

    //private void awake() {
    //GameStateMachineManager.Instance.OnChangeState(GameStateMachineManager.Instance.gameInit_State);
    //}
    // Start is called before the first frame update
    void Start() {
        if (GameStateMachineManager.Instance.gameInit_State) {
            GameStateMachineManager.Instance.OnChangeState(GameStateMachineManager.Instance.gameInit_State);
        }
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.L)) {
            GoLogin();
        }

    }
    public void GoLogin() {
        GameStateMachineManager.Instance.OnChangeState(GameStateMachineManager.Instance.gameLogin_State);
    }

    public void ConditionsCheck() {
        switch (GameStateMachineManager.Instance.currentGameState) {
            case State.Init:
            if (initCondition1) GameStateMachineManager.Instance.OnChangeState(GameStateMachineManager.Instance.gameLogin_State);
            break;
            case State.Login:
            case State.Ready:
            case State.Start:
            case State.Result:
            case State.End:
            case State.Disconnected:
            break;
        }
    }
}
