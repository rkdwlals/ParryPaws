using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageClearCircleEffect : MonoBehaviour
{
    // 🔥 항상 하나만 존재하게 할 싱글턴 인스턴스
    public static StageClearCircleEffect Instance;

    [Header("참조")]
    public Transform player;
    public Camera mainCamera;
    public Material circleMaterial;
    public AudioSource bgmSource;
    public string nextSceneName = "StageSelect";

    [Header("레벨 해금 설정")]
    public int unlockLevel = 2;

    [Header("애니메이션 설정")]
    public float startRadius = 1.5f;
    public float endRadius = 0.0f;
    public float duration = 1.5f;

    bool playing = false;

    void Awake()
    {
        // 🔥 싱글턴 세팅
        if (Instance != null && Instance != this)
        {
            // 이미 살아 있는 매니저가 있으면, 새로 생긴 건 삭제
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 🔥 씬이 바뀌어도 계속 살아 있게
        DontDestroyOnLoad(gameObject);

        // 🔥 혹시라도 Inspector에서 꺼놨으면 강제 켜기
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (circleMaterial != null)
            circleMaterial.SetFloat("_Radius", startRadius);
    }

    void OnDisable()
    {
        // 누가 StageClearManager를 꺼버리는지 찾기 위한 디버그용
        Debug.LogWarning("StageClearManager(StageClearCircleEffect)가 비활성화되었습니다.", this);
    }

    // ✅ 외부에서 이 함수만 부르면 됨
    public void PlayEffect()
    {
        // 혹시라도 비활성 상태면 그냥 무시 (에러 방지)
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("StageClearManager가 비활성 상태라서 연출을 재생할 수 없습니다.", this);
            return;
        }

        if (!playing)
            StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        playing = true;

        if (circleMaterial == null || player == null || mainCamera == null)
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        Vector3 vp = mainCamera.WorldToViewportPoint(player.position);
        circleMaterial.SetVector("_Center", new Vector4(vp.x, vp.y, 0, 0));

        float t = 0f;
        float startVolume = (bgmSource != null) ? bgmSource.volume : 1f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);

            float r = Mathf.Lerp(startRadius, endRadius, lerp);
            circleMaterial.SetFloat("_Radius", r);

            if (bgmSource != null)
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, lerp);

            yield return null;
        }

        circleMaterial.SetFloat("_Radius", endRadius);
        if (bgmSource != null) bgmSource.volume = 0f;

        if (ButtonSettings.releasedLevelStatic < unlockLevel)
        {
            ButtonSettings.releasedLevelStatic = unlockLevel;
            PlayerPrefs.SetInt("Level", ButtonSettings.releasedLevelStatic);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(nextSceneName);

        playing = false;
    }
}
