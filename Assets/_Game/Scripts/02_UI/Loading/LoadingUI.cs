using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private float smoothSpeed = 2f; // Tốc độ slider đuổi theo tiến độ thật

    private float targetProgress = 0f;
    private float currentProgress = 0f;

    // Thuộc tính để Bootstrap kiểm tra xem UI đã chạy mượt xong chưa
    public bool IsVisualComplete => currentProgress >= 0.99f;

    // Bootstrap chỉ cần gọi hàm này để cập nhật ĐÍCH ĐẾN, không cần await
    public void UpdateTargetProgress(float value)
    {
        targetProgress = value;
    }

    private void Update()
    {
        if (currentProgress < targetProgress)
        {
            // Nội suy mượt mà giá trị hiện tại tiến dần về giá trị đích mỗi khung hình
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, smoothSpeed * Time.deltaTime);

            // Cập nhật lên màn hình
            progressBar.value = currentProgress;
            progressText.text = $"{currentProgress * 100f:0}%";
        }
    }
}