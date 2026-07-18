using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSwitchSystem : MonoBehaviour
{
    // 【新增】缓存EnterFightRoomPath实例和面板原始位置
    private EnterFightRoomPath _enterFightRoomPath;
    private Vector3 _originPanelPos; 

    [Header("拖入对应按钮")]
    public Button switchLeftBtn;
    public Button switchRightBtn;

    [Header("地图设置")]
    public float moveSpeed = 2000f;
    private const float MAP_WIDTH = 1920f;

    [Header("拖入4张地图")]
    public RectTransform Map1;
    public RectTransform Map2;
    public RectTransform Map3;
    public RectTransform Map4;
    
    [Header("资源跳转后的转接")]
    public Text manaText;                 // 魔力值显示文本组件
    public RectTransform bagItemRoot;     // 背包物品根节点（BagItemPath）
    
    private List<RectTransform> mapList = new List<RectTransform>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private int currentIndex = 0;
    private bool isMoving = false;

    void Awake()
    {
        // 初始化地图列表
        mapList.Add(Map1);
        mapList.Add(Map2);
        mapList.Add(Map3);
        mapList.Add(Map4);

        // 强制初始化所有地图位置
        for (int i = 0; i < mapList.Count; i++)
        {
            Vector3 pos = new Vector3(MAP_WIDTH * i, 0, 0);
            mapList[i].anchoredPosition = pos;
            targetPositions.Add(pos);
            mapList[i].gameObject.SetActive(i == 0);
        }

        // 绑定按钮事件（不用在面板点了，代码自动绑）
        switchLeftBtn.onClick.AddListener(SwitchLeft);
        switchRightBtn.onClick.AddListener(SwitchRight);

        // 【新增】初始化EnterFightRoomPath实例
        _enterFightRoomPath = FindObjectOfType<EnterFightRoomPath>();
        
        // 更新按钮状态
        UpdateButtonState();
        
    }

    void Start()
    {
         // 刷新建造系统
         RefreshBuildManager();
    }

    void Update()
    {
        if (!isMoving) return;

        bool allArrived = true;
        for (int i = 0; i < mapList.Count; i++)
        {
            mapList[i].anchoredPosition = Vector3.MoveTowards(
                mapList[i].anchoredPosition,
                targetPositions[i],
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(mapList[i].anchoredPosition, targetPositions[i]) > 0.1f)
            {
                allArrived = false;
            }
        }

        if (allArrived)
        {
            isMoving = false;
            // 只保留当前地图激活
            for (int i = 0; i < mapList.Count; i++)
            {
                mapList[i].gameObject.SetActive(i == currentIndex);
            }
            // 【新增】地图移动完成后恢复面板
            RestorePanelAfterMove();
        }
    }

    public void SwitchRight()
    {
        if (currentIndex >= mapList.Count - 1 || isMoving) return;

        currentIndex++;
        // 全量激活所有地图
        foreach (var map in mapList)
        {
            map.gameObject.SetActive(true);
        }
        // 所有地图左移1920
        for (int i = 0; i < targetPositions.Count; i++)
        {
            targetPositions[i] -= new Vector3(MAP_WIDTH, 0, 0);
        }

        isMoving = true;
        UpdateButtonState();
    
        // 【新增】先处理面板 → 再刷新数据
        HandlePanelBeforeRefresh();
        RefreshBuildManager();
        
        // 【新增】通知建筑信息面板刷新建筑按钮绑定
        BuildingInfoPath infoPath = FindObjectOfType<BuildingInfoPath>(true);
        if (infoPath != null)
        {
            infoPath.OnMapSwitchCompleted();
        }
    }

    public void SwitchLeft()
    {
        if (currentIndex <= 0 || isMoving) return;

        currentIndex--;
        // 全量激活所有地图
        foreach (var map in mapList)
        {
            map.gameObject.SetActive(true);
        }
        // 所有地图右移1920
        for (int i = 0; i < targetPositions.Count; i++)
        {
            targetPositions[i] += new Vector3(MAP_WIDTH, 0, 0);
        }

        isMoving = true;
        UpdateButtonState();
    
        // 【新增】先处理面板 → 再刷新数据
        HandlePanelBeforeRefresh();
        RefreshBuildManager();
        
        // 【新增】通知建筑信息面板刷新建筑按钮绑定
        BuildingInfoPath infoPath = FindObjectOfType<BuildingInfoPath>(true);
        if (infoPath != null)
        {
            infoPath.OnMapSwitchCompleted();
        }
    }

    void UpdateButtonState()
    {
        switchLeftBtn.gameObject.SetActive(currentIndex > 0);
        switchRightBtn.gameObject.SetActive(currentIndex < mapList.Count - 1);
    }
    // 【新增】地图切换前：面板移到(2000,2000)并切换状态
    private void HandlePanelBeforeRefresh()
    {
        if (_enterFightRoomPath == null || _enterFightRoomPath.panelRect == null) return;
    
        // 缓存面板原始位置
        _originPanelPos = _enterFightRoomPath.panelRect.position;
    
        // 1. 统一设为未激活
        _enterFightRoomPath.panelRect.gameObject.SetActive(false);
        // 2. 移动到(2000,2000)（保留Z轴）
        _enterFightRoomPath.panelRect.position = new Vector3(2000, 2000, _enterFightRoomPath.panelRect.position.z);
        // 3. 设为激活
        _enterFightRoomPath.panelRect.gameObject.SetActive(true);
    }

// 【新增】地图切换完成后：恢复面板位置和未激活状态
    private void RestorePanelAfterMove()
    {
        if (_enterFightRoomPath == null || _enterFightRoomPath.panelRect == null) return;
    
        // 1. 设为未激活
        _enterFightRoomPath.panelRect.gameObject.SetActive(false);
        // 2. 移回原始位置
        _enterFightRoomPath.panelRect.position = _originPanelPos;
        //3. 更改当前按钮状态
        InitAndLockAllResouce.Instance.UpdateMapSwitchPermission(); //新增

    }

    void RefreshBuildManager()
    {
        var buildManager = FindObjectOfType<BuildingPlacementManager>();
        if (buildManager != null)
        {
            buildManager.RefreshBuildParent();
        }
        
        var enterFightRoomPath = FindObjectOfType<EnterFightRoomPath>();
        if (enterFightRoomPath != null)
        {
            enterFightRoomPath.RefreshCurrentMapRegions();
        }
    }

    public RectTransform GetCurrentRegionToBuild()
    {
        var region = mapList[currentIndex].Find("RegionToBuild");
        return region != null ? region.GetComponent<RectTransform>() : null;
    }
}

