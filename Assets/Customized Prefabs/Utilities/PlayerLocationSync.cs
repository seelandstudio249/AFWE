using FishNet.Component.Spawning;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocationSync : NetworkBehaviour
{
    [SerializeField] PlayerSpawner playerSpawner;
    [SerializeField] List<NetworkObject> nob = new List<NetworkObject>();

    // Start is called before the first frame update
    void Start()
    {
        playerSpawner.OnSpawned += CoLocatePlayer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CoLocatePlayer(NetworkObject tst) {
        CoLocatePlayerObs(tst);
    }

    [ServerRpc]
    private void testing4(NetworkObject tst) {
        CoLocatePlayerObs(tst);
    }

    [ObserversRpc(BufferLast = true)]
    private void CoLocatePlayerObs(NetworkObject tst) {
        nob.Add(tst);
        foreach(NetworkObject obj in nob) {
            obj.transform.parent = GameObject.FindGameObjectWithTag("GameContainer").transform;
        }
    }
}
