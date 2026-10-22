using UnityEngine;

public class NinjaShuriken : MonoBehaviour
{
    [Header("기본 설정")]
    public float lifeTime = 5f;              // 자동 제거 시간
    public int damageToPlayer = 1;           // 플레이어에게 줄 데미지
    public int damageToEnemyOnReflect = 1;   // 튕긴 뒤 적에게 줄 데미지

    private Rigidbody2D rb;
    private bool isReflected = false;        // 패링으로 튕겨졌는지 여부

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 일정 시간 뒤 자동 삭제
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// 적이 발사할 때 방향/속도를 세팅하는 함수
    /// </summary>
    public void Init(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        Vector2 dirNorm = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : Vector2.right;

        Vector2 vel = dirNorm * speed;

        // ✅ Unity 6 스타일 – linearVelocity 사용
        rb.linearVelocity = vel;

        isReflected = false;

        Debug.Log($"[Shuriken] Init dir={dirNorm}, speed={speed}, vel={vel}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) 플레이어와 충돌 (패링 / 데미지)
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            // 패링 중 & 아직 미반사 → 튕기기
            if (player.IsParrying && !isReflected)
            {
                isReflected = true;

                if (rb == null) rb = GetComponent<Rigidbody2D>();
                rb.linearVelocity = -rb.linearVelocity;   // 방향 반전

                player.OnParrySuccess();                  // 패링 VFX/사운드

                return;
            }

            // 패링이 아니고 아직 안 튕겨진 표창이면 → 플레이어에게 데미지
            if (!isReflected)
            {
                player.HitByProjectile(damageToPlayer);
                Destroy(gameObject);
                return;
            }

            // 이미 반사된 상태에서 플레이어를 다시 치면 그냥 무시 (원하면 Destroy 해도 됨)
            return;
        }

        // 2) 닌자 적과 충돌
        NinjaEnemyController enemy = other.GetComponent<NinjaEnemyController>();
        if (enemy != null)
        {
            // ⬇ "반사된 상태에서만" 닌자에게 데미지 + 스턴
            if (isReflected && !enemy.IsDead)
            {
                enemy.TakeDamage(damageToEnemyOnReflect, true);
                Destroy(gameObject);
            }

            // 반사 안 된 상태면 닌자에겐 아무 일도 없음
            return;
        }

        // 3) 그 밖의 것(벽, 발판 등)에 부딪히면 삭제
        Destroy(gameObject);
    }
}
