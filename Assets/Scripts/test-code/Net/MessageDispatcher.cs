using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class MessageDispatcher : MonoBehaviour
{
    public static MessageDispatcher Instance;
    
    private readonly Dictionary<int, Action<byte[]>> msgHandlers = new Dictionary<int, Action<byte[]>>();
    private readonly object lockObj = new object();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 注册消息回调
    /// </summary>
    public void Register(int packId, Action<byte[]> handler)
    {
        lock (lockObj)
        {
            if (!msgHandlers.ContainsKey(packId))
                msgHandlers[packId] = handler;
            else
                msgHandlers[packId] += handler;
        }
    }

    /// <summary>
    /// 注销消息回调
    /// </summary>
    public void Unregister(int packId, Action<byte[]> handler)
    {
        lock (lockObj)
        {
            if (msgHandlers.ContainsKey(packId))
            {
                msgHandlers[packId] -= handler;
                if (msgHandlers[packId] == null)
                    msgHandlers.Remove(packId);
            }
        }
    }

    /// <summary>
    /// 分发消息（自动切换到Unity主线程）
    /// </summary>
    public async void Dispatch(int packId, byte[] body)
    {
        // 切到主线程再执行回调
        await UniTask.SwitchToMainThread();
        
        Action<byte[]> handler;
        lock (lockObj)
        {
            msgHandlers.TryGetValue(packId, out handler);
        }
        
        handler?.Invoke(body);
    }
}
