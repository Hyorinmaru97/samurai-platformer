using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHp = 30;
    int hp;

    [Header("UI — босс бар снизу экрана")]
    public Slider bossHpBar;
    public GameObject bossHpPanel;

    BossAI bossAI;

    void Start()
    {
        hp = maxHp;
        bossAI = GetComponent<BossAI>();

        if (bossHpPanel) bossHpPanel.SetActive(false);
        if (bossHpBar)
        {
            bossHpBar.maxValue = maxHp;
            bossHpBar.value = maxHp;
        }
    }

    public void ShowHpBar()
    {
        if (bossHpPanel) bossHpPanel.SetActive(true);
    }

    public void TakeDamage(int dmg)
    {
        if (hp <= 0) return;

        ShowHpBar();
        hp -= dmg;
        hp = Mathf.Max(hp, 0);

        if (bossHpBar) bossHpBar.value = hp;

        if (hp <= 0)
        {
            if (bossAI) bossAI.Die();
            if (bossHpPanel) bossHpPanel.SetActive(false);
        }
    }
}
