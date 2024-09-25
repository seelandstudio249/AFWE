using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingNetworking : ManagerNetworkingBaseScript {
    #region Network Drawing
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnDrawingServerRpc(DrawingData data) {
        GameObject newDrawing = Instantiate(((DrawingFunction)managerScript).lineDrawPrefab);
        LineRenderer lr = newDrawing.GetComponent<LineRenderer>();
        if (lr != null) {
            lr.material = new Material(((DrawingFunction)managerScript).lineMaterial);
            lr.material.color = data.Color;
            lr.startWidth = lr.endWidth = data.LineWidth;
            lr.positionCount = 1;
            lr.SetPosition(0, data.Points[0]);
        }

        ServerManager.Spawn(newDrawing);
        ((DrawingFunction)managerScript).drawingsById[data.DrawingId] = newDrawing;

        BroadcastStartDrawingObserversRpc(data.DrawingId, newDrawing);
    }
    [ObserversRpc(BufferLast = true)]
    private void BroadcastStartDrawingObserversRpc(int drawingID, GameObject go) {
        ((DrawingFunction)managerScript).drawingsById[drawingID] = go;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateDrawingServerRpc(int drawingId, Vector3 newPosition) {
        if (((DrawingFunction)managerScript).drawingsById.TryGetValue(drawingId, out GameObject drawingObject)) {
            LineRenderer lineRenderer = drawingObject.GetComponent<LineRenderer>();
            if (lineRenderer != null) {
                int newIndex = lineRenderer.positionCount;
                lineRenderer.positionCount = newIndex + 1;
                lineRenderer.SetPosition(newIndex, newPosition);
                UpdateDrawingObserversRpc(drawingId, newPosition, lineRenderer.material.color, lineRenderer.startWidth);
            }
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void UpdateDrawingObserversRpc(int drawingId, Vector3 newPosition, Color color, float lineWidth) {
        if (((DrawingFunction)managerScript).drawingsById.TryGetValue(drawingId, out GameObject drawingObject)) {
            LineRenderer lr = drawingObject.GetComponent<LineRenderer>();
            if (lr != null) {
                lr.positionCount += 1;
                lr.SetPosition(lr.positionCount - 1, newPosition);

                lr.material.color = color;
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void CompleteDrawingServerRpc(int drawingId) {
        if (((DrawingFunction)managerScript).drawingsById.TryGetValue(drawingId, out GameObject drawingObject)) {
            CompleteDrawingObserversRpc(drawingObject);
            ((DrawingFunction)managerScript).drawingObjects.Add(drawingObject);
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void CompleteDrawingObserversRpc(GameObject drawingObject) {
        LineRenderer lr = drawingObject.GetComponent<LineRenderer>();
        if (lr != null) {
            Bounds bounds = new Bounds(lr.GetPosition(0), Vector3.zero);
            for (int i = 0; i < lr.positionCount; i++) {
                bounds.Encapsulate(lr.GetPosition(i));
            }

            drawingObject.transform.position = bounds.center;
            for (int i = 0; i < lr.positionCount; i++) {
                lr.SetPosition(i, lr.GetPosition(i) - bounds.center);
            }
            BoxCollider collider = drawingObject.GetComponent<BoxCollider>() ?? drawingObject.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = bounds.size;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveAllLines() {
        foreach (GameObject drawingObject in ((DrawingFunction)managerScript).drawingObjects) {
            Despawn(drawingObject);
        }
        ((DrawingFunction)managerScript).drawingObjects.Clear();
    }
    #endregion
}
