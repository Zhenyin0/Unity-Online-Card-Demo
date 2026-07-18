using UnityEngine;

public class CollisionTester : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("✅ 建筑进入了区域！碰到的物体：" + other.gameObject.name);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("❌ 建筑离开了区域！离开的物体：" + other.gameObject.name);
    }
}