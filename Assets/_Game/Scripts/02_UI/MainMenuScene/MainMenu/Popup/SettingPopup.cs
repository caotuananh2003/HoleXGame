using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Popup cài đặt âm thanh.
/// Mode = Popup trong Inspector.
///
/// Khi mở: Toggle đọc trạng thái hiện tại từ AudioManager.
/// Khi Toggle thay đổi: cập nhật AudioManager (tự lưu qua SaveManager).
/// </summary>
public class SettingPopup : UIWindow
{
    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    // -------------------------------------------------------------------------
    // Audio
    // -------------------------------------------------------------------------

    [Header("Audio")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundToggle;

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private AudioManager audioManager;

    [Inject]
    private void Construct(AudioManager audioManager)
    {
        this.audioManager = audioManager;
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        if (closeButton == null)
            Debug.LogWarning("[SettingPopup] closeButton is not assigned in Inspector.");
        else
            closeButton.onClick.AddListener(OnCloseClicked);

        if (musicToggle == null)
            Debug.LogWarning("[SettingPopup] musicToggle is not assigned in Inspector.");
        else
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);

        if (soundToggle == null)
            Debug.LogWarning("[SettingPopup] soundToggle is not assigned in Inspector.");
        else
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);

        if (musicToggle != null)
            musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);

        if (soundToggle != null)
            soundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
    }

    // =========================================================================
    // UIWindow — sync UI khi popup được mở / bật lại
    // =========================================================================

    // OnEnable chạy mỗi khi SetActive(true) — đảm bảo toggle luôn phản ánh
    // trạng thái hiện tại khi popup xuất hiện, kể cả khi mở lại lần 2 trở đi.
    private void OnEnable()
    {
        SyncToggles();
    }

    // =========================================================================
    // UI sync
    // =========================================================================

    /// <summary>
    /// Đọc trạng thái từ AudioManager và cập nhật Toggle mà không trigger callback.
    /// </summary>
    private void SyncToggles()
    {
        if (audioManager == null)
        {
            Debug.LogWarning("[SettingPopup] audioManager is null. Cannot sync toggles.");
            return;
        }

        // Toggle.isOn = true nghĩa là âm thanh BẬT (không bị mute).
        // SetWithoutNotify() tránh trigger onValueChanged khi sync.
        if (musicToggle != null)
            musicToggle.SetIsOnWithoutNotify(!audioManager.IsBGMMuted);

        if (soundToggle != null)
            soundToggle.SetIsOnWithoutNotify(!audioManager.IsSFXMuted);
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    private void OnCloseClicked()
    {
        UIManager?.Close<SettingPopup>();
    }

    /// <summary>
    /// isOn = true → âm nhạc BẬT → không mute.
    /// isOn = false → âm nhạc TẮT → mute.
    /// </summary>
    private void OnMusicToggleChanged(bool isOn)
    {
        if (audioManager == null)
        {
            Debug.LogWarning("[SettingPopup] audioManager is null. Cannot change music state.");
            return;
        }

        audioManager.SetBGMMuted(!isOn);
    }

    /// <summary>
    /// isOn = true → âm thanh BẬT → không mute.
    /// isOn = false → âm thanh TẮT → mute.
    /// </summary>
    private void OnSoundToggleChanged(bool isOn)
    {
        if (audioManager == null)
        {
            Debug.LogWarning("[SettingPopup] audioManager is null. Cannot change sound state.");
            return;
        }

        audioManager.SetSFXMuted(!isOn);
    }
}
