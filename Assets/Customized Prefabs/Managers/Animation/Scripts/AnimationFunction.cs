using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Component.Animating;
using UnityEngine.UI;
using System.Reflection;
using Unity.VisualScripting;

public class AnimationFunction : ManagerBaseScript {
    [Serializable]
    public class ButtonClass {
        public MRButtonClass mrButton;
        public string animationName;
    }

    public Animator animator;
    Vector3 animatorObjectOriginalPosition;

    #region UI
    [Header("UI")]
    [SerializeField] ButtonClass[] buttons;
    [SerializeField] ButtonClass stopAnimationButton;
    #endregion

    protected override void Awake() {
        base.Awake();
        animatorObjectOriginalPosition = animator.transform.localPosition;

        AssignGameMode += delegate {
            if (gameMode == GamePlayType.Multiplayer) {
                animator.gameObject.SetActive(false);
            }
        };

        foreach (ButtonClass item in buttons) {
            item.mrButton.button.OnClicked.AddListener(delegate { PlayAnimationButtonOnClick(item.animationName); });
        }

        stopAnimationButton.mrButton.button.OnClicked.AddListener(delegate {
            StopAnimationOnClick();
        });
    }

    void PlayAnimationButtonOnClick(string animationName) {
        if (gameMode == GamePlayType.Multiplayer) {
            ((AnimationNetworking)networkingScript).PlayStopAnimationServer(animationName);
        } else {
            PlayAnimation(animationName);
        }
    }

    void StopAnimationOnClick() {
        if (gameMode == GamePlayType.Multiplayer) {
            ((AnimationNetworking)networkingScript).PlayStopAnimationServer(null);
        } else {
            StopAnimation();
        }
    }

    public void PlayAnimation(string animationName) {
        animator.enabled = true;
        animator.Play(animationName);
    }

    public void StopAnimation() {
        animator.enabled = false;
        animator.transform.localPosition = animatorObjectOriginalPosition;
    }
}
