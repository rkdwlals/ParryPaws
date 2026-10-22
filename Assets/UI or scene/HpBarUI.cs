using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    public Image hpImage;       // 체력바 이미지
    public Sprite[] hpSprites;  // 0~4칸 스프라이트 (총 5개)

    public int maxHp = 4;
    public int currentHp = 4;

    void Start()
    {
        UpdateHpBar();
    }

    public void SetHp(int value)
    {
        currentHp = Mathf.Clamp(value, 0, maxHp);
        UpdateHpBar();
    }

    public void TakeDamage(int amount)
    {
        SetHp(currentHp - amount);
    }

    public void Heal(int amount)
    {
        SetHp(currentHp + amount);
    }

    void UpdateHpBar()
    {
        if (hpImage == null || hpSprites == null || hpSprites.Length == 0)
            return;

        // currentHp가 0~4라고 가정
        int index = Mathf.Clamp(currentHp, 0, hpSprites.Length - 1);
        hpImage.sprite = hpSprites[index];
    }
}
