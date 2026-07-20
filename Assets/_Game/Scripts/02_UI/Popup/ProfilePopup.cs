using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup thông tin người dùng.
/// Mode = Popup trong Inspector.
/// </summary>
public class ProfilePopup : UIWindow {
    [Header("Header")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button EditNameButton;

    [Header("Avatar ScrollView")]
    [SerializeField] private Transform viewport;
    //[SerializeField] private AvatarButtonItem avatarButtonPrefab;

    [Header("Data")]
    [SerializeField] private AvatarDatabase avatarDatabase;

    [SerializeField] private FrameDatabase frameDatabase;
}
