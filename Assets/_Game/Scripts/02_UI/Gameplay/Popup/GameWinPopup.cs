using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameWinPopup : UIWindow
{
    [Header("Sequence")]
    [SerializeField] private WinSequencePlayer winSequencePlayer;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button watchAdsButton;

    [Header("Reward Display")]
    [SerializeField] private TMP_Text currencyRewardText;

    private int currencyReward;

    private void Start()
    {
        Validate();
        SetButtonsInteractable(false);
        if (resumeButton   != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (watchAdsButton != null) watchAdsButton.onClick.AddListener(OnWatchAdsClicked);
    }

    private void OnDestroy()
    {
        if (resumeButton   != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
        if (watchAdsButton != null) watchAdsButton.onClick.RemoveListener(OnWatchAdsClicked);
        winSequencePlayer?.Cleanup();
    }

    public override void Open()
    {
        base.Open();
        UIManager?.PlaySFX(AudioID.SFX.UiWin);
        SetButtonsInteractable(false);
        PlaySequenceAsync().Forget();
    }

    public override void Close()
    {
        winSequencePlayer?.Cleanup();
        base.Close();
    }

    public void Setup(int reward)
    {
        currencyReward = reward;
        if (currencyRewardText != null) currencyRewardText.text = $"x{reward}";
    }

    private async UniTaskVoid PlaySequenceAsync()
    {
        if (winSequencePlayer != null) await winSequencePlayer.PlayAsync();
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool v)
    {
        if (resumeButton   != null) resumeButton.interactable   = v;
        if (watchAdsButton != null) watchAdsButton.interactable = v;
    }

    private void OnResumeClicked()
    {
        AddCurrency(currencyReward);
        UIManager?.Close<GameWinPopup>();
        TransitionService.Instance?.TransitionToMainMenuAsync().Forget();
    }

    private void OnWatchAdsClicked()
    {
        AddCurrency(currencyReward * 2);
        UIManager?.Close<GameWinPopup>();
        TransitionService.Instance?.TransitionToMainMenuAsync().Forget();
    }

    private void AddCurrency(int amount)
    {
        if (SaveManager.Instance?.PlayerData == null) { Debug.LogError("[GameWinPopup] PlayerData is null."); return; }
        SaveManager.Instance.PlayerData.currency += amount;
        SaveManager.Instance.Save().Forget();
    }

    private void Validate()
    {
        if (winSequencePlayer  == null) Debug.LogError("[GameWinPopup] winSequencePlayer is not assigned.",  this);
        if (resumeButton       == null) Debug.LogError("[GameWinPopup] resumeButton is not assigned.",       this);
        if (watchAdsButton     == null) Debug.LogError("[GameWinPopup] watchAdsButton is not assigned.",     this);
        if (currencyRewardText == null) Debug.LogError("[GameWinPopup] currencyRewardText is not assigned.", this);
    }
}
