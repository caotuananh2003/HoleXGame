using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup nhập tên người chơi.
/// Mode = Popup trong Inspector.
///
/// Flow:
///   EditProfilePopup.editNameButton → Open EditNamePopup
///   → Người chơi click vào nameInputField → nhập tên → ContinueButton active
///   → ContinueButton → gọi EditProfilePopup.ApplyNameFromPopup() → Close
///
/// ContinueButton có 2 trạng thái visual:
///   - enableObject  : hiện khi đã từng click vào InputField (có thể confirm)
///   - disableObject : hiện khi chưa click vào InputField lần nào (mặc định)
/// </summary>
public class EditNamePopup : UIWindow
{
    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    [Header("Input")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Continue Button")]
    [SerializeField] private Button     continueButton;
    [SerializeField] private GameObject enableObject;  // Hiện khi InputField đã được chạm vào
    [SerializeField] private GameObject disableObject; // Hiện khi chưa chạm vào InputField

    // =========================================================================
    // Runtime state
    // =========================================================================

    private bool hasInteractedWithInput; // True khi người dùng đã click vào InputField ít nhất 1 lần

    // =========================================================================
    // UIWindow override
    // =========================================================================

    public override void Open()
    {
        base.Open();

        // Reset state mỗi lần mở
        hasInteractedWithInput = false;
        RefreshContinueButtonVisual();

        // Hiển thị tên hiện tại từ EditProfilePopup
        EditProfilePopup editProfilePopup = UIManager?.GetWindow<EditProfilePopup>();
        if (nameInputField != null && editProfilePopup != null)
            nameInputField.SetTextWithoutNotify(editProfilePopup.CurrentEditingName);
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        ValidateInspectorRefs();
        RegisterListeners();
    }

    private void OnDestroy()
    {
        UnregisterListeners();
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    private void OnCloseClicked()
    {
        UIManager?.Close<EditNamePopup>();
    }

    private void OnInputFieldSelected(string _)
    {
        // Chỉ cần click vào InputField một lần là đủ để unlock ContinueButton
        if (hasInteractedWithInput) return;

        hasInteractedWithInput = true;
        RefreshContinueButtonVisual();
    }

    private void OnContinueClicked()
    {
        // Nếu chưa tương tác thì không làm gì — ContinueButton vẫn ở trạng thái disable visual
        if (!hasInteractedWithInput) return;

        string newName = nameInputField != null
            ? nameInputField.text.Trim()
            : string.Empty;

        // Đẩy tên mới về EditProfilePopup — chưa save, chỉ cập nhật UI
        EditProfilePopup editProfilePopup = UIManager?.GetWindow<EditProfilePopup>();
        editProfilePopup?.ApplyNameFromPopup(newName);

        UIManager?.Close<EditNamePopup>();
    }

    // =========================================================================
    // Visual
    // =========================================================================

    /// <summary>
    /// Cập nhật trạng thái visual của ContinueButton.
    /// enableObject  hiện khi hasInteractedWithInput = true.
    /// disableObject hiện khi hasInteractedWithInput = false.
    /// </summary>
    private void RefreshContinueButtonVisual()
    {
        if (enableObject  != null) enableObject.SetActive(hasInteractedWithInput);
        if (disableObject != null) disableObject.SetActive(!hasInteractedWithInput);
    }

    // =========================================================================
    // Register / Unregister
    // =========================================================================

    private void RegisterListeners()
    {
        if (closeButton    != null) closeButton.onClick.AddListener(OnCloseClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);

        // onSelect fire khi người dùng click/tap vào InputField
        if (nameInputField != null) nameInputField.onSelect.AddListener(OnInputFieldSelected);
    }

    private void UnregisterListeners()
    {
        if (closeButton    != null) closeButton.onClick.RemoveListener(OnCloseClicked);
        if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
        if (nameInputField != null) nameInputField.onSelect.RemoveListener(OnInputFieldSelected);
    }

    // =========================================================================
    // Validate
    // =========================================================================

    private void ValidateInspectorRefs()
    {
        if (closeButton    == null) Debug.LogWarning("[EditNamePopup] closeButton is not assigned.");
        if (nameInputField == null) Debug.LogWarning("[EditNamePopup] nameInputField is not assigned.");
        if (continueButton == null) Debug.LogWarning("[EditNamePopup] continueButton is not assigned.");
        if (enableObject   == null) Debug.LogWarning("[EditNamePopup] enableObject is not assigned.");
        if (disableObject  == null) Debug.LogWarning("[EditNamePopup] disableObject is not assigned.");
    }
}
