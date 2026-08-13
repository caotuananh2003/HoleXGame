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
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(Action onClicked)
    {
        button.onClick.AddListener(() => onClicked?.Invoke());
        //Debug.Log($"{name} : Initialize");
    }

    public void SetSelected(bool selected)
    {
        if (selectedVisual != null)
        {
            selectedVisual.SetActive(selected);
        }
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}
