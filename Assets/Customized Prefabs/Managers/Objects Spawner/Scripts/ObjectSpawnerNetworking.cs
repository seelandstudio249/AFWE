using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnerNetworking : ManagerNetworkingBaseScript {
    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectServer(int index) {
        if (index >= 0) {
            GameObject obj = ((ObjectSpawnerFunction)managerScript).SpawnObject(index);
            spawnedObjectData objData = new spawnedObjectData {
                spawnedObject = obj,
                oriPosition = obj.transform.position
            };
            obj.transform.parent = ((ObjectSpawnerFunction)managerScript).objectsHolder;
            obj.transform.localPosition = objData.oriPosition;
            InstanceFinder.ServerManager.Spawn(obj);
            ((ObjectSpawnerFunction)managerScript).spawnedObjects.Add(objData);
            SpawnObjectObserver(((ObjectSpawnerFunction)managerScript).spawnedObjects);
        } else {
            foreach (spawnedObjectData obj in ((ObjectSpawnerFunction)managerScript).spawnedObjects) {
                Despawn(obj.spawnedObject);
            }
            ((ObjectSpawnerFunction)managerScript).spawnedObjects.Clear();
        }

        [ObserversRpc(BufferLast = true)]
        void SpawnObjectObserver(List<spawnedObjectData> spawnedObjects) {
            foreach (spawnedObjectData obj in spawnedObjects) {
                obj.spawnedObject.transform.parent = ((ObjectSpawnerFunction)managerScript).objectsHolder;
                obj.spawnedObject.transform.localPosition = obj.oriPosition;
            }
        }
    }
}
