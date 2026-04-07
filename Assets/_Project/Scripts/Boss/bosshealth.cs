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

    [Header("Soul Reward")]
    public int soulReward = 150;
    public GameObject soulVFXPrefab;

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
            // Души
            if (SoulManager.Instance != null)
                SoulManager.Instance.AddSouls(soulReward);

            // VFX
            if (soulVFXPrefab != null)
            {
                int count = Random.Range(3, 6);
                for (int i = 0; i < count; i++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-0.3f, 0.3f),
                        Random.Range(0f, 0.3f),
                        0f
                    );
                    Instantiate(soulVFXPrefab, transform.position + offset, Quaternion.identity);
                }
            }

            if (bossAI) bossAI.Die();
            if (bossHpPanel) bossHpPanel.SetActive(false);
        }
    }
}