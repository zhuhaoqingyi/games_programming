using UnityEngine;
using System.Collections.Generic;
using GameCore;
using GridSystem;
using PowerSystem;
using LogisticsSystem;
using ProductionSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    public GridManager gridManager;
    public PowerManager powerManager;

    private StorageManager storageManager = new StorageManager();
    private GameTime gameTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeStartingBuildings();
    }

    private void InitializeGame()
    {
        gameTime = new GameTime(0f, 0f);
        InitializeInitialResources();
    }

    private void InitializeStartingBuildings()
    {
        if (GridManager.Instance == null) return;

        for (int x = -1; x <= 2; x++)
        {
            for (int y = -1; y <= 2; y++)
            {
                GridManager.Instance.PlaceBuildingWithDirection(
                    new GridPosition(x, y),
                    BuildingType.BasicBoard,
                    BuildDirection.East
                );
            }
        }

        GridManager.Instance.PlaceBuildingWithDirection(
            new GridPosition(-1, -1),
            BuildingType.EmergencyShelter,
            BuildDirection.East
        );

        // Ensure containers from directly placed buildings bypassing BuildingPlacer are registered
        RegisterInitialContainers();
    }

    private void RegisterInitialContainers()
    {
        if (GridManager.Instance == null) return;

        foreach (var kvp in GridManager.Instance.GetAllPlacedBuildings())
        {
            GridPosition pos = kvp.Key;
            PlacedBuilding placed = kvp.Value;
            if (placed == null || placed.GameObject == null) continue;

            ContainerComponent container = placed.GameObject.GetComponentInChildren<ContainerComponent>();
            if (container != null && container.resourceCapacities.Count > 0)
            {
                Dictionary<ResourceType, int> capacities = new Dictionary<ResourceType, int>();
                foreach (var rc in container.resourceCapacities)
                {
                    capacities[rc.resourceType] = rc.capacity;
                }
                storageManager.AddContainer(capacities, container.GetTotalCapacity());
                Debug.Log($"[GameManager] Registered initial container at ({pos.x}, {pos.y}) with {container.resourceCapacities.Count} resource types");
            }
        }
    }

    private void InitializeInitialResources()
    {
        storageManager.AddResource(ResourceType.SpaceOre, 50);
    }

    private void Update()
    {
        gameTime = new GameTime(gameTime.totalTime + Time.deltaTime, Time.deltaTime);
        UpdateSystems();
    }

    private void UpdateSystems()
    {
        powerManager?.UpdatePower();
        // Productor自带Update逻辑，不需要ProductionManager驱动
    }

    public int GetResourceAmount(ResourceType type)
    {
        return storageManager.GetResourceAmount(type);
    }

    public bool AddResource(ResourceType type, int amount)
    {
        return storageManager.AddResource(type, amount);
    }

    public bool RemoveResource(ResourceType type, int amount)
    {
        return storageManager.RemoveResource(type, amount);
    }

    public bool HasEnoughResource(ResourceType type, int amount)
    {
        return storageManager.HasEnoughResource(type, amount);
    }

    public Dictionary<ResourceType, int> GetAllResources()
    {
        return storageManager.GetAllResources();
    }

    public int GetTotalStorageCapacity()
    {
        return storageManager.TotalCapacity;
    }

    public int GetResourceCapacity(ResourceType type)
    {
        return storageManager.GetResourceCapacity(type);
    }

    public void AddStorageCapacity(int amount)
    {
        storageManager.AddSimpleStorage(amount);
    }

    public void RemoveStorageCapacity(int amount)
    {
        storageManager.RemoveSimpleStorage(amount);
    }

    public void AddContainer(Dictionary<ResourceType, int> capacities, int totalCapacityAdd)
    {
        storageManager.AddContainer(capacities, totalCapacityAdd);
    }

    public void RemoveContainer(Dictionary<ResourceType, int> capacities, int totalCapacityRemove)
    {
        storageManager.RemoveContainer(capacities, totalCapacityRemove);
    }

    public void EnforceCapacityLimits()
    {
        storageManager.EnforceCapacityLimits();
    }

    public GameTime GetGameTime()
    {
        return gameTime;
    }

    public bool CheckVictoryCondition()
    {
        return HasEnoughResource(ResourceType.AdvancedPart, 100) &&
               HasEnoughResource(ResourceType.BasicPart, 80);
    }

    public void OnVictory()
    {
        Debug.Log("胜利！建造星际飞船完成！");
    }
}
