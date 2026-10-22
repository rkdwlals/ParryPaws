using System.Collections;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Move & Jump")]
    public float moveSpeed = 3f;
    public float jumpForce = 7f;
    public int maxJumpCount = 2;

    [Header("Respawn")]
    public Transform defaultRespawnPoint;
    public float respawnDelay = 1.5f;

    [Header("Parry")]
    public float parryDuration = 0.2f;     // Q 패링 유효 시간

    [Header("HP (UI와 동일, 0~4 칸)")]
    public int maxHP = 4;
    public int currentHP;
    public HpBarUI hpBar;                 // 플레이어 HP바 (HpBarUI 스크립트)

    [Header("Attack Range")]
    public Transform attackPoint;         // 공격 범위 중심(플레이어 자식 오브젝트)
    public float attackRadius = 0.8f;     // 공격 범위 반지름

    [Header("Attack VFX")]
    public Animator slashVfxAnimator;     // attack1~4 슬래시용 VFX Animator (VFX_slash 컨트롤러)
    public Animator powerVfxAnimator;     // powerAttack용 VFX Animator
    public Animator thrustVfxAnimator;    // thrust용 VFX Animator
    public bool flipVfxByDirection = true; // 좌우 방향에 따라 이펙트도 뒤집을지

    [Header("Parry VFX")]
    public Animator parryVfxAnimator;     // 패링(VFX_hit2)용 Animator

    [Header("Sound")]
    public PlayerSoundManager sound;      // 🔊 사운드 매니저

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 input;

    private int jumpCount = 0;
    private bool isGrounded = false;

    private bool isFacingRight = true;
    private Vector3 baseScale;

    private Vector3 currentRespawnPosition;
    private bool isDead = false;

    // 패링 상태
    private bool isParrying = false;
    private float parryEndTime = 0f;
    public bool IsParrying => isParrying;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        baseScale = transform.localScale;
        isFacingRight = baseScale.x >= 0;

        currentRespawnPosition = defaultRespawnPoint != null
            ? defaultRespawnPoint.position
            : transform.position;

        // HP 초기화
        currentHP = maxHP;
        if (hpBar != null)
        {
            hpBar.maxHp = maxHP;
            hpBar.SetHp(currentHP);
        }

        // 걷기 루프는 시작 시 꺼두기
        if (sound != null)
            sound.StopWalkLoop();
    }

    private void Update()
    {
        if (isDead) return;

        GetInput();
        AttackInput();
        OtherActions();
        JumpInput();
        UpdateParryState();

        UpdateWalkSound();
    }

    void GetInput()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = 0f;

        if (anim != null)
            anim.SetBool("isWalking", Mathf.Abs(input.x) > 0.1f);
    }

    void JumpInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && jumpCount < maxJumpCount)
        {
            jumpCount++;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // 사운드: 점프 / 더블점프
            if (sound != null)
            {
                if (jumpCount == 1)
                    sound.PlayJump();
                else if (jumpCount == 2)
                    sound.PlayDoubleJump();
            }

            // 공중으로 뜨면 걷기 루프 정지
            if (sound != null)
                sound.StopWalkLoop();

            if (anim != null && jumpCount == 2)
                anim.SetTrigger("roll");
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(input.x * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(input.x) > 0.1f)
        {
            if (input.x > 0 && !isFacingRight)
                isFacingRight = true;
            else if (input.x < 0 && isFacingRight)
                isFacingRight = false;
        }
    }

    private void LateUpdate()
    {
        float dir = isFacingRight ? 1f : -1f;
        float xAbs = Mathf.Abs(baseScale.x);

        transform.localScale = new Vector3(
            dir * xAbs,
            baseScale.y,
            baseScale.z
        );
    }

    // 🔊 걷기 루프 제어
    private void UpdateWalkSound()
    {
        if (sound == null || rb == null || isDead) return;

        bool walkingNow =
            isGrounded &&
            Mathf.Abs(input.x) > 0.1f &&
            Mathf.Abs(rb.linearVelocity.y) < 0.1f;

        if (walkingNow)
            sound.StartWalkLoop();
        else
            sound.StopWalkLoop();
    }

    // ===== 공격/패링 VFX 재생 공통 함수 =====
    private void PlayAttackVfx(Animator targetAnimator, string triggerName)
    {
        if (targetAnimator == null) return;

        // 플레이어 방향에 따라 이펙트도 뒤집기
        if (flipVfxByDirection)
        {
            Vector3 ls = targetAnimator.transform.localScale;
            float sign = isFacingRight ? 1f : -1f;
            ls.x = Mathf.Abs(ls.x) * sign;
            targetAnimator.transform.localScale = ls;
        }

        // 이펙트 위치를 공격 지점에 맞추고 싶으면
        if (attackPoint != null)
        {
            targetAnimator.transform.position = attackPoint.position;
        }

        targetAnimator.SetTrigger(triggerName);
    }

    // ===== 패링 전용 VFX =====
    private void PlayParryVfx()
    {
        PlayAttackVfx(parryVfxAnimator, "parry");
    }

    /// <summary>
    /// 물체(화살, 공격 등)를 실제로 패링에 성공했을 때 외부에서 호출해 주는 함수
    /// </summary>
    public void OnParrySuccess()
    {
        if (!isParrying) return;

        if (sound != null)
            sound.PlayParry();     // 패링 성공 사운드

        PlayParryVfx();
    }

    // ===== 공격 입력 및 판정 (좌클릭, E, 우클릭 트리거는 여기/OtherActions에서, 판정은 DoMeleeHit에서) =====
    void AttackInput()
    {
        if (anim == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("idle") || state.IsName("walk"))
            {
                anim.SetTrigger("attack1");
                PlayAttackVfx(slashVfxAnimator, "attack1");
                if (sound != null) sound.PlayAttack1();
            }
            else if (state.IsName("attack1"))
            {
                anim.SetTrigger("attack2");
                PlayAttackVfx(slashVfxAnimator, "attack2");
                if (sound != null) sound.PlayAttack2();
            }
            else if (state.IsName("attack2"))
            {
                anim.SetTrigger("attack3");
                PlayAttackVfx(slashVfxAnimator, "attack3");
                if (sound != null) sound.PlayAttack3();
            }
            else if (state.IsName("attack3"))
            {
                anim.SetTrigger("attack4");
                PlayAttackVfx(slashVfxAnimator, "attack4");
                if (sound != null) sound.PlayAttack4();
            }

            // 입력 시점에 바로 공격 판정
            DoMeleeHit();
        }
    }

    void OtherActions()
    {
        if (anim == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("roll");
            if (sound != null) sound.PlayRoll();
        }

        // Q = 패링 (여기서는 사운드 X, 성공 시에만)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetTrigger("parry");
            StartParryWindow();
        }

        // E = 찌르기 스킬 (전방 공격)
        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("thrust");
            PlayAttackVfx(thrustVfxAnimator, "thrust");
            if (sound != null) sound.PlayStab();
            DoMeleeHit();
        }

        // 우클릭 = 파워 공격
        if (Input.GetMouseButtonDown(1))
        {
            anim.SetTrigger("powerAttack");
            PlayAttackVfx(powerVfxAnimator, "powerAttack");
            if (sound != null) sound.PlayRightClickAttack();
            DoMeleeHit();
        }

        if (Input.GetKeyDown(KeyCode.K))
            anim.SetTrigger("dead");
    }

    void StartParryWindow()
    {
        isParrying = true;
        parryEndTime = Time.time + parryDuration;
    }

    void UpdateParryState()
    {
        if (isParrying && Time.time >= parryEndTime)
        {
            isParrying = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        foreach (var c in collision.contacts)
        {
            if (c.normal.y > 0.5f)
            {
                bool wasGrounded = isGrounded;
                isGrounded = true;
                jumpCount = 0;

                // 착지 사운드 (공중에서 내려온 경우에만)
                if (sound != null && !wasGrounded)
                    sound.PlayLand();

                break;
            }
        }

        if (collision.collider.CompareTag("DeathZone"))
        {
            Die();
        }

        if (collision.collider.CompareTag("BossStage"))
        {
            currentRespawnPosition = collision.collider.transform.position;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isDead) return;

        // 단순하게 충돌이 끊기면 땅에서 떨어진 걸로 처리
        isGrounded = false;

        if (sound != null)
            sound.StopWalkLoop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("DeathZone"))
        {
            Die();
        }

        if (collision.CompareTag("BossStage"))
        {
            currentRespawnPosition = collision.transform.position;
        }
    }

    // ========== HP 관련 ==========
    public void OnHit(int amount)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);

        if (hpBar != null)
            hpBar.SetHp(currentHP);

        if (sound != null)
            sound.PlayHit();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);

        if (hpBar != null)
            hpBar.SetHp(currentHP);
    }

    // 화살 맞았을 때(ArrowProjectile에서 호출)
    public void HitByProjectile(int damage)
    {
        OnHit(damage);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        input = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        isParrying = false;

        if (sound != null)
        {
            sound.StopWalkLoop();
            sound.PlayDeath();
        }

        if (anim != null)
        {
            anim.ResetTrigger("attack1");
            anim.ResetTrigger("attack2");
            anim.ResetTrigger("attack3");
            anim.ResetTrigger("attack4");
            anim.ResetTrigger("roll");
            anim.ResetTrigger("parry");
            anim.ResetTrigger("thrust");
            anim.ResetTrigger("powerAttack");
            anim.ResetTrigger("dead");

            anim.SetBool("isWalking", false);

            anim.Play("dead", 0, 0f);
        }

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        transform.position = currentRespawnPosition;
        rb.linearVelocity = Vector2.zero;

        jumpCount = 0;
        isGrounded = true;
        isDead = false;
        isParrying = false;

        currentHP = maxHP;
        if (hpBar != null)
            hpBar.SetHp(currentHP);

        if (sound != null)
            sound.StopWalkLoop();
    }

    // ===== 실제 근접 공격 판정 =====
    private void DoMeleeHit()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        foreach (var h in hits)
        {
            // 1) 까마귀(기존 로직)
            CrowArcherController crow = h.GetComponentInParent<CrowArcherController>();
            if (crow != null && !crow.IsDead)
            {
                crow.OnHit();
                break;
            }

            // 2) 닌자 적
            NinjaEnemyController ninja = h.GetComponentInParent<NinjaEnemyController>();
            if (ninja != null && !ninja.IsDead)
            {
                // 원하는 만큼 데미지 / 스턴 여부 조절
                ninja.TakeDamage(1, false);   // 1 데미지, 스턴 없음
                break;
            }
        }
    }


    // 에디터에서 공격 범위 확인용
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
