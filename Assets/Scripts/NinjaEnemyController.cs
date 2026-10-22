using System.Collections;
using UnityEngine;

public class NinjaEnemyController : MonoBehaviour
{
    [Header("참조")]
    public Transform player;              // 플레이어 Transform
    public Animator animator;
    public Rigidbody2D rb;
    public Collider2D mainCollider;
    public SpriteRenderer spriteRenderer;

    [Header("공격 관련")]
    public GameObject shurikenPrefab;     // 표창 프리팹
    public Transform shurikenSpawnPoint;  // 표창 나가는 위치 (닌자 자식 오브젝트)
    public float shurikenSpeed = 8f;

    [Header("순간이동 관련")]
    public Transform[] teleportPoints;    // 순간이동 위치들
    public float teleportIntervalMin = 3f;
    public float teleportIntervalMax = 6f;

    [Header("AI 타이밍")]
    public float attackIntervalMin = 2f;
    public float attackIntervalMax = 4f;

    [Header("체력 / 상태")]
    public int maxHP = 3;
    public float stunDuration = 2f;

    [Header("Bomb VFX")]
    public Animator bombVfxAnimator;          // bombThrow 때 재생할 VFX Animator
    public Transform bombVfxPoint;            // VFX 위치(발판 / 닌자 앞 / 손 등)
    public bool flipBombVfxByDirection = true; // 닌자 방향에 따라 VFX 좌우 반전

    private int currentHP;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isTeleporting = false;

    private float nextAttackTime;
    private float nextTeleportTime;

    private Vector3 baseScale;

    public bool IsDead => isDead;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (mainCollider == null) mainCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        currentHP = maxHP;

        // 🔒 닌자는 물리적으로 안 움직이게 고정 (텔레포트만 할 거라서 이렇게 해도 됨)
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.constraints =
                RigidbodyConstraints2D.FreezePositionX |
                RigidbodyConstraints2D.FreezePositionY |
                RigidbodyConstraints2D.FreezeRotation;
        }

        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        ScheduleNextAttack();
        ScheduleNextTeleport();
    }

    void Update()
    {
        if (isDead) return;

        if (!isStunned && !isTeleporting)
        {
            // 공격 타이밍
            if (Time.time >= nextAttackTime)
            {
                animator.SetTrigger("attack");   // 실제 표창 발사는 애니메이션 이벤트로 ShootShuriken 호출
                ScheduleNextAttack();
            }

            // 순간이동 타이밍
            if (Time.time >= nextTeleportTime)
            {
                StartCoroutine(TeleportRoutine());
                ScheduleNextTeleport();
            }
        }

        FacePlayer();
    }

    void ScheduleNextAttack()
    {
        nextAttackTime = Time.time + Random.Range(attackIntervalMin, attackIntervalMax);
    }

    void ScheduleNextTeleport()
    {
        nextTeleportTime = Time.time + Random.Range(teleportIntervalMin, teleportIntervalMax);
    }

    void FacePlayer()
    {
        if (player == null || spriteRenderer == null) return;

        bool lookRight = (player.position.x >= transform.position.x);

        float sign = lookRight ? 1f : -1f;

        transform.localScale = new Vector3(
            Mathf.Abs(baseScale.x) * sign,
            baseScale.y,
            baseScale.z
        );
    }

    // --------------------
    // 공격 – 애니메이션 이벤트에서 호출
    // --------------------
    public void ShootShuriken()
    {
        Debug.Log("[NinjaEnemy] ShootShuriken() 호출됨");

        if (shurikenPrefab == null || shurikenSpawnPoint == null) return;
        if (isDead) return;

        GameObject proj = Instantiate(
            shurikenPrefab,
            shurikenSpawnPoint.position,
            Quaternion.identity
        );

        // 닌자가 바라보는 방향(스케일 X 부호) 기준으로 좌/우 발사
        float dirX = Mathf.Sign(transform.localScale.x);   // 오른쪽: +1, 왼쪽: -1
        Vector2 dir = new Vector2(dirX, 0f);

        NinjaShuriken ns = proj.GetComponent<NinjaShuriken>();
        if (ns != null)
        {
            ns.Init(dir, shurikenSpeed);
        }
        else
        {
            // 혹시 스크립트 누락 시 직접 속도 세팅
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = dir * shurikenSpeed;
            }
        }
    }

    // --------------------
    // Bomb VFX – bombThrow 애니메이션 이벤트에서 호출
    // --------------------
    public void PlayBombVfx()
    {
        if (bombVfxAnimator == null) return;

        // 위치 맞추기 (발판/손/앞쪽)
        if (bombVfxPoint != null)
        {
            bombVfxAnimator.transform.position = bombVfxPoint.position;
        }

        // 닌자 바라보는 방향에 따라 VFX도 좌우 반전
        if (flipBombVfxByDirection)
        {
            Vector3 ls = bombVfxAnimator.transform.localScale;
            float sign = Mathf.Sign(transform.localScale.x); // 오른쪽: +1, 왼쪽: -1
            ls.x = Mathf.Abs(ls.x) * sign;
            bombVfxAnimator.transform.localScale = ls;
        }

        // VFX Animator에 있는 Trigger 이름은 "play" 라고 가정
        bombVfxAnimator.SetTrigger("play");
    }

    // --------------------
    // 순간이동 루틴
    // --------------------
    IEnumerator TeleportRoutine()
    {
        if (teleportPoints == null || teleportPoints.Length == 0) yield break;

        isTeleporting = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("teleport"); // special 애니 시작

        // special 애니에서 사라지는 타이밍에 맞춰 약간 기다렸다가
        yield return new WaitForSeconds(0.25f);
        HideForTeleport();

        // 랜덤 위치로 이동
        Transform targetPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
        transform.position = targetPoint.position;

        // bombThrow 애니 시작 전에 다시 보이게
        yield return new WaitForSeconds(0.1f);
        ShowAfterTeleport();

        // bombThrow 애니 길이만큼 여유
        yield return new WaitForSeconds(0.5f);

        isTeleporting = false;
    }

    public void HideForTeleport()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;
    }

    public void ShowAfterTeleport()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        // 텔레포트 도중 죽었으면 콜라이더를 다시 켜지 않음
        if (mainCollider != null && !isDead)
            mainCollider.enabled = true;
    }

    // --------------------
    // 피격 / 스턴 / 사망
    // --------------------
    public void TakeDamage(int amount, bool applyStun)
    {
        if (isDead) return;

        currentHP -= amount;
        Debug.Log($"[NinjaEnemy] TakeDamage: {amount}, stun={applyStun}, hp={currentHP}");

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (applyStun)
        {
            StartCoroutine(StunRoutine());
        }
    }

    IEnumerator StunRoutine()
    {
        if (isStunned) yield break;

        isStunned = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("stun");

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // 본체 + 자식 콜라이더 전부 끄기
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        animator.SetBool("isDead", true);

        StartCoroutine(DieCleanup());
    }

    IEnumerator DieCleanup()
    {
        // dead 애니 길이에 맞춰 조정
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
