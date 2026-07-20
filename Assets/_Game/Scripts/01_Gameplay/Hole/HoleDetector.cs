using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chịu trách nhiệm phát hiện các object có thể bị nuốt (swallowable)
/// nằm trong bán kính hiện tại của hole.
///
/// Hoạt động:
/// 1. Mỗi frame quét Physics.OverlapSphere() với bán kính = HoleSizeController.Radius.
/// 2. Chỉ xử lý các object đang ở layer "Swallowable".
/// 3. Đưa Rigidbody của object vào shared victims list.
/// 4. Chuyển object sang layer "Swallowing" để không bị thêm nhiều lần.
/// 5. SwallowHandler sẽ đọc victims list này để hút object về tâm hole.
///
/// Layer sử dụng:
/// - Layer 9  (Swallowable): Object bình thường, có thể bị hole phát hiện.
/// - Layer 10 (Swallowing): Object đã được phát hiện và đang trong quá trình bị hút.
///                           Layer này giúp tránh thêm cùng một object nhiều lần.
///
/// Không trực tiếp hút, di chuyển hay destroy object.
/// Chỉ chịu trách nhiệm phát hiện và đăng ký object vào victims list.
/// </summary>
[RequireComponent(typeof(HoleSizeController))]
public class HoleDetector : MonoBehaviour
{
    private const int SwallowableLayer = 9;  // Layer Swallowable
    private const int SwallowingLayer  = 10; // Layer Swallowing

    private List<Rigidbody>   victims; // List rigidbody của victims
    private float radius;
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
    }

    // Gọi từ HoleSizeController.Awake() để khởi tạo victims list (Dùng chung, cùng trỏ tới 1 List<Rigidbody> trong bộ nhớ)
    public void Initialize(List<Rigidbody> sharedVictims, float initialRadius)
    {
        victims = sharedVictims;
        radius = initialRadius;
    }

    private void Update()
    {
        if (victims == null) return;

        // Array các Collider trong bán kính radius
        Collider[] nearby = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider col in nearby)
        {
            // Bỏ qua chính collider của gameobject đang gắn HoleDetector (player)
            if (col.gameObject == gameObject) continue;
            // Bỏ qua các collider không phải Swallowable
            if (col.gameObject.layer != SwallowableLayer) continue;

            Rigidbody rb = col.GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                Debug.Log("Swallowable Object has a null rugidbody");
                continue;
            }

            if (victims.Contains(rb)) continue;

            victims.Add(rb);
            col.gameObject.layer = SwallowingLayer;
            //rb.isKinematic       = false;
        }
    }
}
