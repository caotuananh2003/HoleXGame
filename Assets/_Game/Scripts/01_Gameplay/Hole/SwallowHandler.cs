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
    //private HoleSizeController sizeController;

    private void Awake()
    {
        //sizeController = GetComponent<HoleSizeController>();
    }

    /// <summary>Gọi từ HoleSizeController.Awake() để inject shared victims list.</summary>
    public void Initialize(List<Rigidbody> sharedVictims, float initialScale)
    {
        victims = sharedVictims;
        scale = initialScale;
    }

    private void FixedUpdate()
    {
        if (victims == null)
            return;

        for (int i = victims.Count - 1; i >= 0; i--)
        {
            if (victims[i] == null)
            {
                victims.RemoveAt(i);
                continue;
            }

            victims[i].AddForce(
                Vector3.down * scale * swallowGravity * Time.fixedDeltaTime,
                ForceMode.VelocityChange);
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
