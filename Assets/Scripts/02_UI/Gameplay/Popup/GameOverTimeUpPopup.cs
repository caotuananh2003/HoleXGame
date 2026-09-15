using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameOverTimeUpPopup : UIWindow
{
    private const int   MaxRevives         = 3;
    private const float AddSeconds         = 20f;
    private const int   ReviveCurrencyCost = 900;

    [Header("Sequence")]
    [SerializeField] private GameOverTimeUpSequencePlayer sequencePlayer;

    [Header("Buttons")]
    [SerializeField] private Button adsBonusButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    [Header("Clock Indicators")]
    [SerializeField] private ClockIndicator clock1;
    [SerializeField] private ClockIndicator clock2;
    [SerializeField] private ClockIndicator clock3;

    private int reviveCount;

    private void Start()
    {
        SetButtonsInteractable(false);
        adsBonusButton.onClick.AddListener(OnAdsBonusClicked);
        resumeButton.onClick.AddListener(OnResumeClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDestroy()
    {
        adsBonusButton.onClick.RemoveListener(OnAdsBonusClicked);
        resumeButton.onClick.RemoveListener(OnResumeClicked);
        quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    public override void Open()
    {
        base.Open();
        reviveCount = 0;
        UIManager?.PlaySFX(AudioID.SFX.UiLose);
        SetButtonsInteractable(false);
        PlaySequenceAsync().Forget();
    }

    private async UniTaskVoid PlaySequenceAsync()
    {
        await sequencePlayer.PlayAsync();
        await PlayButtonsAndEnableAsync();
    }

    private async UniTask PlayButtonsAndEnableAsync()
    {
        await sequencePlayer.PlayButtonsAsync();
        SetButtonsInteractable(true);
        RefreshReviveButtons();
    }

    private void SetButtonsInteractable(bool v)
    {
        adsBonusButton.interactable = v;
        resumeButton.interactable   = v;
        quitButton.interactable     = v;
    }

    private void RefreshReviveButtons()
    {
        bool canRevive = reviveCount < MaxRevives;
        adsBonusButton.gameObject.SetActive(canRevive);
        resumeButton.gameObject.SetActive(canRevive);
    }

    private void OnAdsBonusClicked() => Revive();

    private void OnResumeClicked()
    {
        if (SaveManager.Instance?.PlayerData == null) { Debug.LogWarning("[GameOverTimeUpPopup] PlayerData is null."); return; }
        if (SaveManager.Instance.PlayerData.currency < ReviveCurrencyCost) { Debug.Log($"[GameOverTimeUpPopup] Không đủ {ReviveCurrencyCost} currency."); return; }

        SaveManager.Instance.PlayerData.currency -= ReviveCurrencyCost;
        SaveManager.Instance.Save().Forget();
        Revive();
    }

    private void OnQuitClicked()
    {
        UIManager?.Close<GameOverTimeUpPopup>();
        UIManager?.Open<TryAgainPopup>();
    }

    private void Revive()
    {
        if (reviveCount >= MaxRevives) return;
        reviveCount++;
        MarkClockUsed(reviveCount);

        var gameTimer = FindAnyObjectByType<GameTimer>(FindObjectsInactive.Include);
        gameTimer?.AddTime(AddSeconds);

        var holeController = FindAnyObjectByType<HoleController>(FindObjectsInactive.Include);
        holeController?.SetInputEnabled(true);

        GameManager.Instance?.ChangeState(GameState.Gameplay);
        UIManager?.Close<GameOverTimeUpPopup>();
    }

    private void MarkClockUsed(int revive)
    {
        switch (revive)
        {
            case 1: clock1?.SetUsed(true); break;
            case 2: clock2?.SetUsed(true); break;
            case 3: clock3?.SetUsed(true); break;
        }
    }
}
