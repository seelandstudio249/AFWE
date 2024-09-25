using FishNet.Component.Animating;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationNetworking : ManagerNetworkingBaseScript {
    public NetworkAnimator animator;

    [ServerRpc(RequireOwnership = false)]
    public void PlayStopAnimationServer(string animationName) {
        if (animationName != null) {
            animator.Play(animationName);
        } else {
            animator.Play("Idle");
        }
    }
}
