using UnityEngine;
using UnityEngine.UI;

public class TryAgainPopup : PopupWindow
{
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        if (closeButton    != null) closeButton.onClick.AddListener(OnCloseClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (tryAgainButton != null) tryAgainButton.onClick.RemoveListener(OnTryAgainClicked);
        if (closeButton    != null) closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnTryAgainClicked()
    {
        UIManager?.Close<TryAgainPopup>();
        GameplayController.Instance?.RestartLevel();
    }

    private void OnCloseClicked()
    {
        UIManager?.Close<TryAgainPopup>();
        TransitionService.Instance?.TransitionToMainMenuAsync().Forget();
    }
}
