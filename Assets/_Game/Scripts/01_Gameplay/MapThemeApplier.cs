using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapThemeApplier : MonoBehaviour
{
    [SerializeField] private PlayerProfile playerProfile;

    private void Start()
    {
        if (playerProfile == null) { Debug.LogWarning("[MapThemeApplier] playerProfile is not assigned."); return; }
        if (SaveManager.Instance?.PlayerData == null) { Debug.LogWarning("[MapThemeApplier] PlayerData is null."); return; }

        ResolveDefaultIfNeeded();
        // TODO: ApplyCurrentTheme()
    }

    private void ResolveDefaultIfNeeded()
    {
        if (!string.IsNullOrEmpty(SaveManager.Instance.PlayerData.equippedMapThemeId)) return;

        MapThemeDatabase db = playerProfile.MapThemeDatabase;
        if (db == null || db.MapThemeDefinition.Count == 0) { Debug.LogWarning("[MapThemeApplier] MapThemeDatabase rỗng."); return; }

        string defaultId = db.MapThemeDefinition[0].Id;
        SaveManager.Instance.PlayerData.equippedMapThemeId = defaultId;
        SaveManager.Instance.Save().Forget();
    }
}
