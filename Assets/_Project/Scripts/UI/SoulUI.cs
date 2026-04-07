using UnityEngine;
using TMPro;

public class SoulUI : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI soulsText;

    void OnEnable()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.OnSoulsChanged += UpdateUI;
            UpdateUI(SoulManager.Instance.GetSouls());
        }
    }

    void OnDisable()
    {
        if (SoulManager.Instance != null)
            SoulManager.Instance.OnSoulsChanged -= UpdateUI;
    }

    void UpdateUI(int amount)
    {
        if (soulsText) soulsText.text = amount.ToString();
    }
}