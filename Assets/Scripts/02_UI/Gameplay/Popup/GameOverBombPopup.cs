using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameOverBombPopup : UIWindow
{
    private const int RebornCost = 900;

    [Header("Sequence")]
    [SerializeField] private GameOverBombSequencePlayer sequencePlayer;
    [SerializeField] private GameObject phase1Root;
    [SerializeField] private GameObject phase2Root;

    [Header("Buttons")]
    [SerializeField] private Button rebornButton;
    [SerializeField] private Button quitButton;

    private int quitClickCount;

    private void Start()
    {
        SetButtonsInteractable(false);
        if (rebornButton != null) rebornButton.onClick.AddListener(OnRebornClicked);
        if (quitButton   != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDestroy()
    {
        if (rebornButton != null) rebornButton.onClick.RemoveListener(OnRebornClicked);
        if (quitButton   != null) quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    public override void Open()
    {
        base.Open();
        quitClickCount = 0;
        UIManager?.PlaySFX(AudioID.SFX.UiLose);
        SetButtonsInteractable(false);
        PlaySequenceAsync().Forget();
    }

    private async UniTaskVoid PlaySequenceAsync()
    {
        if (sequencePlayer != null) await sequencePlayer.PlayAsync();
        await PlayButtonsAndEnableAsync();
    }

    private async UniTaskVoid PlayPhase2Async()
    {
        if (phase1Root != null) phase1Root.SetActive(false);
        if (sequencePlayer != null) await sequencePlayer.PlayPhase2Async();
        await PlayButtonsAndEnableAsync();
    }

    private async UniTask PlayButtonsAndEnableAsync()
    {
        if (sequencePlayer != null) await sequencePlayer.PlayButtonsAsync();
        SetButtonsInteractable(true);
        RefreshRebornButtonState();
    }

    private void SetButtonsInteractable(bool v)
    {
        if (rebornButton != null) rebornButton.interactable = v;
        if (quitButton   != null) quitButton.interactable   = v;
    }

    private void RefreshRebornButtonState()
    {
        if (rebornButton == null) return;
        rebornButton.interactable = SaveManager.Instance?.PlayerData != null
                                 && SaveManager.Instance.PlayerData.currency >= RebornCost;
    }

    private void OnQuitClicked()
    {
        quitClickCount++;
        if (quitClickCount == 1)
        {
            SetButtonsInteractable(false);
            PlayPhase2Async().Forget();
        }
        else
        {
            UIManager?.Close<GameOverBombPopup>();
            UIManager?.Open<TryAgainPopup>();
        }
    }

    private void OnRebornClicked()
    {
        if (SaveManager.Instance?.PlayerData == null) { Debug.LogError("[GameOverBombPopup] PlayerData is null."); return; }
        if (SaveManager.Instance.PlayerData.currency < RebornCost) { Debug.LogWarning("[GameOverBombPopup] Không đủ vàng."); return; }

        SaveManager.Instance.PlayerData.currency -= RebornCost;
        SaveManager.Instance.Save().Forget();

        UIManager?.Close<GameOverBombPopup>();
        GameplayController.Instance?.RebornPlayer();
    }
}
