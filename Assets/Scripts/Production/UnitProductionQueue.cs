using System;
using System.Collections.Generic;
using UnityEngine;
using RTS.Data;
using RTS.Buildings;

namespace RTS.Production
{
    /// <summary>
    /// Manages a queue of units being produced by a building.
    /// Similar to BuildQueue but for individual building unit production.
    /// Attach this component to production buildings (e.g., Barracks).
    /// </summary>
    public class UnitProductionQueue : MonoBehaviour
    {
        [Header("Queue Settings")]
        [Tooltip("Maximum number of units that can be queued")]
        [SerializeField] private int maxQueueSize = 5;

        [Tooltip("Spawn point for produced units")]
        [SerializeField] private Transform spawnPoint;

        [Header("Available Units")]
        [Tooltip("Units that this building can produce")]
        [SerializeField] private UnitData[] availableUnits;

        // Events
        public event Action<UnitData> OnProductionStarted;
        public event Action<GameObject> OnUnitProduced;
        public event Action OnQueueChanged;

        // Queue of units being produced
        private List<QueuedUnit> productionQueue = new List<QueuedUnit>();

        [Serializable]
        private class QueuedUnit
        {
            public UnitData unitData;
            public float timeRemaining;
            public float totalTime;
        }

        public int QueueCount => productionQueue.Count;
        public int MaxQueueSize => maxQueueSize;

        private void Awake()
        {
            // Set default spawn point if not assigned
            if (spawnPoint == null)
            {
                spawnPoint = transform;
            }
        }

        private void Update()
        {
            ProcessQueue();
        }

        /// <summary>
        /// Adds a unit to the production queue.
        /// </summary>
        public bool AddToQueue(UnitData unitData)
        {
            if (unitData == null)
            {
                Debug.LogWarning("Cannot add null UnitData to production queue");
                return false;
            }

            if (productionQueue.Count >= maxQueueSize)
            {
                Debug.LogWarning($"Production queue is full ({maxQueueSize} units)");
                return false;
            }

            // Check resources
            if (ResourceManager.Instance != null)
            {
                if (!ResourceManager.Instance.CanAfford(unitData.creditsCost, unitData.powerCost))
                {
                    Debug.LogWarning($"Cannot afford {unitData.unitName}");
                    return false;
                }

                // Spend resources
                ResourceManager.Instance.SpendResources(unitData.creditsCost, unitData.powerCost);
            }

            // Add to queue
            var queuedUnit = new QueuedUnit
            {
                unitData = unitData,
                timeRemaining = unitData.productionTime,
                totalTime = unitData.productionTime
            };

            productionQueue.Add(queuedUnit);

            // Fire events
            if (productionQueue.Count == 1)
            {
                OnProductionStarted?.Invoke(unitData);
            }

            OnQueueChanged?.Invoke();

            Debug.Log($"Added {unitData.unitName} to production queue. Queue size: {productionQueue.Count}/{maxQueueSize}");

            return true;
        }

        /// <summary>
        /// Cancels production at the specified queue index.
        /// </summary>
        public void CancelProduction(int index)
        {
            if (index < 0 || index >= productionQueue.Count)
            {
                Debug.LogWarning($"Invalid queue index: {index}");
                return;
            }

            var cancelled = productionQueue[index];

            // Refund partial cost based on progress
            float progressPercent = 1f - (cancelled.timeRemaining / cancelled.totalTime);
            int refund = Mathf.FloorToInt(cancelled.unitData.creditsCost * (1f - progressPercent));

            if (ResourceManager.Instance != null && refund > 0)
            {
                ResourceManager.Instance.RefundResources(refund, 0);
            }

            productionQueue.RemoveAt(index);
            OnQueueChanged?.Invoke();

            Debug.Log($"Cancelled production of {cancelled.unitData.unitName}. Refunded {refund} credits");
        }

        /// <summary>
        /// Gets the current production queue as a read-only list.
        /// </summary>
        public IReadOnlyList<UnitData> GetProductionQueue()
        {
            var units = new List<UnitData>(productionQueue.Count);
            foreach (var queued in productionQueue)
            {
                units.Add(queued.unitData);
            }
            return units;
        }

        /// <summary>
        /// Gets the production progress for the unit at the specified index (0-1).
        /// </summary>
        public float GetProductionProgress(int index)
        {
            if (index < 0 || index >= productionQueue.Count)
                return 0f;

            var queued = productionQueue[index];
            return 1f - (queued.timeRemaining / queued.totalTime);
        }

        /// <summary>
        /// Checks if a specific unit type can be produced.
        /// </summary>
        public bool CanProduceUnit(UnitData unitData)
        {
            if (unitData == null)
                return false;

            // Check if queue is full
            if (productionQueue.Count >= maxQueueSize)
                return false;

            // Check if this building can produce this unit
            if (availableUnits != null)
            {
                bool found = Array.Exists(availableUnits, u => u == unitData);
                if (!found)
                    return false;
            }

            // Check resources
            if (ResourceManager.Instance != null)
            {
                if (!ResourceManager.Instance.CanAfford(unitData.creditsCost, unitData.powerCost))
                    return false;
            }

            // Check prerequisites
            if (BuildingManager.Instance != null)
            {
                if (!unitData.HasPrerequisites(BuildingManager.Instance))
                    return false;
            }

            return true;
        }

        private void ProcessQueue()
        {
            if (productionQueue.Count == 0)
                return;

            // Process the first unit in queue
            var current = productionQueue[0];
            current.timeRemaining -= Time.deltaTime;

            if (current.timeRemaining <= 0f)
            {
                // Production complete
                CompleteProduction(current);
                productionQueue.RemoveAt(0);

                OnQueueChanged?.Invoke();

                // Start next unit in queue
                if (productionQueue.Count > 0)
                {
                    OnProductionStarted?.Invoke(productionQueue[0].unitData);
                }
            }
        }

        private void CompleteProduction(QueuedUnit queuedUnit)
        {
            if (queuedUnit.unitData.prefab == null)
            {
                Debug.LogWarning($"UnitData '{queuedUnit.unitData.unitName}' has no prefab assigned!");
                return;
            }

            // Spawn the unit
            Vector3 spawnPos = spawnPoint.position;
            Quaternion spawnRot = spawnPoint.rotation;

            GameObject unit = Instantiate(queuedUnit.unitData.prefab, spawnPos, spawnRot);
            unit.name = queuedUnit.unitData.unitName;

            OnUnitProduced?.Invoke(unit);

            Debug.Log($"Produced {queuedUnit.unitData.unitName} at {spawnPos}");
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (spawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward);
            }
        }
#endif
    }
}
