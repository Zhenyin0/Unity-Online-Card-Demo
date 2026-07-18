using UnityEngine;
public class AutoHideAfterTime : MonoBehaviour
{
    public float hideAfter = 60f;
    void Start() => Invoke("HideMe", hideAfter);
    void HideMe() => gameObject.SetActive(false);
}