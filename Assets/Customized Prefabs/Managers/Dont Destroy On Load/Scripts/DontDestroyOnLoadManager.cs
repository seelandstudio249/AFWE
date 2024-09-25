using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadManager : ManagerBaseScript {
    [SerializeField] GameObject[] dontDestroyObjs;

    public IEnumerator SetUpDontDestroyObjs() {
        yield return new WaitForSeconds(0.1f);
        DontDestroyOnLoad(this.gameObject);
        foreach (GameObject gameObject in dontDestroyObjs) {
            DontDestroyOnLoad(gameObject);
        }
    }
}
