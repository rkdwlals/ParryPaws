using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [Header("공용 오디오 소스 (효과음용)")]
    public AudioSource sfxSource;

    [Header("이동 사운드")]
    public AudioClip walkClip;         // 걷기 (루프용)
    public AudioClip rollClip;         // 구르기
    public AudioClip landClip;         // 착지

    // 🔊 걷기 전용 오디오 소스
    private AudioSource walkSource;

    [Header("점프 사운드")]
    public AudioClip jumpClip;
    public AudioClip doubleJumpClip;

    [Header("공격 사운드")]
    public AudioClip attack1Clip;
    public AudioClip attack2Clip;
    public AudioClip attack3Clip;
    public AudioClip attack4Clip;
    public AudioClip rightClickAttackClip;
    public AudioClip stabClip;

    [Header("전투 관련 사운드")]
    public AudioClip parryClip;
    public AudioClip hitClip;
    public AudioClip deathClip;

    // 🔉 볼륨 (클리핑 방지)
    // 원본 파일 피크가 0 ~ -1.4 dBFS로 높은 클립들을 피크 약 -6 dBFS가 되도록 낮춘 기본값.
    // 피크가 -8 dBFS 이하인 클립(점프, 공격1·2, 찌르기)은 1 그대로 둠.
    [Header("볼륨 (클리핑 방지용 기본값)")]
    [Range(0f, 1f)] public float masterSfxVolume = 1f;  // 효과음 전체
    [Range(0f, 1f)] public float walkVolume = 1f;       // 걷기 루프 (원본 피크 -22.5 dBFS)
    [Range(0f, 1f)] public float attack3Volume = 0.51f; // 원본 피크 -0.1 dBFS
    [Range(0f, 1f)] public float attack4Volume = 0.57f; // 원본 피크 -1.1 dBFS
    [Range(0f, 1f)] public float rightClickAttackVolume = 0.58f; // 원본 피크 -1.3 dBFS
    [Range(0f, 1f)] public float parryVolume = 0.59f;   // 원본 피크 -1.4 dBFS
    [Range(0f, 1f)] public float hitVolume = 0.5f;      // 원본 피크 0.0 dBFS
    [Range(0f, 1f)] public float deathVolume = 0.5f;    // 원본 피크 0.0 dBFS

    void Awake()
    {
        // SFX용 소스
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        // 걷기 전용 소스
        walkSource = gameObject.AddComponent<AudioSource>();
        walkSource.playOnAwake = false;
        walkSource.loop = true;       // 🔁 루프
        walkSource.spatialBlend = 0;  // 2D
        walkSource.volume = walkVolume * masterSfxVolume;
    }

    // 공용 OneShot
    void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume * masterSfxVolume);
    }

    // ================= 이동 =================

    // 🔊 걷기 시작
    public void StartWalkLoop()
    {
        if (walkClip == null || walkSource == null) return;

        if (walkSource.clip != walkClip)
            walkSource.clip = walkClip;

        if (!walkSource.isPlaying)
            walkSource.Play();
    }

    // 🔊 걷기 종료
    public void StopWalkLoop()
    {
        if (walkSource != null && walkSource.isPlaying)
            walkSource.Stop();
    }

    public void PlayRoll()
    {
        PlayClip(rollClip, 1f);
    }

    public void PlayLand()
    {
        PlayClip(landClip, 1f);
    }

    // ================= 점프 =================

    public void PlayJump() => PlayClip(jumpClip, 1f);
    public void PlayDoubleJump() => PlayClip(doubleJumpClip, 1f);

    // ================= 공격 =================

    public void PlayAttack1() => PlayClip(attack1Clip, 1f);
    public void PlayAttack2() => PlayClip(attack2Clip, 1f);
    public void PlayAttack3() => PlayClip(attack3Clip, attack3Volume);
    public void PlayAttack4() => PlayClip(attack4Clip, attack4Volume);
    public void PlayRightClickAttack() => PlayClip(rightClickAttackClip, rightClickAttackVolume);
    public void PlayStab() => PlayClip(stabClip, 1f);

    // ================= 전투 =================

    public void PlayParry() => PlayClip(parryClip, parryVolume);
    public void PlayHit() => PlayClip(hitClip, hitVolume);
    public void PlayDeath() => PlayClip(deathClip, deathVolume);
}