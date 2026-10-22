using UnityEngine;

public class CrowHitbox : MonoBehaviour
{
    private CrowArcherController crow;

    private void Awake()
    {
        crow = GetComponentInParent<CrowArcherController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ArrowProjectile arrow = other.GetComponent<ArrowProjectile>();

        // 화살인지 + 반사된 화살인지 체크
        if (arrow != null && arrow.IsReflected())
        {
            if (crow != null && !crow.IsDead)
            {
                // 1회 피격 처리 (맞은 횟수 증가 + HPBar 감소)
                crow.OnHit();

                // 죽지 않았다면 스턴
                crow.Stun(crow.stunDuration);
            }

            // 화살 삭제
            Destroy(arrow.gameObject);
        }
    }
}
