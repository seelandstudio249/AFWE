using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;

public class PinchTracker : MonoBehaviour {
    HandsAggregatorSubsystem _HandsAggregatorSubsystem;

    private const float PinchTreshold = 0.95f;

    [Header("Hands Status")]
    [Tooltip("To detect if left hand is currently pinching")]
    public bool leftHandTriggerStatus = false;
    [Tooltip("To detect if right hand is currently pinching")]
    public bool rightHandTriggerStatus = false;
    [Space(0.5f)]

    [Header("Ray Tracker")]
    [Tooltip("To detect if left hand ray cast is hitting anything")]
    public bool leftHandGotPinchData = false;
    [Tooltip("To detect if right hand ray cast is hitting anything")]
    public bool rightHandGotPinchData = false;

    private ControllerLookup controllerLookup;
    public ControllerLookup ControllerLookup => controllerLookup;
    public ArticulatedHandController LeftHand => (ArticulatedHandController)controllerLookup.LeftHandController;
    public ArticulatedHandController RightHand => (ArticulatedHandController)controllerLookup.RightHandController;

    public UnityEvent<bool> LeftHandStatusTriggered { get; } = new UnityEvent<bool>();
    public UnityEvent<bool> RightHandStatusTriggered { get; } = new UnityEvent<bool>();

    // Start is called before the first frame update
    void Start()
    {
        GetHandControllerLookup();
        _HandsAggregatorSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();
    }

    private void GetHandControllerLookup() {
        if (controllerLookup == null) {
            ControllerLookup[] lookups = GameObject.FindObjectsOfType(typeof(ControllerLookup)) as ControllerLookup[];
            if (lookups.Length == 0) {
                Debug.LogError(
                    "Could not locate an instance of the ControllerLookup class in the hierarchy. It is recommended to add ControllerLookup to your camera rig.");
            } else if (lookups.Length > 1) {
                Debug.LogWarning(
                    "Found more than one instance of the ControllerLookup class in the hierarchy. Defaulting to the first instance.");
                controllerLookup = lookups[0];
            } else {
                controllerLookup = lookups[0];
            }
        }
    }

    private void Update() {
        GetHandPinchData();
        Pinching();
    }

    private void Pinching() {
        if (_HandsAggregatorSubsystem != null) {
            var newStatus = Pinching(LeftHand);
            if (newStatus != leftHandTriggerStatus) {
                leftHandTriggerStatus = newStatus;
                LeftHandStatusTriggered.Invoke(leftHandTriggerStatus);
            }

            newStatus = Pinching(RightHand);
            if (newStatus != rightHandTriggerStatus) {
                rightHandTriggerStatus = newStatus;
                RightHandStatusTriggered.Invoke(rightHandTriggerStatus);
            }
        }
    }

    private bool Pinching(ArticulatedHandController handController) {
        var progressDetectable = 
            _HandsAggregatorSubsystem.TryGetPinchProgress(handController.HandNode, 
                                                        out bool isReadyToPinch,
                                                        out bool isPinching,
                                                        out float pinchAmount);

        return progressDetectable && isPinching && pinchAmount > PinchTreshold;
    }

    private void GetHandPinchData() {
        leftHandGotPinchData = XRSubsystemHelpers.HandsAggregator.TryGetPinchProgress(
                    XRNode.LeftHand,
                    out bool isLeftPinchReady,
                    out bool isLeftPinching,
                    out float leftpinchAmount
                );
        rightHandGotPinchData = XRSubsystemHelpers.HandsAggregator.TryGetPinchProgress(
                    XRNode.RightHand,
                    out bool isRightPinchReady,
                    out bool isRightPinching,
                    out float rightPinchAmount
                );
    }
}
