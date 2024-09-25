using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmUpChecker : MonoBehaviour
{
    public float facingCameraTrackingThreshold = 80.0f;
    public bool requireFlatHand = false;

    private ControllerLookup controllerLookup;
    public ControllerLookup ControllerLookup => controllerLookup;
    public ArticulatedHandController LeftHand => (ArticulatedHandController)controllerLookup.LeftHandController;
    public ArticulatedHandController RightHand => (ArticulatedHandController)controllerLookup.RightHandController;
    public bool leftHandUp, rightHandUp;

    private void Awake() {
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

    void Update() {
        leftHandUp = CheckPalmOrientation(LeftHand);
        rightHandUp = CheckPalmOrientation(RightHand);
    }

    private bool CheckPalmOrientation(ArticulatedHandController hand) {
        if (XRSubsystemHelpers.HandsAggregator != null &&
            XRSubsystemHelpers.HandsAggregator.TryGetJoint(TrackedHandJoint.Palm, hand.HandNode, out HandJointPose palmPose)) {
            // Calculate dot product between palm up vector and camera forward vector
            float dotProduct = Vector3.Dot(palmPose.Up, Camera.main.transform.forward);

            // Calculate the angle in degrees
            float palmCameraAngle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            // Check if the palm is facing up (towards the camera)
            if (palmCameraAngle < facingCameraTrackingThreshold) {
                if (IsPalmMeetingThresholdRequirements(hand, palmPose, palmCameraAngle)) {
                    Debug.Log($"{hand} hand's palm is facing up.");
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsPalmMeetingThresholdRequirements(ArticulatedHandController hand, HandJointPose palmPose, float palmCameraAngle) {
        if (requireFlatHand) {
            // Example flat hand requirement check: Adjust to your specific criteria
            if (XRSubsystemHelpers.HandsAggregator.TryGetJoint(TrackedHandJoint.IndexTip, hand.HandNode, out HandJointPose indexTipPose) &&
                XRSubsystemHelpers.HandsAggregator.TryGetJoint(TrackedHandJoint.RingTip, hand.HandNode, out HandJointPose ringTipPose)) {
                Vector3 indexToPalm = palmPose.Position - indexTipPose.Position;
                Vector3 ringToPalm = palmPose.Position - ringTipPose.Position;
                float indexRingAngle = Vector3.Angle(indexToPalm, ringToPalm);
                return indexRingAngle > 150.0f;
            }
            return false;
        }

        return true;
    }
}
