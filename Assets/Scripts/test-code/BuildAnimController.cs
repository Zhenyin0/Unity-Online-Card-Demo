using UnityEngine;

public class BuildAnimController : MonoBehaviour
{
    public float playDuration = 60f;
    //public float BagItemtime = 61f;
    public GameObject itemBackground;
    private bool _isDone;

    void Start()
    {
        ResetAll();
    }

    void OnAnimEnd()
    {
        if (_isDone) return;
        _isDone = true;

        gameObject.SetActive(false);
        if (itemBackground != null)
        {
            itemBackground.SetActive(true);
  
        }
    }

    // 清空所有计时+重置状态
    void ResetAll()
    {
        CancelInvoke();
        _isDone = false;
        gameObject.SetActive(true);
        Invoke(nameof(OnAnimEnd), playDuration);
        
        
    }

    // 收集后重置
    public void ResetAnim()
    {
        if (_isDone)
        {
            // ========== 追加：触发建筑产出结算 ==========
            BagItemForMapInteract bagInteract = FindObjectOfType<BagItemForMapInteract>();
            if (bagInteract != null)
            {
                bagInteract.SettleBuildProduct(gameObject);
            }
            // ========== 追加结束 ==========
        }

    if (itemBackground != null)
            itemBackground.SetActive(false);
        ResetAll();
    }
    /// <summary>
    /// 供预览建筑转正时调用，手动启动生产计时
    /// </summary>
    public void StartProduction()
    {
        ResetAll();
    }
}