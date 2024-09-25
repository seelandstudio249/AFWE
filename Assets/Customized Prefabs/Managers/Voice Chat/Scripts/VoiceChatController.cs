using Dissonance;
using MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoiceChatController : ManagerBaseScript {
    [SerializeField] DissonanceComms dissonanceComms;
    [SerializeField] VoiceBroadcastTrigger voiceBroadcastTrigger;

    #region UI
    [Header("UI")]
    [SerializeField] MRButtonClass muteButton;
    [SerializeField] MRButtonClass unmuteButton;
    [SerializeField] TMP_Text statusText;

    [SerializeField] private float defaultVolume = 0.5f;
    [SerializeField] private Slider micVolumeSliderObject;
    [SerializeField] private TextMeshPro micVolumeSliderValueText;

    [SerializeField] private float defaultRemoteVolume = 0.5f;
    [SerializeField] private Slider remoteVolumeSliderObject;
    [SerializeField] private TextMeshPro remoteVolumeSliderValueText;
    #endregion

    protected override void Awake() {
        base.Awake();
        muteButton.button.OnClicked.AddListener(delegate {
            ToggleMuted(true);
            statusText.text = "Muted";
        });

        unmuteButton.button.OnClicked.AddListener(delegate {
            ToggleMuted(false);
            statusText.text = "Unmuted";
        });
        micVolumeSliderObject.OnValueUpdated.AddListener(OnVolumeSliderUpdated);
        remoteVolumeSliderObject.OnValueUpdated.AddListener(OnRemoteVolumeSliderUpdated);
    }

    public void ToggleMuted(bool isMuted) {
        dissonanceComms.IsMuted = isMuted;
    }

    public void OnVolumeSliderUpdated(SliderEventData e) {
        SetFaderVolumes(e.NewValue);
    }

    public void SetFaderVolumes(float volume) {
        volume = (float)Math.Round(volume, 2);
        voiceBroadcastTrigger.ActivationFader.Volume = volume;
        voiceBroadcastTrigger.ColliderTriggerFader.Volume = volume;
        micVolumeSliderValueText.text = string.Format("Mic Volume: {0}", volume.ToString("0.00"));
    }

    public void OnRemoteVolumeSliderUpdated(SliderEventData e) {
        SetRemoteVoiceVolume(e.NewValue);
    }

    public void SetRemoteVoiceVolume(float volume) {
        dissonanceComms.RemoteVoiceVolume = volume;
        remoteVolumeSliderValueText.text = string.Format("Remote Volume: {0}", volume.ToString("0.00"));
    }
}
