using UnityEngine;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public Image blackOverlay;
    public Image whiteFlash;
    public Image titleBackground;
    public GameObject pressAnyKey;

    public AudioSource parrySound;

    private bool waitingForInput = true;
    private bool animationPlaying = false;

    void Start()
    {
        // 초기 상태
        blackOverlay.canvasRenderer.SetAlpha(1f);
        whiteFlash.canvasRenderer.SetAlpha(0f);
        titleBackground.canvasRenderer.SetAlpha(0f);

        pressAnyKey.SetActive(true);
    }

    void Update()
    {
        if (waitingForInput && Input.anyKeyDown)
        {
            waitingForInput = false;
            StartParrySequence();
        }
    }

    void StartParrySequence()
    {
        if (animationPlaying) return;
        animationPlaying = true;

        // Press Any Key 텍스트 제거
        pressAnyKey.SetActive(false);

        // 팅 소리
        parrySound.Play();

        // 번쩍
        whiteFlash.canvasRenderer.SetAlpha(1f);
        whiteFlash.CrossFadeAlpha(0f, 0.3f, false);

        // 검은 화면 페이드아웃
        blackOverlay.CrossFadeAlpha(0f, 1.0f, false);

        // 배경 페이드 인
        titleBackground.CrossFadeAlpha(1f, 1.0f, false);

        // UI 완전히 사라지면 비활성화
        Invoke(nameof(DisableTransitionUI), 1.0f);
    }

    void DisableTransitionUI()
    {
        // 배경 이미지를 제외하고 굳이 남길 필요 없으면 비활성화
        blackOverlay.gameObject.SetActive(false);
        whiteFlash.gameObject.SetActive(false);

        // titleBackground는 화면에 표시되므로 남김
        // titleBackground.gameObject.SetActive(true);
    }
}
