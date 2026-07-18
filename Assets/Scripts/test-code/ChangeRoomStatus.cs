using UnityEngine;
using UnityEngine.UI;

public class ChangeRoomStatus : MonoBehaviour
{
    [Header("步骤1：等待玩家进入面板")]
    public GameObject waitPlayerEnterPanel;
    public Button waitPlayerNextBtn;

    [Header("步骤2：模式选择面板")]
    public GameObject selectModulePanel;
    public Button superBattleBtn;
    public Button ultimateBattleBtn;
    public Button nomalBattleBtn;

    [Header("步骤3：选卡面板")]
    public GameObject cardPathPanel;
    public Button selectCardOverBtn;
    public Button selectCardCancelBtn;

    [Header("步骤4：等待进房面板")]
    public GameObject waitRoomEnterPanel;
    public Button waitRoomCancleBtn;

    void Start()
    {
        // 绑定步骤1：等待玩家进入 → 下一步
        if (waitPlayerNextBtn != null)
            waitPlayerNextBtn.onClick.AddListener(OnWaitPlayerNext);

        // 绑定步骤2：三个模式三选一，共用同一个逻辑方法
        if (superBattleBtn != null)
            superBattleBtn.onClick.AddListener(() => OnSelectMode(superBattleBtn.gameObject, ultimateBattleBtn.gameObject, nomalBattleBtn.gameObject));
        if (ultimateBattleBtn != null)
            ultimateBattleBtn.onClick.AddListener(() => OnSelectMode(ultimateBattleBtn.gameObject, superBattleBtn.gameObject, nomalBattleBtn.gameObject));
        if (nomalBattleBtn != null)
            nomalBattleBtn.onClick.AddListener(() => OnSelectMode(nomalBattleBtn.gameObject, superBattleBtn.gameObject, ultimateBattleBtn.gameObject));

        // 绑定步骤3：选卡确认/取消 → 进入等待进房
        if (selectCardOverBtn != null)
            selectCardOverBtn.onClick.AddListener(OnCardFinish);
        if (selectCardCancelBtn != null)
            selectCardCancelBtn.onClick.AddListener(OnCardFinish);

        // 绑定步骤4：等待进房 → 取消返回选卡
        if (waitRoomCancleBtn != null)
            waitRoomCancleBtn.onClick.AddListener(OnWaitRoomCancle);
    }

    /// <summary>
    /// 步骤1：点击下一步，关闭等待面板，打开模式选择面板
    /// </summary>
    void OnWaitPlayerNext()
    {
        if (waitPlayerEnterPanel != null)
            waitPlayerEnterPanel.SetActive(false);
        if (selectModulePanel != null)
            selectModulePanel.SetActive(true);
    }

    /// <summary>
    /// 步骤2：模式三选一逻辑，保留选中项，隐藏另外两项，同时打开选卡面板
    /// </summary>
    /// <param name="keepActive">要保留激活的选项</param>
    /// <param name="hide1">要隐藏的选项1</param>
    /// <param name="hide2">要隐藏的选项2</param>
    void OnSelectMode(GameObject keepActive, GameObject hide1, GameObject hide2)
    {
        if (keepActive != null) keepActive.SetActive(true);
        if (hide1 != null) hide1.SetActive(false);
        if (hide2 != null) hide2.SetActive(false);

        if (cardPathPanel != null)
            cardPathPanel.SetActive(true);
    }

    /// <summary>
    /// 步骤3：选卡结束（确认/取消共用），关闭选卡面板，打开等待进房面板
    /// </summary>
    void OnCardFinish()
    {
        if (cardPathPanel != null)
            cardPathPanel.SetActive(false);
        if (waitRoomEnterPanel != null)
            waitRoomEnterPanel.SetActive(true);
    }

    /// <summary>
    /// 步骤4：等待进房点取消，关闭等待面板，返回选卡面板
    /// </summary>
    void OnWaitRoomCancle()
    {
        if (waitRoomEnterPanel != null)
            waitRoomEnterPanel.SetActive(false);
        if (cardPathPanel != null)
            cardPathPanel.SetActive(true);
    }

    // 脚本销毁时移除所有按钮监听，避免内存泄漏
    private void OnDestroy()
    {
        if (waitPlayerNextBtn != null)
            waitPlayerNextBtn.onClick.RemoveListener(OnWaitPlayerNext);

        if (superBattleBtn != null) superBattleBtn.onClick.RemoveAllListeners();
        if (ultimateBattleBtn != null) ultimateBattleBtn.onClick.RemoveAllListeners();
        if (nomalBattleBtn != null) nomalBattleBtn.onClick.RemoveAllListeners();

        if (selectCardOverBtn != null)
            selectCardOverBtn.onClick.RemoveListener(OnCardFinish);
        if (selectCardCancelBtn != null)
            selectCardCancelBtn.onClick.RemoveListener(OnCardFinish);

        if (waitRoomCancleBtn != null)
            waitRoomCancleBtn.onClick.RemoveListener(OnWaitRoomCancle);
    }
}
