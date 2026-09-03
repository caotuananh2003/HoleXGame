using Cysharp.Threading.Tasks;
using UnityEngine;

public class HoleSkinApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer holeSkinRenderer;
    [SerializeField] private PlayerProfile  playerProfile;
    public static HoleSkinApplier Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Apply();
    }

    /// <summary>
    /// Áp dụng skin hiện tại từ save data.
    /// Gọi từ Start() hoặc từ GameplayController.StartLevel() sau khi save đã load xong.
    /// </summary>
    public void Apply()
    {
        if (holeSkinRenderer == null || playerProfile == null) return;

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager.Instance is null");
            return;
        }

        if (SaveManager.Instance?.PlayerData == null)
        {
            Debug.LogWarning("[HoleSkinApplier] PlayerData is null — skin sẽ được áp dụng khi SaveManager sẵn sàng.");
            return;
        }

        ResolveDefaultIfNeeded();
        ApplyCurrentSkin();
    }

    private void ResolveDefaultIfNeeded()
    {
        if (!string.IsNullOrEmpty(SaveManager.Instance.PlayerData.equippedHoleSkinId)) return;

        HoleSkinDatabase db = playerProfile.HoleSkinDatabase;
        if (db == null || db.HoleDefinition.Count == 0) { Debug.LogWarning("[HoleSkinApplier] HoleSkinDatabase rỗng."); return; }

        string defaultId = db.HoleDefinition[0].Id;
        SaveManager.Instance.PlayerData.equippedHoleSkinId = defaultId;
        SaveManager.Instance.Save().Forget();
    }

    private void ApplyCurrentSkin()
    {
        string id = SaveManager.Instance.PlayerData.equippedHoleSkinId;
        if (string.IsNullOrEmpty(id)) return;

        HoleSkinDefinition def = playerProfile.HoleSkinDatabase?.GetById(id);
        if (def == null) { Debug.LogWarning($"[HoleSkinApplier] Không tìm thấy id='{id}'."); return; }

        holeSkinRenderer.sprite = def.Icon;
    }
}
