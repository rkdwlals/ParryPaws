using UnityEngine;

public class GoalZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (StageClearCircleEffect.Instance != null)
            {
                // 플레이어/카메라를 여기서 세팅해도 됨 (필요하면)
                // StageClearCircleEffect.Instance.player = other.transform;
                // StageClearCircleEffect.Instance.mainCamera = Camera.main;

                StageClearCircleEffect.Instance.PlayEffect();
            }
            else
            {
                Debug.LogError("StageClearCircleEffect.Instance가 없습니다! StageClearManager가 씬에 있는지 확인하세요.");
            }
        }
    }
}
