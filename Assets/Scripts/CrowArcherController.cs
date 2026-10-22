using System.Collections;
using UnityEngine;

public class CrowArcherController : MonoBehaviour
{
    [Header("HP (칸 단위)")]
    public int maxHp = 5;                 // 남은 HP 칸 수 (0~5)
    private int currentHp;                // 현재 남은 칸 수
    public EnemyHpBar EnemyHPBar;         // Crow 머리 위 HP바

    [Header("Attack")]
    public Transform firePoint;
    public GameObject arrowPrefab;
    public float attackInterval = 2f;
    public float arrowSpeedStraight = 8f;
    public float arrowSpeedArc = 6f;
    public float arcUpVelocity = 4f;
    public float firstShotDelay = 1f;

    [Header("Attack Timing")]
    public float attack1FireDelay = 0.2f;
    public float attack2FireDelay = 0.25f;

    [Header("Stun / Death")]
    public float stunDuration = 2f;
    public float corpseStayTime = 1.5f;   // 죽은 뒤 시체가 남아 있는 시간
    public float corpseFadeTime = 0.5f;   // 서서히 사라지는 시간

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    private bool isStunned = false;
    private bool isDead = false;
    public bool IsDead => isDead;

    private float attackTimer = 0f;
    private Vector3 baseScale;
    private bool isFacingRight = true;
    private Coroutine stunRoutine;

    private bool useAttack1Next = true;
    private bool isAttacking = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        baseScale = transform.localScale;
        isFacingRight = baseScale.x >= 0;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        attackTimer = -firstShotDelay;

        // HP 초기화: 풀피
        currentHp = maxHp;
        if (EnemyHPBar != null)
        {
            EnemyHPBar.maxHp = maxHp;      // 0~5
            EnemyHPBar.SetHp(currentHp);   // 5칸(풀피)
        }
    }

    private void Update()
    {
        if (isDead || isStunned) return;
        if (player == null) return;

        FacePlayer();

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval && !isAttacking)
        {
            attackTimer = 0f;
            Shoot();
        }
    }

    private void FacePlayer()
    {
        float dir = player.position.x - transform.position.x;

        if (dir > 0f && !isFacingRight)
            isFacingRight = true;
        else if (dir < 0f && isFacingRight)
            isFacingRight = false;

        float sign = isFacingRight ? 1f : -1f;

        transform.localScale = new Vector3(
            sign * Mathf.Abs(baseScale.x),
            baseScale.y,
            baseScale.z
        );
    }

    private void Shoot()
    {
        if (arrowPrefab == null || firePoint == null) return;

        isAttacking = true;

        if (useAttack1Next)
        {
            if (anim != null)
                anim.SetTrigger("attack1");

            StartCoroutine(ShootAttack1Routine());
        }
        else
        {
            if (anim != null)
                anim.SetTrigger("attack2");

            StartCoroutine(ShootAttack2Routine());
        }
    }

    private IEnumerator ShootAttack1Routine()
    {
        yield return new WaitForSeconds(attack1FireDelay);

        if (isDead || isStunned || player == null)
        {
            isAttacking = false;
            yield break;
        }

        Vector2 toPlayer = (player.position - firePoint.position);
        Vector2 dirToPlayer = toPlayer.normalized;

        GameObject arrowGo = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        ArrowProjectile arrow = arrowGo.GetComponent<ArrowProjectile>();
        if (arrow != null)
        {
            arrow.InitStraight(dirToPlayer, arrowSpeedStraight, gameObject);
        }

        useAttack1Next = !useAttack1Next;
        isAttacking = false;
    }

    private IEnumerator ShootAttack2Routine()
    {
        yield return new WaitForSeconds(attack2FireDelay);

        if (isDead || isStunned || player == null)
        {
            isAttacking = false;
            yield break;
        }

        Vector2 toPlayer = (player.position - firePoint.position);
        float horizSign = Mathf.Sign(toPlayer.x);
        if (Mathf.Abs(horizSign) < 0.01f)
            horizSign = isFacingRight ? 1f : -1f;

        GameObject arrowGo = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        ArrowProjectile arrow = arrowGo.GetComponent<ArrowProjectile>();
        if (arrow != null)
        {
            arrow.InitArc(horizSign, arrowSpeedArc, arcUpVelocity, gameObject);
        }

        useAttack1Next = !useAttack1Next;
        isAttacking = false;
    }

    // ===== Crow가 플레이어 공격 또는 튕긴 화살에 맞았을 때 호출 =====
    public void OnHit()
    {
        if (isDead) return;

        // 1칸 감소
        currentHp = Mathf.Max(0, currentHp - 1);

        if (EnemyHPBar != null)
        {
            EnemyHPBar.SetHp(currentHp);   // EnemyHpBar에서 인덱스 변환
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Stun(float duration)
    {
        if (isDead) return;

        if (stunRoutine != null)
            StopCoroutine(stunRoutine);

        stunRoutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("isStunned", true);

        yield return new WaitForSeconds(duration);

        isStunned = false;

        if (anim != null)
            anim.SetBool("isStunned", false);
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.linearVelocity = Vector2.zero;

        // 죽으면 충돌 판정 끄기 (본체 + 자식 콜라이더 전부)
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;
        rb.simulated = false;   // 콜라이더가 꺼진 뒤 바닥을 뚫고 떨어지지 않도록 물리도 정지

        if (anim != null)
        {
            anim.SetBool("isStunned", false);
            anim.ResetTrigger("attack1");
            anim.ResetTrigger("attack2");
            anim.SetTrigger("dead");
        }

        if (EnemyHPBar != null)
        {
            EnemyHPBar.gameObject.SetActive(false);
        }

        StartCoroutine(RemoveCorpse());
    }

    // 닌자처럼 잠시 뒤 사라지도록: 대기 → 페이드아웃 → 삭제
    private IEnumerator RemoveCorpse()
    {
        yield return new WaitForSeconds(corpseStayTime);

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float t = 0f;
        while (t < corpseFadeTime)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / corpseFadeTime);
            foreach (var sr in renderers)
            {
                if (sr == null) continue;
                Color c = sr.color;
                c.a = a;
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
