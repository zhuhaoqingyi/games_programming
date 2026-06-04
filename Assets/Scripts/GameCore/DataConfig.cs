using System.Collections.Generic;

namespace GameCore
{
    public class BoardDefinition
    {
        public BoardType type;
        public string name;
        public string description;
        public BuildingCategory category;
        public List<BoardCost> costs;
        public string prefabPath;
        public string iconPath;

        public BoardDefinition(BoardType type, BuildingCategory category, string name, string description, string prefabPath, string iconPath)
        {
            this.type = type;
            this.category = category;
            this.name = name;
            this.description = description;
            this.prefabPath = prefabPath;
            this.iconPath = iconPath;
            this.costs = new List<BoardCost>();
        }

        public bool CanAfford(Dictionary<ResourceType, int> resources)
        {
            if (costs == null) return true;
            foreach (var cost in costs)
            {
                if (!resources.TryGetValue(cost.resourceType, out int amount) || amount < cost.amount)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class BoardCost
    {
        public ResourceType resourceType;
        public int amount;

        public BoardCost(ResourceType resourceType, int amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }
    }

    public static class DataConfig
    {
        public static Dictionary<ResourceType, ResourceDefinition> ResourceDefinitions = new Dictionary<ResourceType, ResourceDefinition>();
        public static Dictionary<BuildingType, BuildingDefinition> BuildingDefinitions = new Dictionary<BuildingType, BuildingDefinition>();
        public static Dictionary<BoardType, BoardDefinition> BoardDefinitions = new Dictionary<BoardType, BoardDefinition>();
        public static Dictionary<string, RecipeDefinition> RecipeDefinitions = new Dictionary<string, RecipeDefinition>();

        static DataConfig()
        {
            InitializeResources();
            InitializeBoards();
            InitializeBuildings();
            InitializeRecipes();
        }

        private static void InitializeResources()
        {
            ResourceDefinitions[ResourceType.SpaceOre] = new ResourceDefinition(
                ResourceType.SpaceOre, "Space Ore", "Natural ore floating in space", 2.5f);
            ResourceDefinitions[ResourceType.MetalMaterial] = new ResourceDefinition(
                ResourceType.MetalMaterial, "Metal Material", "Refined advanced metal material", 3.0f);
            ResourceDefinitions[ResourceType.BasicPart] = new ResourceDefinition(
                ResourceType.BasicPart, "Basic Part", "Precision machined mechanical parts", 1.5f);
            ResourceDefinitions[ResourceType.AdvancedPart] = new ResourceDefinition(
                ResourceType.AdvancedPart, "Advanced Part", "Top-tier material for spaceship construction", 4.0f);
        }

        private static void InitializeBoards()
        {
            var basicBoard = new BoardDefinition(
                BoardType.BasicBoard, BuildingCategory.Board, "Basic Space Board", "Most basic space platform module",
                prefabPath: "Prefabs/Boards/BasicBoard", iconPath: "Icons/Boards/BasicBoard");
            basicBoard.costs.Add(new BoardCost(ResourceType.SpaceOre, 10));
            BoardDefinitions[BoardType.BasicBoard] = basicBoard;

            var reinforcedBoard = new BoardDefinition(
                BoardType.ReinforcedBoard, BuildingCategory.Board, "Reinforced Space Board", "Reinforced structure, more durable",
                prefabPath: "Prefabs/Boards/ReinforcedBoard", iconPath: "Icons/Boards/ReinforcedBoard");
            reinforcedBoard.costs.Add(new BoardCost(ResourceType.MetalMaterial, 10));
            reinforcedBoard.costs.Add(new BoardCost(ResourceType.SpaceOre, 10));
            BoardDefinitions[BoardType.ReinforcedBoard] = reinforcedBoard;

            var advancedBoard = new BoardDefinition(
                BoardType.AdvancedBoard, BuildingCategory.Board, "Advanced Space Board", "Advanced composite space board",
                prefabPath: "Prefabs/Boards/AdvancedBoard", iconPath: "Icons/Boards/AdvancedBoard");
            advancedBoard.costs.Add(new BoardCost(ResourceType.AdvancedPart, 5));
            advancedBoard.costs.Add(new BoardCost(ResourceType.BasicPart, 10));
            BoardDefinitions[BoardType.AdvancedBoard] = advancedBoard;

            var basicBoardBuilding = new BuildingDefinition(
                BuildingType.BasicBoard, BuildingCategory.Board, basicBoard.name, basicBoard.description,
                width: 1, height: 1, prefabPath: basicBoard.prefabPath, iconPath: basicBoard.iconPath,
                isBoard: true);
            foreach (var cost in basicBoard.costs)
            {
                basicBoardBuilding.costs.Add(new BuildingCost(cost.resourceType, cost.amount));
            }
            BuildingDefinitions[BuildingType.BasicBoard] = basicBoardBuilding;

            var reinforcedBoardBuilding = new BuildingDefinition(
                BuildingType.ReinforcedBoard, BuildingCategory.Board, reinforcedBoard.name, reinforcedBoard.description,
                width: 1, height: 1, prefabPath: reinforcedBoard.prefabPath, iconPath: reinforcedBoard.iconPath,
                isBoard: true);
            foreach (var cost in reinforcedBoard.costs)
            {
                reinforcedBoardBuilding.costs.Add(new BuildingCost(cost.resourceType, cost.amount));
            }
            BuildingDefinitions[BuildingType.ReinforcedBoard] = reinforcedBoardBuilding;

            var advancedBoardBuilding = new BuildingDefinition(
                BuildingType.AdvancedBoard, BuildingCategory.Board, advancedBoard.name, advancedBoard.description,
                width: 1, height: 1, prefabPath: advancedBoard.prefabPath, iconPath: advancedBoard.iconPath,
                isBoard: true);
            foreach (var cost in advancedBoard.costs)
            {
                advancedBoardBuilding.costs.Add(new BuildingCost(cost.resourceType, cost.amount));
            }
            BuildingDefinitions[BuildingType.AdvancedBoard] = advancedBoardBuilding;
        }

        private static void InitializeBuildings()
        {
            var emergencyShelter = new BuildingDefinition(
                BuildingType.EmergencyShelter, BuildingCategory.Core, "Emergency Shelter", "Player starting core base",
                width: 4, height: 4, powerConsumption: 0, powerProduction: 0,
                storageCapacity: 0, isCoreBuilding: true, prefabPath: "Prefabs/Buildings/EmergencyShelter", iconPath: "Icons/Buildings/EmergencyShelter");
            BuildingDefinitions[BuildingType.EmergencyShelter] = emergencyShelter;

            var miningPlatform = new BuildingDefinition(
                BuildingType.MiningPlatform, BuildingCategory.Production, "Mining Platform", "Automatically mines space ore",
                width: 2, height: 2, functionalAreaWidth: 2, functionalAreaHeight: 2, direction: BuildDirection.East,
                powerConsumption: 10, powerProduction: 0, canRotate: true,
                prefabPath: "Prefabs/Buildings/MiningPlatform", iconPath: "Icons/Buildings/MiningPlatform");
            miningPlatform.directionalPrefabPaths[BuildDirection.East] = "Prefabs/Buildings/MiningPlatform1";
            miningPlatform.directionalPrefabPaths[BuildDirection.South] = "Prefabs/Buildings/MiningPlatform2";
            miningPlatform.directionalPrefabPaths[BuildDirection.West] = "Prefabs/Buildings/MiningPlatform3";
            miningPlatform.directionalPrefabPaths[BuildDirection.North] = "Prefabs/Buildings/MiningPlatform4";
            BuildingDefinitions[BuildingType.MiningPlatform] = miningPlatform;

            var nuclearReactor = new BuildingDefinition(
                BuildingType.NuclearReactor, BuildingCategory.Power, "Nuclear Reactor", "Generates power by consuming ore",
                width: 3, height: 3, powerConsumption: 0, powerProduction: 50,
                prefabPath: "Prefabs/Buildings/NuclearReactor", iconPath: "Icons/Buildings/NuclearReactor");
            nuclearReactor.costs.Add(new BuildingCost(ResourceType.SpaceOre, 50));
            BuildingDefinitions[BuildingType.NuclearReactor] = nuclearReactor;

            var solarArray = new BuildingDefinition(
                BuildingType.SolarArray, BuildingCategory.Power, "Solar Array", "Infinite clean energy",
                width: 2, height: 1, powerConsumption: 0, powerProduction: 10,
                prefabPath: "Prefabs/Buildings/SolarArray", iconPath: "Icons/Buildings/SolarArray");
            solarArray.costs.Add(new BuildingCost(ResourceType.MetalMaterial, 30));
            solarArray.costs.Add(new BuildingCost(ResourceType.BasicPart, 10));
            BuildingDefinitions[BuildingType.SolarArray] = solarArray;

            var storageDock = new BuildingDefinition(
                BuildingType.StorageDock, BuildingCategory.Storage, "Storage Dock", "Stores all types of resources",
                width: 2, height: 1, powerConsumption: 2, powerProduction: 0,
                storageCapacity: 500, prefabPath: "Prefabs/Buildings/StorageDock", iconPath: "Icons/Buildings/StorageDock4");
            storageDock.costs.Add(new BuildingCost(ResourceType.SpaceOre, 30));
            BuildingDefinitions[BuildingType.StorageDock] = storageDock;

            var furnaceRefinery = new BuildingDefinition(
                BuildingType.FurnaceRefinery, BuildingCategory.Production, "Furnace Refinery", "Refines ore into metal material",
                width: 2, height: 2, powerConsumption: 15, powerProduction: 0,
                isProductionBuilding: true, prefabPath: "Prefabs/Buildings/FurnaceRefinery", iconPath: "Icons/Buildings/FurnaceRefinery");
            furnaceRefinery.costs.Add(new BuildingCost(ResourceType.SpaceOre, 40));
            BuildingDefinitions[BuildingType.FurnaceRefinery] = furnaceRefinery;

            var partAssembly = new BuildingDefinition(
                BuildingType.PartAssembly, BuildingCategory.Production, "Part Assembly", "Converts metal material into basic parts",
                width: 3, height: 3, powerConsumption: 20, powerProduction: 0,
                isProductionBuilding: true, prefabPath: "Prefabs/Buildings/PartAssembly", iconPath: "Icons/Buildings/PartAssembly");
            partAssembly.costs.Add(new BuildingCost(ResourceType.MetalMaterial, 30));
            partAssembly.costs.Add(new BuildingCost(ResourceType.SpaceOre, 20));
            BuildingDefinitions[BuildingType.PartAssembly] = partAssembly;

            var advancedFactory = new BuildingDefinition(
                BuildingType.AdvancedFactory, BuildingCategory.Production, "Advanced Factory", "Processes basic parts into advanced parts",
                width: 4, height: 4, powerConsumption: 40, powerProduction: 0,
                isProductionBuilding: true, prefabPath: "Prefabs/Buildings/AdvancedFactory", iconPath: "Icons/Buildings/AdvancedFactory");
            advancedFactory.costs.Add(new BuildingCost(ResourceType.MetalMaterial, 50));
            advancedFactory.costs.Add(new BuildingCost(ResourceType.BasicPart, 30));
            BuildingDefinitions[BuildingType.AdvancedFactory] = advancedFactory;

            var conveyorBelt = new BuildingDefinition(
                BuildingType.ConveyorBelt, BuildingCategory.Logistics, "Conveyor Belt", "Automatically transports materials",
                width: 1, height: 1, powerConsumption: 1, powerProduction: 0,
                prefabPath: "Prefabs/Buildings/ConveyorBelt", iconPath: "Icons/Buildings/ConveyorBelt");
            conveyorBelt.costs.Add(new BuildingCost(ResourceType.BasicPart, 5));
            BuildingDefinitions[BuildingType.ConveyorBelt] = conveyorBelt;

            var sorter = new BuildingDefinition(
                BuildingType.Sorter, BuildingCategory.Logistics, "Sorter", "Sorts different materials",
                width: 1, height: 1, powerConsumption: 5, powerProduction: 0,
                prefabPath: "Prefabs/Buildings/Sorter", iconPath: "Icons/Buildings/Sorter");
            sorter.costs.Add(new BuildingCost(ResourceType.BasicPart, 10));
            sorter.costs.Add(new BuildingCost(ResourceType.AdvancedPart, 2));
            BuildingDefinitions[BuildingType.Sorter] = sorter;

            var shipAssembly = new BuildingDefinition(
                BuildingType.ShipAssembly, BuildingCategory.Special, "Ship Assembly Platform", "Builds interstellar spaceship",
                width: 5, height: 3, powerConsumption: 100, powerProduction: 0,
                prefabPath: "Prefabs/Buildings/ShipAssembly", iconPath: "Icons/Buildings/ShipAssembly");
            shipAssembly.costs.Add(new BuildingCost(ResourceType.AdvancedPart, 100));
            shipAssembly.costs.Add(new BuildingCost(ResourceType.AdvancedPart, 50));
            shipAssembly.costs.Add(new BuildingCost(ResourceType.BasicPart, 80));
            BuildingDefinitions[BuildingType.ShipAssembly] = shipAssembly;
        }

        private static void InitializeRecipes()
        {
            var refineMetal = new RecipeDefinition("Refine Metal Material", 5f, BuildingType.FurnaceRefinery);
            refineMetal.AddIngredient(ResourceType.SpaceOre, 1);
            refineMetal.SetOutput(ResourceType.MetalMaterial, 1);
            RecipeDefinitions["RefineMetal"] = refineMetal;

            var makeBasicPart = new RecipeDefinition("Make Basic Part", 8f, BuildingType.PartAssembly);
            makeBasicPart.AddIngredient(ResourceType.MetalMaterial, 4);
            makeBasicPart.SetOutput(ResourceType.BasicPart, 1);
            RecipeDefinitions["MakeBasicPart"] = makeBasicPart;

            var makeAdvancedPart = new RecipeDefinition("Make Advanced Part", 12f, BuildingType.AdvancedFactory);
            makeAdvancedPart.AddIngredient(ResourceType.BasicPart, 4);
            makeAdvancedPart.SetOutput(ResourceType.AdvancedPart, 1);
            RecipeDefinitions["MakeAdvancedPart"] = makeAdvancedPart;
        }

        public static ResourceDefinition GetResource(ResourceType type)
        {
            return ResourceDefinitions.TryGetValue(type, out var def) ? def : null;
        }

        public static BuildingDefinition GetBuilding(BuildingType type)
        {
            return BuildingDefinitions.TryGetValue(type, out var def) ? def : null;
        }

        public static BoardDefinition GetBoard(BoardType type)
        {
            return BoardDefinitions.TryGetValue(type, out var def) ? def : null;
        }

        public static RecipeDefinition GetRecipe(string key)
        {
            return RecipeDefinitions.TryGetValue(key, out var def) ? def : null;
        }

        public static List<RecipeDefinition> GetRecipesForBuilding(BuildingType type)
        {
            var result = new List<RecipeDefinition>();
            foreach (var recipe in RecipeDefinitions.Values)
            {
                if (recipe.requiredBuilding == type)
                {
                    result.Add(recipe);
                }
            }
            return result;
        }

        public static List<BuildingDefinition> GetBuildingsByCategory(BuildingCategory category)
        {
            var result = new List<BuildingDefinition>();
            foreach (var building in BuildingDefinitions.Values)
            {
                if (building.category == category)
                {
                    result.Add(building);
                }
            }
            return result;
        }

        public static List<BoardDefinition> GetBoardsByCategory(BuildingCategory category)
        {
            var result = new List<BoardDefinition>();
            foreach (var board in BoardDefinitions.Values)
            {
                if (board.category == category)
                {
                    result.Add(board);
                }
            }
            return result;
        }

        public static Dictionary<ResourceType, ResourceDefinition> GetAllResources()
        {
            return ResourceDefinitions;
        }

        public static string GetCategoryName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Core: return "Core";
                case BuildingCategory.Power: return "Power";
                case BuildingCategory.Production: return "Production";
                case BuildingCategory.Logistics: return "Logistics";
                case BuildingCategory.Storage: return "Storage";
                case BuildingCategory.Special: return "Special";
                case BuildingCategory.Board: return "Board";
                default: return "Unknown";
            }
        }
    }
}
