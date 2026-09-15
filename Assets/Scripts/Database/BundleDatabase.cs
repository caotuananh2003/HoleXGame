using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Database chứa tất cả BundleDefinition.
/// Tạo asset: Database → Bundle Database.
/// Gán vào PlayerProfile.BundleDatabase trong Inspector.
/// </summary>
[CreateAssetMenu(fileName = "BundleDatabase", menuName = "Database/Bundle Database")]
public class BundleDatabase : ScriptableObject
{
    [SerializeField] private List<BundleDefinition> bundles = new();

    public IReadOnlyList<BundleDefinition> Bundles => bundles;

    /// <summary>Tìm BundleDefinition theo id. Trả null nếu không tìm thấy.</summary>
    public BundleDefinition GetById(string id)
    {
        return bundles.Find(x => x.Id == id);
    }
}
