using DG.Tweening;
using UnityEngine;

public class UIFromTopLeft : MonoBehaviour
{
    public RectTransform targetRect , targetRect1;
    public float duration = 0.5f;

    public void NameJumpOpen()
    {
        // 安全检查
        if (targetRect == null) return;
        targetRect.DOKill();

        // ------------- 🔥 唯一修复：正确计算左上角坐标 -------------
        // 获取父画布的尺寸（适配所有分辨率，永远不会移出屏幕）
        RectTransform canvasRect = targetRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        // 真正的UI左上角（不会把UI挤出画布！）
        Vector2 startPos = new Vector2(-canvasRect.rect.width / 3, canvasRect.rect.height / 3);

        // 初始状态
        targetRect.anchoredPosition = startPos;
        targetRect.localScale = Vector3.zero;
        //targetRect.GetComponent<CanvasRenderer>().SetAlpha(0);
        targetRect.gameObject.SetActive(true);

        // 动画（完全不变！）
        targetRect.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutQuad);
        targetRect.DOScale(1, duration).SetEase(Ease.OutBack);
        // targetRect.GetComponent<CanvasRenderer>().DOFade(1, duration);
    }
    
    public void NameJumpBack()
    {
        // 安全检查
        if (targetRect == null || !targetRect.gameObject.activeSelf) return;
        targetRect.DOKill();

        RectTransform canvasRect = targetRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(-canvasRect.rect.width / 3, canvasRect.rect.height / 3);

        targetRect.DOAnchorPos(startPos, duration).SetEase(Ease.InQuad);
        targetRect.DOScale(0, duration).SetEase(Ease.InBack)
            .OnComplete(() => targetRect.gameObject.SetActive(false));
        // targetRect.GetComponent<CanvasRenderer>().DOFade(1, duration);
    }
    

    public void EmailJumpOpen()
    {
        // 安全检查
        if (targetRect1 == null) return;
        targetRect1.DOKill();

        // ------------- 🔥 唯一修复：正确计算左上角坐标 -------------
        // 获取父画布的尺寸（适配所有分辨率，永远不会移出屏幕）
        RectTransform canvasRect = targetRect1.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        // 真正的UI左上角（不会把UI挤出画布！）
        Vector2 startPos = new Vector2(-canvasRect.rect.width / 3, canvasRect.rect.height / 3);

        // 初始状态
        targetRect1.anchoredPosition = startPos;
        targetRect1.localScale = Vector3.zero;
        //targetRect.GetComponent<CanvasRenderer>().SetAlpha(0);
        targetRect1.gameObject.SetActive(true);

        // 动画（完全不变！）
        targetRect1.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutQuad);
        targetRect1.DOScale(1, duration).SetEase(Ease.OutBack);
        // targetRect.GetComponent<CanvasRenderer>().DOFade(1, duration);
    }

    public void EmailJumpBack()
    {
        if (targetRect1 == null || !targetRect1.gameObject.activeSelf) return;
        targetRect1.DOKill();

        RectTransform canvasRect = targetRect1.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(-canvasRect.rect.width / 3, canvasRect.rect.height / 3);

        targetRect1.DOAnchorPos(startPos, duration).SetEase(Ease.InQuad);
        targetRect1.DOScale(0, duration).SetEase(Ease.InBack)
            .OnComplete(() => targetRect1.gameObject.SetActive(false));
        // targetRect.GetComponent<CanvasRenderer>().DOFade(1, duration);
    }


}