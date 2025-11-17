using UnityEngine;
using System.Collections.Generic;
using System;
using RTS.Data;
using RTS.Buildings;

[System.Serializable]
public class QueuedBuilding
{
    public Building building;
    public BuildingData data;
    public float timeRemaining;
    public float totalTime;
    
    public QueuedBuilding(Building bldg, BuildingData buildingData)
    {
        building = bldg;
        data = buildingData;
        timeRemaining = buildingData.buildTime;
        totalTime = buildingData.buildTime;
    }
    
    public float GetProgress()
    {
        if (totalTime <= 0)
            return 1f;
        return 1f - (timeRemaining / totalTime);
    }
}

public class BuildQueue : MonoBehaviour
{
    public static BuildQueue Instance { get; private set; }
    
    [SerializeField] private int maxQueueSize = 5;
    
    private List<QueuedBuilding> queue = new List<QueuedBuilding>();
    
    public event Action<QueuedBuilding> OnBuildingStarted;
    public event Action<QueuedBuilding> OnBuildingCompleted;
    public event Action OnQueueChanged;
    
    public List<QueuedBuilding> Queue => queue;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        ProcessQueue();
    }
    
    public bool AddToQueue(Building building, BuildingData data)
    {
        if (queue.Count >= maxQueueSize)
        {
            Debug.LogWarning("Build queue is full!");
            return false;
        }
        
        QueuedBuilding queuedBuilding = new QueuedBuilding(building, data);
        queue.Add(queuedBuilding);
        
        // Disable the building until construction is complete
        if (building != null)
        {
            building.StartConstruction();
        }
        
        OnQueueChanged?.Invoke();
        
        if (queue.Count == 1)
        {
            OnBuildingStarted?.Invoke(queuedBuilding);
        }
        
        Debug.Log($"Added {data.buildingName} to build queue. Position: {queue.Count}");
        
        return true;
    }
    
    private void ProcessQueue()
    {
        if (queue.Count == 0)
            return;
            
        // Process the first building in queue
        QueuedBuilding current = queue[0];
        
        if (current == null || current.data == null)
        {
            queue.RemoveAt(0);
            OnQueueChanged?.Invoke();
            return;
        }
        
        // Update construction time
        current.timeRemaining -= Time.deltaTime;
        
        // Update the building's construction progress
        if (current.building != null)
        {
            current.building.UpdateConstruction(Time.deltaTime);
        }
        
        // Check if construction is complete
        if (current.timeRemaining <= 0)
        {
            CompleteBuilding(current);
        }
    }
    
    private void CompleteBuilding(QueuedBuilding queuedBuilding)
    {
        Debug.Log($"Completed construction of {queuedBuilding.data.buildingName}");
        
        // Enable the building
        if (queuedBuilding.building != null)
        {
            // Building is now functional
            queuedBuilding.building.gameObject.SetActive(true);
        }
        
        // Remove from queue
        queue.Remove(queuedBuilding);
        
        OnBuildingCompleted?.Invoke(queuedBuilding);
        OnQueueChanged?.Invoke();
        
        // Start next building if any
        if (queue.Count > 0)
        {
            OnBuildingStarted?.Invoke(queue[0]);
        }
    }
    
    public bool CanAddToQueue()
    {
        return queue.Count < maxQueueSize;
    }
    
    public void CancelBuilding(int index)
    {
        if (index < 0 || index >= queue.Count)
            return;
            
        QueuedBuilding canceled = queue[index];
        
        // Refund resources
        if (ResourceManager.Instance != null && canceled.data != null)
        {
            ResourceManager.Instance.RefundResources(canceled.data.creditsCost, canceled.data.powerRequired);
        }
        
        // Destroy the building object
        if (canceled.building != null)
        {
            Destroy(canceled.building.gameObject);
        }
        
        queue.RemoveAt(index);
        OnQueueChanged?.Invoke();
        
        Debug.Log($"Canceled construction of {canceled.data.buildingName}");
    }
}