using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagersControl : MonoBehaviour {
    public Login loginScript;

    [SerializeField] List<ManagerBaseScript> managersList;

    public void AssignGameMode(GamePlayType gamePlayType) {
        foreach (ManagerBaseScript manager in managersList) {
            manager?.AssignGameMode?.Invoke(gamePlayType);
        }
    }

    public void AfterLogin() {
        foreach (ManagerBaseScript manager in managersList) {
            manager?.AfterLogin?.Invoke();
        }
    }

    public T GetSpecificManagerScript<T>()where T : ManagerBaseScript {
        foreach (ManagerBaseScript manager in managersList) {
            if (manager is T) {
                return manager as T;
            }
        }
        return null;
    }
}
