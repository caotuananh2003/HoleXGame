using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào mỗi button trong BottomPanel hoặc bất kỳ tab group nào.
/// selectedVisual: GameObject bật khi tab đang active.
/// unSelectedVisual: GameObject bật khi tab không active.
///
/// Awake() có thể chưa chạy khi Initialize() được gọi lần đầu
/// (nếu GameObject đang inactive lúc đó).
/// Dùng lazy property để đảm bảo button luôn được resolve trước khi dùng.
/// </summary>
[RequireComponent(typeof(Button))]
public class TabButton : MonoBehaviour
{
    [SerializeField] private GameObject selectedVisual;
    [SerializeField] private GameObject unSelectedVisual;

    // Lazy: GetComponent chỉ gọi lần đầu, cache lại sau đó.
    private Button _button;
    private Button button => _button != null ? _button : (_button = GetComponent<Button>());

    public void Initialize(Action onClicked)
    {
        button.onClick.AddListener(() => onClicked?.Invoke());
    }

    public void SetSelected(bool selected) // Gọi hàm này mỗi khi đổi tab từ Action onClicked tạo đã gán cho Listener trong Initialize
    {
        if (selectedVisual   != null) selectedVisual.SetActive(selected);
        if (unSelectedVisual != null) unSelectedVisual.SetActive(!selected);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveAllListeners();
    }
}
