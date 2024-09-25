using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Observing;
using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkObserver))]
[RequireComponent(typeof(ObjectManipulator))]
public class AuthorizedGameObject : NetworkBehaviour {
    public ObjectManipulator objManipulator;
    public NetworkObject networkObj;

    private void Awake() {
        objManipulator = GetComponent<ObjectManipulator>();
        networkObj = GetComponent<NetworkObject>();
        RegisterFunctions();
    }

    public void RegisterFunctions() {
        objManipulator.firstSelectEntered.AddListener(delegate {
            AssignAuthorityToPlayerServer(InstanceFinder.NetworkManager.ClientManager.Connection, networkObj);
        });

        objManipulator.lastSelectExited.AddListener(delegate {
            RemoveAuthorityFromPlayerServer(networkObj);
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void AssignAuthorityToPlayerServer(NetworkConnection player, NetworkObject obj) {
        if (InstanceFinder.ServerManager) {
            obj.GiveOwnership(player);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveAuthorityFromPlayerServer(NetworkObject obj) {
        if (InstanceFinder.ServerManager) {
            obj.RemoveOwnership();
        }
    }
}
