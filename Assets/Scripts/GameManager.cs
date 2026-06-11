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

    /// <summary>
    /// 由主菜单 Load 按钮设置，告知 GameManager 进入场景后加载存档
    /// </summary>
    public static bool PendingLoad { get; set; }

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
        if (PendingLoad)
        {
            PendingLoad = false;
            Debug.Log("[GameManager] 主菜单请求加载存档...");
            if (SaveSystem.Instance != null && SaveSystem.Instance.LoadGame())
            {
                Debug.Log("[GameManager] 存档加载成功");
                return;
            }
            Debug.LogWarning("[GameManager] 存档加载失败，改为初始化新游戏");
        }

        Debug.Log("[GameManager] 初始化新游戏");
        InitializeStartingBuildings();
        InitializeInitialResources();
    }

    private void InitializeGame()
    {
        gameTime = new GameTime(0f, 0f);
    }

    private void InitializeStartingBuildings()
    {
        if (GridManager.Instance == null) return;

        // === 1. 初始 4x4 太空板平台 (x: -1~2, y: -1~2) ===
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

        // 太空避难所放在左上角
        GridManager.Instance.PlaceBuildingWithDirection(
            new GridPosition(-1, -1),
            BuildingType.EmergencyShelter,
            BuildDirection.East
        );

        // === 2. 左边扩充 4x2 (x: -3~-2, y: -1~2)，放置两个采矿机（箭头向右/向东采集） ===
        for (int x = -2; x >= -3; x--)
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
        // 上方采矿机（占据 -3,1 到 -2,2），排气方向向东（箭头向右）
        GridManager.Instance.PlaceBuildingWithDirection(
            new GridPosition(-3, 0),
            BuildingType.MiningPlatform,
            BuildDirection.West
        );

        // === 3. 右边扩充 4x2 (x: 3~4, y: -1~2)，放置两个采矿机（箭头向左/向西采集） ===
        for (int x = 3; x <= 4; x++)
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
        // 上方采矿机（占据 3,1 到 4,2），排气方向向西（箭头向左）
        GridManager.Instance.PlaceBuildingWithDirection(
            new GridPosition(3, 0),
            BuildingType.MiningPlatform,
            BuildDirection.East
        );
        for (int x = -1; x <= 2; x++)
        {
            
                GridManager.Instance.PlaceBuildingWithDirection(
                    new GridPosition(x, -2),
                    BuildingType.BasicBoard,
                    BuildDirection.East
                );
            
        }
        // === 4. 下面扩充 2x2 (x: -1~0, y: -3~-2)，放置一个推进器 ===
        for (int x = 0; x <= 1; x++)
        {
            for (int y = -3; y >= -4; y--)
            {
                GridManager.Instance.PlaceBuildingWithDirection(
                    new GridPosition(x, y),
                    BuildingType.BasicBoard,
                    BuildDirection.East
                );
            }
        }
        
        // 推进器（占据 -1,-3 到 0,-2），排气方向向南
        GridManager.Instance.PlaceBuildingWithDirection(
            new GridPosition(0, -4),
            BuildingType.Thruster,
            BuildDirection.South
        );
        for (int x = -1; x <= 2; x++)
        {
            
                GridManager.Instance.PlaceBuildingWithDirection(
                    new GridPosition(x, 3),
                    BuildingType.BasicBoard,
                    BuildDirection.East
                );
            
        }
        for (int x = -1; x <= 2; x++)
        {
            
                GridManager.Instance.PlaceBuildingWithDirection(
                    new GridPosition(x, 4),
                    BuildingType.BasicBoard,
                    BuildDirection.East
                );
            
        }
        for (int x = 0; x <= 1; x++)
        {
            
                GridManager.Instance.PlaceBuildingWithDirection(
                    new GridPosition(x, 5),
                    BuildingType.BasicBoard,
                    BuildDirection.East
                );
            
        }
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
        // 初始资源不检查容量（此时容器尚未注册）
        storageManager.AddResource(ResourceType.SpaceOre, 70, checkCapacity: false);
        storageManager.AddResource(ResourceType.MetalMaterial, 40, checkCapacity: false);
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
