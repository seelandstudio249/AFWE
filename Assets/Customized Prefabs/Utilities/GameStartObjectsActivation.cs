using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ObjClass {
    [Header("Objects to turn on")]
    public GameObject[] objectsToActive;
    [Header("Objects to turn off")]
    public GameObject[] objectToDeActive;

}

public class GameStartObjectsActivation : NetworkBehaviour {
    [Header("Objects")]
    [Header("When server start")]
    public ObjClass objsServer;
    [Header("When client start")]
    public ObjClass objsClient;
    [Header("When disconnected")]
    public ObjClass objsDisconnected;

    public override void OnStartServer() {
        base.OnStartServer();
        foreach(GameObject obj in objsServer.objectsToActive) {

        }
        foreach (GameObject obj in objsServer.objectToDeActive) {

        }
    }

    public override void OnStartClient() {
        base.OnStartClient();
        foreach (GameObject obj in objsClient.objectsToActive) {

        }
        foreach (GameObject obj in objsClient.objectToDeActive) {

        }
    }

    public override void OnStopClient() {
        base.OnStopClient();
        foreach (GameObject obj in objsDisconnected.objectsToActive) {

        }
        foreach (GameObject obj in objsDisconnected.objectToDeActive) {

        }
    }
}
