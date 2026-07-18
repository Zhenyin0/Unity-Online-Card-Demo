using UnityEngine;
using UnityEngine.UI;

public class DropdownImageSwitcher : MonoBehaviour
{
    [Header("引用设置")]
    [Tooltip("你的Dropdown组件")]
    public Dropdown targetDropdown;

    [Tooltip("要修改图片的build-image对象")]
    public Image buildImage;

    [Tooltip("按Dropdown选项顺序，存放每个选项对应的图片（顺序必须和Dropdown的选项顺序一致！）")]
    public Sprite[] optionSprites;


    void Start()
    {
        if (targetDropdown != null)
        {
            // 移除重复监听，避免多次调用
            //targetDropdown.onValueChanged.RemoveAllListeners();
            // 绑定选项变化的回调方法
            targetDropdown.onValueChanged.AddListener(OnOptionChanged);
            // 初始化默认选中项的图片
            OnOptionChanged(targetDropdown.value);
        }
    }


    // 当Dropdown选项变化时自动调用，参数是选中选项的索引（从0开始）
    void OnOptionChanged(int selectedIndex)
    {
        // 防止数组越界或引用缺失
        if (buildImage == null || optionSprites == null || selectedIndex < 0 || selectedIndex >= optionSprites.Length)
        {
            Debug.LogWarning("DropdownImageSwitcher: 引用缺失或图片数组长度不匹配！");
            return;
        }

        // 核心逻辑：设置build-image的图片为当前选中索引对应的图片
        buildImage.sprite = optionSprites[selectedIndex];
    }
}