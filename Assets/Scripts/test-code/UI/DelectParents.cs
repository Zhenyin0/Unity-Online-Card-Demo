using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 必须引入UI命名空间才能使用Button

public class DelectParents : MonoBehaviour
{
    private Button _targetBtn;

    void Start()
    {
        // 获取自身挂载的Button组件
        _targetBtn = GetComponent<Button>();

        // 防止空引用报错（物体没加Button组件时提示）
        if (_targetBtn == null)
        {
            Debug.LogError("当前物体未挂载Button组件！脚本无法生效", gameObject);
            return;
        }

        // 代码自动绑定点击事件
        _targetBtn.onClick.AddListener(DestroyParent);
    }

    /// <summary>
    /// 销毁父物体，自身与所有兄弟一并删除
    /// </summary>
    void DestroyParent()
    {
        // 判断是否存在父物体，避免空报错
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Debug.LogWarning("该物体没有父物体，无需销毁", gameObject);
        }
    }

    // 销毁时移除监听，防止内存泄漏
    private void OnDestroy()
    {
        if (_targetBtn != null)
        {
            _targetBtn.onClick.RemoveListener(DestroyParent);
        }
    }
}
