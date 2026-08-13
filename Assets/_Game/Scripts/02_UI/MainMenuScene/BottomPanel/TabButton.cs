using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào mỗi button trong BottomPanel.
/// selectedVisual: GameObject bật khi tab đang active (ví dụ: indicator line, highlight image).
/// </summary>

[RequireComponent(typeof(Button))]
public class TabButton : MonoBehaviour
{
    [SerializeField] private GameObject selectedVisual;
    [SerializeField] private GameObject unSelectedVisual;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(Action onClicked)
    {
        button.onClick.AddListener(() => onClicked?.Invoke());
    }

    public void SetSelected(bool selected)
    {
        selectedVisual.SetActive(selected);
        unSelectedVisual.SetActive(!selected);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}
