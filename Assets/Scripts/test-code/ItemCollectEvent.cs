using UnityEngine;

public class ItemCollectEvent : MonoBehaviour
{
    public BuildAnimController buildAnimCtrl; // 后面拖入people-buildAnim的脚本
    public void OnCollectDone()
    {
        // 隐藏自己
        //gameObject.SetActive(false);
        // 重置动画流程，开始下一轮计时
        if (buildAnimCtrl != null)
            buildAnimCtrl.ResetAnim();
    }
}