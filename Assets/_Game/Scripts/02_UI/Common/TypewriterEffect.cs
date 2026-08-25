using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Hiệu ứng đánh máy cho TMP_Text — reveal từng ký tự từ trái qua phải.
/// Dùng TMP.maxVisibleCharacters để không ảnh hưởng layout.
///
/// Gắn vào cùng GameObject với TMP_Text, hoặc dùng PlayAsync(tmp) trực tiếp.
///
/// Cách dùng từ code khác:
///   await typewriterEffect.PlayAsync();
///   await TypewriterEffect.PlayAsync(tmpText, charsPerSecond);
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text    targetText;
    [SerializeField] private float       charsPerSecond = 30f;  // tốc độ reveal
    [SerializeField] private AudioSource typingSound;           // optional — tiếng gõ phím

    // =========================================================================
    // Instance API — dùng khi component gắn sẵn trên GameObject
    // =========================================================================

    /// <summary>Play hiệu ứng đánh máy trên targetText được assign trong Inspector.</summary>
    public UniTask PlayAsync()
    {
        if (targetText == null)
        {
            Debug.LogWarning("[TypewriterEffect] targetText is not assigned.", this);
            return UniTask.CompletedTask;
        }

        return PlayAsync(targetText, charsPerSecond, typingSound, destroyCancellationToken);
    }

    // =========================================================================
    // Static API — dùng không cần gắn component
    // =========================================================================

    /// <summary>
    /// Play hiệu ứng đánh máy trên bất kỳ TMP_Text nào.
    /// Await cho đến khi toàn bộ text đã hiện.
    /// </summary>
    public static async UniTask PlayAsync(
        TMP_Text          tmp,
        float             charsPerSecond = 30f,
        AudioSource       sound          = null,
        System.Threading.CancellationToken cancellationToken = default)
    {
        if (tmp == null) return;

        // Force mesh update để TMP biết tổng số ký tự
        tmp.ForceMeshUpdate();

        int totalChars = tmp.textInfo.characterCount;

        if (totalChars == 0) return;

        tmp.maxVisibleCharacters = 0;

        float interval = 1f / Mathf.Max(1f, charsPerSecond);

        for (int i = 0; i < totalChars; i++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            tmp.maxVisibleCharacters = i + 1;

            if (sound != null && sound.clip != null)
                sound.Play();

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(interval),
                ignoreTimeScale: true,
                cancellationToken: cancellationToken
            );
        }

        // Đảm bảo hiện hết nếu bị cancel hoặc delay nhỏ
        tmp.maxVisibleCharacters = totalChars;
    }

    /// <summary>Hiện toàn bộ text tức thì, không animation.</summary>
    public static void ShowImmediate(TMP_Text tmp)
    {
        if (tmp == null) return;

        tmp.ForceMeshUpdate();
        tmp.maxVisibleCharacters = tmp.textInfo.characterCount;
    }
}
