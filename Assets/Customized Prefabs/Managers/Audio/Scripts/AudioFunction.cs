using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AudioClass {
    public AudioClip audioClip;
    public bool isLoop;
    public float volume;
}

[Serializable]
public enum AudioType {
    BGM,
    OneTimeAudio
}

public class AudioFunction : ManagerBaseScript {
    [SerializeField] protected AudioSource backgroundMusicPlayer, oneTimeAudioPlayer;

    [SerializeField] protected AudioClass[] oneShotAudioList;
    [SerializeField] protected AudioClass[] bgmAudioList;

    #region Buttons
    [Header("UI")]
    [SerializeField] MRButtonClass PlayAudio1;
    [SerializeField] MRButtonClass PlayAudio2;
    [SerializeField] MRButtonClass PlayAudio3;
    [SerializeField] MRButtonClass PlayBGM;
    [SerializeField] MRButtonClass StopAudio1;
    [SerializeField] MRButtonClass StopAudio2;
    [SerializeField] MRButtonClass StopAudio3;
    [SerializeField] MRButtonClass StopBGM;
    #endregion

    protected override void Awake() {
        base.Awake();
        PlayAudio1.button.OnClicked.AddListener(delegate { PlayAudio(0, AudioType.OneTimeAudio); });
        PlayAudio2.button.OnClicked.AddListener(delegate { PlayAudio(1, AudioType.OneTimeAudio); });
        PlayAudio3.button.OnClicked.AddListener(delegate { PlayAudio(2, AudioType.OneTimeAudio); });
        PlayBGM.button.OnClicked.AddListener(delegate { PlayAudio(0, AudioType.BGM); });
        StopAudio1.button.OnClicked.AddListener(delegate { StopAudio(AudioType.OneTimeAudio); });
        StopAudio2.button.OnClicked.AddListener(delegate { StopAudio(AudioType.OneTimeAudio); });
        StopAudio3.button.OnClicked.AddListener(delegate { StopAudio(AudioType.OneTimeAudio); });
        StopBGM.button.OnClicked.AddListener(delegate { StopAudio(AudioType.BGM); });
        //GameStateMachineManager.Instance.gameEnd_State.onInit_Event += delegate {
        //    StopAudio(AudioType.BGM);
        //    StopAudio(AudioType.OneTimeAudio);
        //};
    }

    private void Start() {
        PlayAudioFoundation(backgroundMusicPlayer, bgmAudioList[0]);
    }

    public void PlayAudio(int index, AudioType audioType) {
        if (gameMode == GamePlayType.Multiplayer) {
            ((AudioNetworking)networkingScript).PlayStopAudioServer(index, audioType);
        } else {
            switch (audioType) {
                case AudioType.BGM:
                PlayAudioFoundation(backgroundMusicPlayer, bgmAudioList[index]);
                break;
                case AudioType.OneTimeAudio:
                PlayAudioFoundation(oneTimeAudioPlayer, oneShotAudioList[index]);
                break;
            }
        }
    }

    public void StopAudio(AudioType audioType) {
        if (gameMode == GamePlayType.Multiplayer) {
            ((AudioNetworking)networkingScript).PlayStopAudioServer(-1, audioType);
        } else {
            switch (audioType) {
                case AudioType.BGM:
                StopAudioFoundation(backgroundMusicPlayer);
                break;
                case AudioType.OneTimeAudio:
                StopAudioFoundation(oneTimeAudioPlayer);
                break;
            }
        }
    }

    public void ServerTurnOnOffAudio(int index, AudioType audioType) {
        if (index >= 0) {
            switch (audioType) {
                case AudioType.BGM:
                PlayAudioFoundation(backgroundMusicPlayer, bgmAudioList[index]);
                break;
                case AudioType.OneTimeAudio:
                PlayAudioFoundation(oneTimeAudioPlayer, oneShotAudioList[index]);
                break;
            }
        } else {
            switch (audioType) {
                case AudioType.BGM:
                StopAudioFoundation(backgroundMusicPlayer);
                break;
                case AudioType.OneTimeAudio:
                StopAudioFoundation(oneTimeAudioPlayer);
                break;
            }
        }
    }

    public void PlayAudioFoundation(AudioSource audioSource, AudioClass audioData) {
        audioSource.Stop();
        audioSource.loop = audioData.isLoop;
        audioSource.clip = audioData.audioClip;
        audioSource.volume = audioData.volume;
        audioSource.Play();
    }

    public void StopAudioFoundation(AudioSource audioSource) {
        audioSource.Stop();
    }
}
