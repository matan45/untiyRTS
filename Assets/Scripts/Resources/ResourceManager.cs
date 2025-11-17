using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    
    [Header("Starting Resources")]
    [SerializeField] private int startingCredits = 5000;
    [SerializeField] private int startingPower = 100;
    
    private int currentCredits;
    private int currentPower;
    private int powerUsed;
    
    public int Credits => currentCredits;
    public int AvailablePower => currentPower - powerUsed;
    public int TotalPower => currentPower;
    
    // Events
    public event Action<int> OnCreditsChanged;
    public event Action<int, int> OnPowerChanged; // available, total
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        currentCredits = startingCredits;
        currentPower = startingPower;
        powerUsed = 0;
        
        OnCreditsChanged?.Invoke(currentCredits);
        OnPowerChanged?.Invoke(AvailablePower, currentPower);
    }
    
    public bool CanAfford(int credits, int power = 0)
    {
        return currentCredits >= credits && AvailablePower >= power;
    }
    
    public bool SpendResources(int credits, int power = 0)
    {
        if (!CanAfford(credits, power))
            return false;
            
        currentCredits -= credits;
        powerUsed += power;
        
        OnCreditsChanged?.Invoke(currentCredits);
        OnPowerChanged?.Invoke(AvailablePower, currentPower);
        
        return true;
    }
    
    public void AddCredits(int amount)
    {
        currentCredits += amount;
        OnCreditsChanged?.Invoke(currentCredits);
    }
    
    public void AddPower(int amount)
    {
        currentPower += amount;
        OnPowerChanged?.Invoke(AvailablePower, currentPower);
    }
    
    public void RefundResources(int credits, int power = 0)
    {
        currentCredits += credits;
        powerUsed -= power;
        
        OnCreditsChanged?.Invoke(currentCredits);
        OnPowerChanged?.Invoke(AvailablePower, currentPower);
    }
}