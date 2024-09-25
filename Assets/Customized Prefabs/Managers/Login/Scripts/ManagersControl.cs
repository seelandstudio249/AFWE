using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagersControl : MonoBehaviour {
    [SerializeField] List<ManagerBaseScript> managersList;

    public void AssignGameMode(GamePlayType gamePlayType) {
        foreach (ManagerBaseScript manager in managersList) {
            manager.AssignGameMode.Invoke(gamePlayType);
        }
    }
}
