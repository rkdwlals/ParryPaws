using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBar : MonoBehaviour
{
    [Header("체력바 설정")]
    public int maxHp = 5;          // 남은 HP 칸 수 (0~5)
    public int currentHp = 5;      // 현재 남은 HP 칸 수 (0 = 빈, 5 = 풀피)

    [Header("체력바 UI")]
    public Image hpImage;          // HPBar 이미지
    public Sprite[] hpSprites;     // Element0 = 풀피, Element5 = 0HP

    private void Start()
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UpdateHpBar();
    }

    /// <summary>
    /// 남은 HP 칸 수를 직접 설정 (0~maxHp)
    /// </summary>
    public void SetHp(int value)
    {
        currentHp = Mathf.Clamp(value, 0, maxHp);
        UpdateHpBar();
    }

    /// <summary>
    /// 한 칸(또는 여러 칸) 감소
    /// </summary>
    public void TakeDamage(int dmg)
    {
        SetHp(currentHp - dmg);
    }

    private void UpdateHpBar()
    {
        if (hpImage == null || hpSprites == null || hpSprites.Length == 0)
            return;

        // currentHp : 0(빈) ~ maxHp(풀피)
        // Element0 = 풀피, Element5 = 0HP 이므로 인덱스를 뒤집어준다.
        int hpClamped = Mathf.Clamp(currentHp, 0, maxHp);       // 0~5
        int index = maxHp - hpClamped;                          // 5→0(풀피), 0→5(0HP)
        index = Mathf.Clamp(index, 0, hpSprites.Length - 1);    // 배열 범위 보정

        hpImage.sprite = hpSprites[index];
    }
}
