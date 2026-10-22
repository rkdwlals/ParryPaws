using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Common")]
    public float lifeTime = 5f;

    [Header("Damage")]
    public int damageToPlayer = 1;   // 플레이어에게 줄 데미지 (플레이어 HP 칸 단위)

    private Rigidbody2D rb;
    private bool isReflected = false;    // 패링으로 튕겨진 화살인지
    private GameObject owner;            // 원래 발사한 까마귀 오브젝트
    private Vector3 baseScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
    }

    private void Start()
    {
        // lifeTime 초 후 화살 자동 파괴
        Destroy(gameObject, lifeTime);
    }

    public bool IsReflected()
    {
        return isReflected;
    }

    // 플레이어를 향해 직선으로 날아가는 화살 설정
    public void InitStraight(Vector2 direction, float speed, GameObject owner)
    {
        this.owner = owner;

        Vector2 dirNorm = direction.normalized;

        rb.gravityScale = 0f;
        rb.linearVelocity = dirNorm * speed;

        float sign = dirNorm.x >= 0 ? 1f : -1f;
        transform.localScale = new Vector3(
            sign * Mathf.Abs(baseScale.x),
            baseScale.y,
            baseScale.z
        );
    }

    // 포물선(수평 + 위쪽 속도) 화살 설정
    public void InitArc(float horizontalSign, float horizontalSpeed, float upVelocity, GameObject owner)
    {
        this.owner = owner;

        rb.gravityScale = 1f;
        rb.linearVelocity = new Vector2(horizontalSign * horizontalSpeed, upVelocity);

        float sign = horizontalSign >= 0 ? 1f : -1f;
        transform.localScale = new Vector3(
            sign * Mathf.Abs(baseScale.x),
            baseScale.y,
            baseScale.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) 이미 반사된 화살일 때: 까마귀(본체 또는 히트박스)에 맞았을 경우
        if (isReflected)
        {
            // 까마귀 컨트롤러는 Crow 본체에 있고, 히트박스는 자식에 있을 수 있으므로 InParent 사용
            CrowArcherController crow = other.GetComponentInParent<CrowArcherController>();
            if (crow != null)
            {
                // 까마귀가 한 번 맞았다고 알림 (내부에서 맞은 횟수/HP바 처리)
                crow.OnHit();

                // 아직 죽지 않았다면 스턴 처리
                if (!crow.IsDead)
                {
                    crow.Stun(crow.stunDuration);
                }

                // 화살 파괴
                Destroy(gameObject);
                return;
            }
        }

        // 2) 아직 반사되지 않은 화살 → 플레이어 피격 / 패링 처리
        if (!isReflected)
        {
            if (other.CompareTag("Player"))
            {
                // 콜라이더가 자식에 있고 PlayerController2D가 부모에 있을 수도 있으므로 InParent 사용
                PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
                if (player != null)
                {
                    if (player.IsParrying)
                    {
                        // ✅ 패링 성공 지점 : 여기에서만 VFX 재생
                        player.OnParrySuccess();   // 패링 이펙트 호출
                        // 그리고 화살을 반사
                        ReflectBackToOwner();
                        return;
                    }
                    else
                    {
                        // 패링이 아니면 플레이어에게 데미지 전달
                        player.HitByProjectile(damageToPlayer);
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }

        // 3) 지면/벽/DeathZone 등에 맞으면 화살 삭제
        if (other.CompareTag("Ground") || other.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
        }
    }

    private void ReflectBackToOwner()
    {
        isReflected = true;

        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        // 반사된 뒤에는 직선으로 날아가도록 중력 제거
        rb.gravityScale = 0f;

        // 현재 위치에서 원래 발사자(까마귀) 방향으로 다시 날려보냄
        Vector2 toOwner = ((Vector2)owner.transform.position - (Vector2)transform.position).normalized;
        float speed = rb.linearVelocity.magnitude;
        if (speed < 0.1f) speed = 8f;    // 너무 느리면 기본 속도 보정

        rb.linearVelocity = toOwner * speed;

        // 방향에 따라 화살 스프라이트 좌우 반전
        float sign = toOwner.x >= 0 ? 1f : -1f;
        transform.localScale = new Vector3(
            sign * Mathf.Abs(baseScale.x),
            baseScale.y,
            baseScale.z
        );
    }
}
