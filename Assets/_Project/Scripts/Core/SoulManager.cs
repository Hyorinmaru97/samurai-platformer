using UnityEngine;
using System;

public class SoulManager : MonoBehaviour
{
    public static SoulManager Instance { get; private set; }

    public event Action<int> OnSoulsChanged;

    int currentSouls;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int GetSouls() => currentSouls;

    public void AddSouls(int amount)
    {
        if (amount <= 0) return;
        currentSouls += amount;
        OnSoulsChanged?.Invoke(currentSouls);
    }

    public bool SpendSouls(int amount)
    {
        if (amount <= 0) return false;
        if (currentSouls < amount) return false;
        currentSouls -= amount;
        OnSoulsChanged?.Invoke(currentSouls);
        return true;
    }
}