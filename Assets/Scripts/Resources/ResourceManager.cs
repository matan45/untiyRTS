using UnityEngine;
using System;

/// <summary>
/// Manages player resources (credits and power).
/// NOTE: This is a SCENE-SPECIFIC singleton - resources reset per level.
/// The instance will be destroyed when loading a new scene.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Singleton Settings")]
    [SerializeField, Tooltip("If true, resources will persist across scene loads. Typically FALSE for level-based games.")]
    private bool persistAcrossScenes = false;

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
    public event Action OnResourcesChanged; // Combined event for any resource change (per CLAUDE.md event-driven architecture)
    
    void Awake()
    {
        // Singleton pattern with optional persistence
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"ResourceManager: Duplicate instance found on {gameObject.name}. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional persistence (typically false for scene-specific resources)
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("ResourceManager: Set to persist across scenes.");
        }
    }
    
    void Start()
    {
        currentCredits = startingCredits;
        currentPower = startingPower;
        powerUsed = 0;

        OnCreditsChanged?.Invoke(currentCredits);
        OnPowerChanged?.Invoke(AvailablePower, currentPower);
        OnResourcesChanged?.Invoke();
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
        OnResourcesChanged?.Invoke();

        return true;
    }
    
    public void AddCredits(int amount)
    {
        currentCredits += amount;
        OnCreditsChanged?.Invoke(currentCredits);
        OnResourcesChanged?.Invoke();
    }
    
    public void AddPower(int amount)
    {
        currentPower += amount;
        OnPowerChanged?.Invoke(AvailablePower, currentPower);
        OnResourcesChanged?.Invoke();
    }
    
    public void RefundResources(int credits, int power = 0)
    {
        currentCredits += credits;
        powerUsed -= power;

        OnCreditsChanged?.Invoke(currentCredits);
        OnPowerChanged?.Invoke(AvailablePower, currentPower);
        OnResourcesChanged?.Invoke();
    }

    void OnDestroy()
    {
        // Clear static instance when destroyed
        if (Instance == this)
        {
            Instance = null;
        }
    }
}