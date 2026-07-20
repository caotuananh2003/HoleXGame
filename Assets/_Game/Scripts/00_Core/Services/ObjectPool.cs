using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool cho bất kỳ Component nào.
/// Tạo mới khi pool trống, tự expand không giới hạn.
/// Không dùng FindObjectOfType — factory delegate do caller cung cấp.
/// </summary>
public sealed class ObjectPool<T> where T : Component
{
    private readonly Stack<T>    available = new();
    private readonly List<T>     all       = new();
    private readonly Func<T>     factory;
    private readonly Transform   poolRoot;

    public int CountAvailable => available.Count;
    public int CountAll       => all.Count;

    /// <param name="factory">Hàm tạo instance mới.</param>
    /// <param name="poolRoot">Parent transform để giữ inactive objects gọn gàng trong hierarchy.</param>
    /// <param name="prewarm">Số instance tạo sẵn lúc khởi tạo.</param>
    public ObjectPool(Func<T> factory, Transform poolRoot, int prewarm = 0)
    {
        this.factory  = factory  ?? throw new ArgumentNullException(nameof(factory));
        this.poolRoot = poolRoot;

        for (int i = 0; i < prewarm; i++)
            Return(CreateNew());
    }

    /// <summary>Lấy một instance từ pool. Tự tạo mới nếu pool trống.</summary>
    public T Get()
    {
        T item = available.Count > 0 ? available.Pop() : CreateNew();
        item.gameObject.SetActive(true);
        return item;
    }

    /// <summary>Trả instance về pool. SetActive(false) và reparent về poolRoot.</summary>
    public void Return(T item)
    {
        if (item == null) return;
        item.gameObject.SetActive(false);
        item.transform.SetParent(poolRoot, false);
        available.Push(item);
    }

    /// <summary>Trả tất cả instance đang active về pool.</summary>
    public void ReturnAll()
    {
        foreach (T item in all)
        {
            if (item != null && item.gameObject.activeSelf)
                Return(item);
        }
    }

    private T CreateNew()
    {
        T item = factory();
        item.gameObject.SetActive(false);
        if (poolRoot != null)
            item.transform.SetParent(poolRoot, false);
        all.Add(item);
        return item;
    }
}
