using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Observing;
using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct DrawingData {
    public int DrawingId;
    public List<Vector3> Points;
    public Color Color;
    public float LineWidth;
}

[RequireComponent(typeof(PinchTracker))]
public class DrawingFunction : ManagerBaseScript {
    [Serializable]
    public class MRButtonSettings {
        public MRButtonClass mrButton;
        public string color;
    }

    public List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public bool drawingStatus = false;
    public Color currentSelectedColor = Color.white;
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    public PinchTracker pinchTracker;
    public GameObject lineDrawPrefab;
    public GameObject currentLine;

    private Transform leftHandClosePoint;
    private Transform rightHandClosePoint;
    public bool wasPinchingLastFrame = false;

    #region UI
    [Header("UI")]
    [SerializeField] MRButtonClass drawingStatusButton;
    [SerializeField] MRButtonSettings redButton;
    [SerializeField] MRButtonSettings blueButton;
    [SerializeField] MRButtonSettings greenButton;
    [SerializeField] MRButtonSettings blackButton;
    [SerializeField] MRButtonSettings whiteButton;
    #endregion

    protected override void Awake() {
        base.Awake();
        drawingStatusButton.button.OnClicked.AddListener(delegate { 
            drawingStatus = !drawingStatus;
            drawingStatusButton.buttonText.text = string.Format("Drawing Status: {0}", drawingStatus);
        });
        redButton.mrButton.button.OnClicked.AddListener(delegate { 
            ChangeColor("red"); 
        });
        blueButton.mrButton.button.OnClicked.AddListener(delegate { 
            ChangeColor("blue"); 
        });
        greenButton.mrButton.button.OnClicked.AddListener(delegate { 
            ChangeColor("green"); 
        });
        blackButton.mrButton.button.OnClicked.AddListener(delegate { 
            ChangeColor("black"); 
        });
        whiteButton.mrButton.button.OnClicked.AddListener(delegate {
            ChangeColor("");
        });

        pinchTracker = GetComponent<PinchTracker>();
    }

    private void Start() {
        CacheHandPoints();
    }

    private void Update() {
        Vector3 handPosition = GetHandPosition();
        bool currentlyPinching = IsPinching();
        switch (drawingStatus) {
            case true:
            DisableObjectManipulators();
            if (currentlyPinching && !wasPinchingLastFrame) {
                StartLocalDrawing(handPosition);
            } else if (!currentlyPinching && wasPinchingLastFrame) {
                CompleteDrawing();
            } else if (currentlyPinching) {
                UpdateLocalDrawing(handPosition);
            }
            break;
            case false:
            EnableObjectManipulators();
            break;
        }
        wasPinchingLastFrame = currentlyPinching;
    }

    #region Color Change
    public Color GetColorFromString(string colorName) {
        colorName = colorName.ToLower();
        Color defaultColor = Color.white;
        switch (colorName) {
            case "red":
            return Color.red;
            case "blue":
            return Color.blue;
            case "green":
            return Color.green;
            case "black":
            return Color.cyan;
            default:
            return defaultColor;
        }
    }

    void ChangeColor(string colorName) {
        currentSelectedColor = GetColorFromString(colorName);
        foreach (LineRenderer renderer in lineRenderers) {
            renderer.sharedMaterial.color = currentSelectedColor;
        }
    }
    #endregion

    #region Object Manipulation
    public void EnableObjectManipulators() {
        foreach (var manipulator in FindObjectsOfType<ObjectManipulator>()) {
            manipulator.enabled = true;
        }
    }

    public void DisableObjectManipulators() {
        foreach (var manipulator in FindObjectsOfType<ObjectManipulator>()) {
            if (manipulator.gameObject != currentLine || manipulator.gameObject == currentLine)
                manipulator.enabled = false;
        }
    }
    #endregion

    #region Hand Data
    private void CacheHandPoints() {
        GameObject leftHand = GameObject.Find("MRTK LeftHand Controller");
        if (leftHand != null) {
            leftHandClosePoint = leftHand.transform.Find("IndexTip PokeInteractor/PokeReticle/RingVisual");
        }

        GameObject rightHand = GameObject.Find("MRTK RightHand Controller");
        if (rightHand != null) {
            rightHandClosePoint = rightHand.transform.Find("IndexTip PokeInteractor/PokeReticle/RingVisual");
        }
    }

    public Vector3 GetHandPosition() {
        if (pinchTracker.leftHandTriggerStatus && leftHandClosePoint != null) {
            return leftHandClosePoint.position;
        } else if (pinchTracker.rightHandTriggerStatus && rightHandClosePoint != null) {
            return rightHandClosePoint.position;
        }
        return Vector3.zero;
    }

    public bool IsPinching() {
        return pinchTracker != null && (pinchTracker.leftHandTriggerStatus || pinchTracker.rightHandTriggerStatus);
    }
    #endregion

    #region Local Drawing
    [SerializeField] private int currentDrawingId;
    public Dictionary<int, GameObject> drawingsById = new Dictionary<int, GameObject>();
    public List<GameObject> drawingObjects = new List<GameObject>();

    private int GenerateDrawingId() {
        Guid guid = Guid.NewGuid();
        int hash = guid.GetHashCode();

        int newId = hash;
        return newId;
    }

    private void StartLocalDrawing(Vector3 startPosition) {
        currentDrawingId = GenerateDrawingId();
        DrawingData data = new DrawingData {
            DrawingId = currentDrawingId,
            Points = new List<Vector3> { startPosition },
            Color = currentSelectedColor,
            LineWidth = lineWidth
        };

        if (gameMode == GamePlayType.Multiplayer) {
            ((DrawingNetworking)networkingScript).RequestSpawnDrawingServerRpc(data);
        } else {
            GameObject newDrawing = Instantiate(lineDrawPrefab);
            LineRenderer lr = newDrawing.GetComponent<LineRenderer>();
            if (lr != null) {
                lr.material = new Material(lineMaterial);
                lr.material.color = data.Color;
                lr.startWidth = lr.endWidth = data.LineWidth;
                lr.positionCount = 1;
                lr.SetPosition(0, data.Points[0]);
            }
            NetworkObserver networkObserver = newDrawing.GetComponent<NetworkObserver>();
            NetworkTransform networkTransform = newDrawing.GetComponent<NetworkTransform>();
            NetworkObject networkObj = newDrawing.GetComponent<NetworkObject>();
            if (networkObserver != null) {
                Destroy(networkObserver);
            }
            if (networkTransform != null) {
                Destroy(networkTransform);
            }
            if (networkObj != null) {
                Destroy(networkObj);
            }
            newDrawing.SetActive(true);
            drawingsById[data.DrawingId] = newDrawing;
        }
    }

    private void UpdateLocalDrawing(Vector3 currentPosition) {
        if (gameMode == GamePlayType.Multiplayer) {
            ((DrawingNetworking)networkingScript).UpdateDrawingServerRpc(currentDrawingId, currentPosition);
        } else {
            if (drawingsById.TryGetValue(currentDrawingId, out GameObject drawingObject)) {
                LineRenderer lineRenderer = drawingObject.GetComponent<LineRenderer>();
                if (lineRenderer != null) {
                    int newIndex = lineRenderer.positionCount;
                    lineRenderer.positionCount = newIndex + 1;
                    lineRenderer.SetPosition(newIndex, currentPosition);
                }
            }
        }
    }

    private void CompleteDrawing() {
        if (gameMode == GamePlayType.Multiplayer) {
            ((DrawingNetworking)networkingScript).CompleteDrawingServerRpc(currentDrawingId);
        } else {
            if (drawingsById.TryGetValue(currentDrawingId, out GameObject drawingObject)) {
                drawingObjects.Add(drawingObject);
            }
        }
    }

    void RemoveAllLines() {
        foreach (GameObject drawingObject in drawingObjects) {
            Destroy(drawingObject);
        }
        drawingObjects.Clear();
    }
    #endregion
}
