using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Apply gravity physics lên victims, xử lý object rơi vào/qua lỗ, fire score event.
/// Dùng HoleSizeController.Scale để tính lực kéo.
/// </summary>
[RequireComponent(typeof(HoleSizeController))]
public class SwallowHandler : MonoBehaviour
{
    private const int SwallowingLayer = 10;

    [SerializeField] private float swallowGravity = 10f;

    public event Action<int> OnObjectSwallowed;

    private List<Rigidbody>    victims;
    private float scale = 1f;

    public void SetScale(float newScale)
    {
        scale = newScale;
    }

    // Gọi từ HoleSizeController.Awake() để khởi tạo victims list (Dùng chung, cùng trỏ tới 1 List<Rigidbody> trong bộ nhớ)
    public void Initialize(List<Rigidbody> sharedVictims, float initialScale)
    {
        victims = sharedVictims;
        scale = initialScale;
    }

    private void FixedUpdate()
    {
        if (victims == null)
            return;

        for (int i = victims.Count - 1; i >= 0; i--) // Chạy ngược lại để khi remove phần tử, không cần thao tác cập nhật lại i
        {
            if (victims[i] == null)
            {
                victims.RemoveAt(i);
                continue;
            }

            Debug.Log($"Removing {victims[i].gameObject.name}");
            victims[i].gameObject.SetActive(false);
            victims.RemoveAt(i);

            //victims[i].AddForce(
            //    Vector3.down * scale * swallowGravity * Time.fixedDeltaTime,
            //    ForceMode.VelocityChange);

            //// Ví dụ: nếu object đã đi qua lỗ thì disable
            //if (victims[i].position.y < -5f)
            //{
            //    victims[i].gameObject.SetActive(false);
            //    victims.RemoveAt(i);
            //}
        }
    }

    public void HandleTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == SwallowingLayer)
            OnObjectSwallowed?.Invoke(1);
    }

    public void HandleTriggerExit(Collider other)
    {
        if (other.transform.position.y >= 0f) return;

        other.gameObject.SetActive(false);
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) victims.Remove(rb);
    }
}
