using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing.Scened;

public class SceneManagerNetworking : ManagerNetworkingBaseScript
{
    [ServerRpc(RequireOwnership = false)]
    public void LoadSceneServer(string sceneName) {
        SceneLoadData sld = new SceneLoadData(sceneName);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        LoadSceneObserver(sceneName);
    }

    [ObserversRpc(BufferLast = true)]
    void LoadSceneObserver(string sceneName) {
        SceneLoadData sld = new SceneLoadData(sceneName);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnloadSceneServer(string sceneName) {
        SceneUnloadData sld = new SceneUnloadData(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sld);
        UnloadSceneObserver(sceneName);
    }

    [ObserversRpc(BufferLast =true)]
    void UnloadSceneObserver(string sceneName) {
        SceneUnloadData sld = new SceneUnloadData(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sld);
    }
}
