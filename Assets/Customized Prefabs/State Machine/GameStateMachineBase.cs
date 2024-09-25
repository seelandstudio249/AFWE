
using System;
using UnityEngine;

public abstract class GameStateMachineBase : MonoBehaviour
{
    public delegate void OnInitEventDelegate();
    public delegate void OnStartEventDelegate();
    public delegate void OnEndEventDelegate();

    public OnInitEventDelegate onInit_Event;
    public OnStartEventDelegate onStart_Event;
    public OnEndEventDelegate onEnd_Event;
    
    public virtual void Awake() {
        onInit_Event+= delegate {  };
        onStart_Event += delegate {  };
        onEnd_Event += delegate {  };
    }

    public abstract void OnStateInit(GameStateMachineManager gameStateMachineManager);
   public abstract void OnStateStart(GameStateMachineManager gameStateMachineManager);
   public abstract void OnStateEnd(GameStateMachineManager gameStateMachineManager);
}

