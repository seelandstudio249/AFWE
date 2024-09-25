using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadNetworking : ManagerNetworkingBaseScript {
    public override void OnStartClient() {
        base.OnStartClient();
        if (managerScript != null) {
            if (managerScript) StartCoroutine(((DontDestroyOnLoadManager)managerScript).SetUpDontDestroyObjs());
        }
    }

    public override void OnStartServer() {
        base.OnStartServer();
        if (managerScript != null) {
            if (managerScript) StartCoroutine(((DontDestroyOnLoadManager)managerScript).SetUpDontDestroyObjs());
        }
    }

    public override void OnStartNetwork() {
        base.OnStartNetwork();
        if (managerScript != null) {
            if (managerScript) StartCoroutine(((DontDestroyOnLoadManager)managerScript).SetUpDontDestroyObjs());
        }
    }
}
