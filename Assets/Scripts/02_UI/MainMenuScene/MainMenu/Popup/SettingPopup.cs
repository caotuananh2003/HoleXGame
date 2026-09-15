using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : PopupWindow
{
    [Serializable]
    private struct ToggleButton
    {
        public Button     button;
        public GameObject onImage;
        public GameObject offImage;

        public readonly void SetActive(bool isOn)
        {
            if (onImage  != null) onImage.SetActive(isOn);
            if (offImage != null) offImage.SetActive(!isOn);
        }
    }

    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    [Header("Audio Buttons")]
    [SerializeField] private ToggleButton musicButton;
    [SerializeField] private ToggleButton soundButton;
    [SerializeField] private ToggleButton vibraButton;

    private void Start()
    {
        if (closeButton        != null) closeButton.onClick.AddListener(OnCloseClicked);
        if (musicButton.button != null) musicButton.button.onClick.AddListener(OnMusicClicked);
        if (soundButton.button != null) soundButton.button.onClick.AddListener(OnSoundClicked);
        if (vibraButton.button != null) vibraButton.button.onClick.AddListener(OnVibraClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (closeButton        != null) closeButton.onClick.RemoveListener(OnCloseClicked);
        if (musicButton.button != null) musicButton.button.onClick.RemoveListener(OnMusicClicked);
        if (soundButton.button != null) soundButton.button.onClick.RemoveListener(OnSoundClicked);
        if (vibraButton.button != null) vibraButton.button.onClick.RemoveListener(OnVibraClicked);
    }

    public override void Open()
    {
        base.Open();
        UIManager?.PlaySFX(AudioID.SFX.UiPopup);
        SyncButtonVisuals();
    }

    private void SyncButtonVisuals()
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        musicButton.SetActive(!am.IsBGMMuted);
        soundButton.SetActive(!am.IsSFXMuted);
        vibraButton.SetActive(am.IsVibrationEnabled);
    }

    private void OnCloseClicked() => UIManager?.Close<SettingPopup>();

    private void OnMusicClicked()
    {
        var am = AudioManager.Instance; if (am == null) return;
        bool nowMuted = !am.IsBGMMuted;
        am.SetBGMMuted(nowMuted);
        musicButton.SetActive(!nowMuted);
    }

    private void OnSoundClicked()
    {
        var am = AudioManager.Instance; if (am == null) return;
        bool nowMuted = !am.IsSFXMuted;
        am.SetSFXMuted(nowMuted);
        soundButton.SetActive(!nowMuted);
    }

    private void OnVibraClicked()
    {
        var am = AudioManager.Instance; if (am == null) return;
        bool nowEnabled = !am.IsVibrationEnabled;
        am.SetVibration(nowEnabled);
        vibraButton.SetActive(nowEnabled);
    }
}
