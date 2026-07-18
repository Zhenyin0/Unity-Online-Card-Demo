using System;
using UnityEngine;

public class MouseClickRing : MonoBehaviour
{
    private static MouseClickRing _instance;
    private AudioSource _audioSource;

    void Awake()
    {
        
        // 单例模式：确保全局只有一个，且不被销毁
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // 如果已经有了一个，就把新来的干掉
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject); // 核心：切换场景不销毁

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>(); // 保险起见，没组件就自动加一个
    }
    

    void Update()
    {
        // 监听左键点击
        if (Input.GetMouseButtonDown(0))
        {
            PlaySound();
        }
        // 监听右键点击
        if (Input.GetMouseButtonDown(1))
        {
            PlaySound();
        }
    }

    private void PlaySound()
    {
        if (_audioSource.isActiveAndEnabled)
        {
            _audioSource.Play();
        }
    }
}