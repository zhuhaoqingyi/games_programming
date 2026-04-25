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
    public ConveyorSystem conveyorSystem;

    private StorageManager storageManager = new StorageManager();
    private ProductionManager productionManager = new ProductionManager();
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

    private void InitializeGame()
    {
        gameTime = new GameTime(0f, 0f);
        InitializeInitialResources();
    }

    private void InitializeInitialResources()
    {
        storageManager.AddResource(ResourceType.SpaceOre, 50);
        storageManager.AddResource(ResourceType.SpaceDebris, 30);
    }

    private void Update()
    {
        gameTime = new GameTime(gameTime.totalTime + Time.deltaTime, Time.deltaTime);
        UpdateSystems();
    }

    private void UpdateSystems()
    {
        powerManager?.UpdatePower(Time.deltaTime);
        conveyorSystem?.UpdateSystem(Time.deltaTime);
        productionManager.UpdateProduction(Time.deltaTime);
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

    public bool HasEnoughResource(RecipeDefinition recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!storageManager.HasEnoughResource(ingredient.resourceType, ingredient.amount))
            {
                return false;
            }
        }
        return true;
    }

    public Dictionary<ResourceType, int> GetAllResources()
    {
        return storageManager.GetAllResources();
    }

    public void RegisterStorage(StorageComponent storage)
    {
        storageManager.RegisterStorage(storage);
    }

    public void UnregisterStorage(StorageComponent storage)
    {
        storageManager.UnregisterStorage(storage);
    }

    public void RegisterConveyor(ConveyorBelt belt)
    {
        conveyorSystem?.RegisterBelt(belt);
    }

    public void UnregisterConveyor(ConveyorBelt belt)
    {
        conveyorSystem?.UnregisterBelt(belt);
    }

    public void RegisterSorter(SorterComponent sorter)
    {
        conveyorSystem?.RegisterSorter(sorter);
    }

    public void UnregisterSorter(SorterComponent sorter)
    {
        conveyorSystem?.UnregisterSorter(sorter);
    }

    public void RegisterProductionBuilding(ProductionBuilding building)
    {
        productionManager.RegisterProductionBuilding(building);
    }

    public void UnregisterProductionBuilding(ProductionBuilding building)
    {
        productionManager.UnregisterProductionBuilding(building);
    }

    public bool TryTransferResource(ResourceType type, int amount, GridPosition from, GridPosition to)
    {
        return conveyorSystem?.TryTransferResource(type, amount, from, to) ?? false;
    }

    public GameTime GetGameTime()
    {
        return gameTime;
    }

    public bool CheckVictoryCondition()
    {
        return HasEnoughResource(ResourceType.AdvancedAlloy, 100) &&
               HasEnoughResource(ResourceType.ElectronicComponent, 50) &&
               HasEnoughResource(ResourceType.MechanicalPart, 80);
    }

    public void OnVictory()
    {
        Debug.Log("胜利！建造星际飞船完成！");
    }
}