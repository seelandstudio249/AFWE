using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Observing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthorityManager : ManagerBaseScript
{
    [Serializable] public class ObjectsData {
        public GameObject gameObject;
        public NetworkObject networkObject;
        public NetworkObserver networkObserver;
        public AuthorizedGameObject authorizedGameObject;
        public NetworkTransform networkTransform;
    }

    [SerializeField] ObjectsData[] objects;

    protected override void AssignGamePlayType(GamePlayType gameMode) {
        base.AssignGamePlayType(gameMode);
        if(gameMode == GamePlayType.Singleplayer) {
            foreach (ObjectsData data in objects) {
                data.networkObject.enabled = false;
                data.networkObserver.enabled = false;
                data.authorizedGameObject.enabled = false;
                data.networkTransform.enabled = false;
                data.gameObject.SetActive(true);
            }
        }
    }
}
