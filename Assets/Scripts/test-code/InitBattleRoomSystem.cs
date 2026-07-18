using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitBattleRoomSystem : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        // 所有Awake全部执行完后，再初始化子系统，100%不会出现单例为空
        InitSubSystems();
    }

    #region ===== 子系统初始化控制 =====
    private void InitSubSystems()
    {
        // // // 按你设计的顺序调用（示例顺序，需匹配你的业务）
        // // RoomRoundStatusSys.Instance.Init();
        // // MainCharAndCardSys.Instance.Init();
        // // CharAnimSystem.Instance.Init();
        // // RoomUIPickSystem.Instance.Init();
        // if (RoomRoundStatusSys.Instance == null)
        //     Debug.LogWarning("RoomRoundStatusSys.Instance 为空，场景里可能没挂这个脚本");
        // else
        //     RoomRoundStatusSys.Instance.Init();
        //
        // if (MainCharAndCardSys.Instance == null)
        //     Debug.LogWarning("MainCharAndCardSys.Instance 为空，场景里可能没挂这个脚本");
        // else
        //     MainCharAndCardSys.Instance.Init();
        //
        // if (CharAnimSystem.Instance == null)
        //     Debug.LogWarning("CharAnimSystem.Instance 为空，场景里可能没挂这个脚本");
        // else
        //     CharAnimSystem.Instance.Init();

        if (RoomUIPickSystem.Instance == null)
            Debug.LogWarning("RoomUIPickSystem.Instance 为空，场景里可能没挂这个脚本");
        else
            RoomUIPickSystem.Instance.Init();
    }
    #endregion
}
