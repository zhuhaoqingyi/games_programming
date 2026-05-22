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
                ResourceType.SpaceOre, "太空矿石", "漂浮在太空中的天然矿石", 2.5f);
            ResourceDefinitions[ResourceType.SpaceDebris] = new ResourceDefinition(
                ResourceType.SpaceDebris, "太空垃圾", "废弃星际文明遗留的残骸", 1.2f);
            ResourceDefinitions[ResourceType.AlloyIngot] = new ResourceDefinition(
                ResourceType.AlloyIngot, "太空合金锭", "精炼后的高级金属材料", 3.0f);
            ResourceDefinitions[ResourceType.MechanicalPart] = new ResourceDefinition(
                ResourceType.MechanicalPart, "星际机械零件", "精密加工的机械部件", 1.5f);
            ResourceDefinitions[ResourceType.ElectronicComponent] = new ResourceDefinition(
                ResourceType.ElectronicComponent, "电子航天元件", "高科技电子元件", 0.8f);
            ResourceDefinitions[ResourceType.AdvancedAlloy] = new ResourceDefinition(
                ResourceType.AdvancedAlloy, "高级合金", "用于建造飞船的顶级材料", 4.0f);
        }

        private static void InitializeBoards()
        {
            var basicBoard = new BoardDefinition(
                BoardType.BasicBoard, BuildingCategory.Board, "基础太空板", "最基础的太空平台模块",
                prefabPath: "Prefabs/Boards/BasicBoard", iconPath: "Icons/Boards/BasicBoard");
            basicBoard.costs.Add(new BoardCost(ResourceType.SpaceDebris, 5));
            BoardDefinitions[BoardType.BasicBoard] = basicBoard;

            var reinforcedBoard = new BoardDefinition(
                BoardType.ReinforcedBoard, BuildingCategory.Board, "加固太空板", "加固结构，更坚固",
                prefabPath: "Prefabs/Boards/ReinforcedBoard", iconPath: "Icons/Boards/ReinforcedBoard");
            reinforcedBoard.costs.Add(new BoardCost(ResourceType.AlloyIngot, 10));
            reinforcedBoard.costs.Add(new BoardCost(ResourceType.SpaceDebris, 10));
            BoardDefinitions[BoardType.ReinforcedBoard] = reinforcedBoard;

            var advancedBoard = new BoardDefinition(
                BoardType.AdvancedBoard, BuildingCategory.Board, "高级太空板", "高级复合太空板",
                prefabPath: "Prefabs/Boards/AdvancedBoard", iconPath: "Icons/Boards/AdvancedBoard");
            advancedBoard.costs.Add(new BoardCost(ResourceType.AdvancedAlloy, 5));
            advancedBoard.costs.Add(new BoardCost(ResourceType.MechanicalPart, 10));
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
                BuildingType.EmergencyShelter, BuildingCategory.Core, "太空紧急避难仓", "玩家开局核心根基",
                width: 2, height: 2, powerConsumption: 0, powerProduction: 0,
                storageCapacity: 100, prefabPath: "Prefabs/Buildings/EmergencyShelter", iconPath: "Icons/Buildings/EmergencyShelter");
            BuildingDefinitions[BuildingType.EmergencyShelter] = emergencyShelter;

            var miningPlatform = new BuildingDefinition(
                BuildingType.MiningPlatform, BuildingCategory.Production, "太空漂浮采矿平台", "自动开采太空矿石和垃圾",
                width: 2, height: 2, functionalAreaWidth: 2, functionalAreaHeight: 2, powerConsumption: 10, powerProduction: 0,
                prefabPath: "Prefabs/Buildings/MiningPlatform", iconPath: "Icons/Buildings/MiningPlatform");
            miningPlatform.costs.Add(new BuildingCost(ResourceType.SpaceOre, 20));
            BuildingDefinitions[BuildingType.MiningPlatform] = miningPlatform;

            var nuclearReactor = new BuildingDefinition(
                BuildingType.NuclearReactor, BuildingCategory.Power, "太空核能发电模块", "消耗矿石发电",
                width: 2, height: 2, powerConsumption: 0, powerProduction: 50,
                prefabPath: "Prefabs/Buildings/NuclearReactor", iconPath: "Icons/Buildings/NuclearReactor");
            nuclearReactor.costs.Add(new BuildingCost(ResourceType.SpaceOre, 50));
            BuildingDefinitions[BuildingType.NuclearReactor] = nuclearReactor;

            var solarArray = new BuildingDefinition(
                BuildingType.SolarArray, BuildingCategory.Power, "太空太阳能发电阵列", "无限清洁能源",
                width: 3, height: 1, powerConsumption: 0, powerProduction: 30,
                prefabPath: "Prefabs/Buildings/SolarArray", iconPath: "Icons/Buildings/SolarArray");
            solarArray.costs.Add(new BuildingCost(ResourceType.AlloyIngot, 30));
            solarArray.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 10));
            BuildingDefinitions[BuildingType.SolarArray] = solarArray;

            var storageDock = new BuildingDefinition(
                BuildingType.StorageDock, BuildingCategory.Storage, "太空仓储对接舱", "存储各类物资",
                width: 2, height: 1, powerConsumption: 2, powerProduction: 0,
                storageCapacity: 500, prefabPath: "Prefabs/Buildings/StorageDock", iconPath: "Icons/Buildings/StorageDock");
            storageDock.costs.Add(new BuildingCost(ResourceType.SpaceOre, 30));
            BuildingDefinitions[BuildingType.StorageDock] = storageDock;

            var furnaceRefinery = new BuildingDefinition(
                BuildingType.FurnaceRefinery, BuildingCategory.Production, "熔炉精炼厂", "矿石精炼成合金锭",
                width: 2, height: 2, powerConsumption: 15, powerProduction: 0,
                isProductionBuilding: true, prefabPath: "Prefabs/Buildings/FurnaceRefinery", iconPath: "Icons/Buildings/FurnaceRefinery");
            furnaceRefinery.costs.Add(new BuildingCost(ResourceType.SpaceOre, 40));
            BuildingDefinitions[BuildingType.FurnaceRefinery] = furnaceRefinery;

            var partAssembly = new BuildingDefinition(
                BuildingType.PartAssembly, BuildingCategory.Production, "零件组装厂", "合金锭制成机械零件",
                width: 2, height: 1, powerConsumption: 20, powerProduction: 0,
                isProductionBuilding: true, prefabPath: "Prefabs/Buildings/PartAssembly", iconPath: "Icons/Buildings/PartAssembly");
            partAssembly.costs.Add(new BuildingCost(ResourceType.AlloyIngot, 30));
            partAssembly.costs.Add(new BuildingCost(ResourceType.SpaceDebris, 20));
            BuildingDefinitions[BuildingType.PartAssembly] = partAssembly;

            var advancedFactory = new BuildingDefinition(
                BuildingType.AdvancedFactory, BuildingCategory.Production, "高级加工厂", "生产电子元件和高级合金",
                width: 3, height: 2, powerConsumption: 40, powerProduction: 0,
                isProductionBuilding: true, prefabPath: "Prefabs/Buildings/AdvancedFactory", iconPath: "Icons/Buildings/AdvancedFactory");
            advancedFactory.costs.Add(new BuildingCost(ResourceType.AlloyIngot, 50));
            advancedFactory.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 30));
            BuildingDefinitions[BuildingType.AdvancedFactory] = advancedFactory;

            var conveyorBelt = new BuildingDefinition(
                BuildingType.ConveyorBelt, BuildingCategory.Logistics, "传送带", "自动运输物资",
                width: 1, height: 1, powerConsumption: 1, powerProduction: 0,
                prefabPath: "Prefabs/Buildings/ConveyorBelt", iconPath: "Icons/Buildings/ConveyorBelt");
            conveyorBelt.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 5));
            BuildingDefinitions[BuildingType.ConveyorBelt] = conveyorBelt;

            var sorter = new BuildingDefinition(
                BuildingType.Sorter, BuildingCategory.Logistics, "分拣器", "分类不同材料",
                width: 1, height: 1, powerConsumption: 5, powerProduction: 0,
                prefabPath: "Prefabs/Buildings/Sorter", iconPath: "Icons/Buildings/Sorter");
            sorter.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 10));
            sorter.costs.Add(new BuildingCost(ResourceType.ElectronicComponent, 2));
            BuildingDefinitions[BuildingType.Sorter] = sorter;

            var shipAssembly = new BuildingDefinition(
                BuildingType.ShipAssembly, BuildingCategory.Special, "飞船组装平台", "建造星际飞船",
                width: 5, height: 3, powerConsumption: 100, powerProduction: 0,
                prefabPath: "Prefabs/Buildings/ShipAssembly", iconPath: "Icons/Buildings/ShipAssembly");
            shipAssembly.costs.Add(new BuildingCost(ResourceType.AdvancedAlloy, 100));
            shipAssembly.costs.Add(new BuildingCost(ResourceType.ElectronicComponent, 50));
            shipAssembly.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 80));
            BuildingDefinitions[BuildingType.ShipAssembly] = shipAssembly;
        }

        private static void InitializeRecipes()
        {
            var refineAlloy = new RecipeDefinition("精炼合金锭", 5f, BuildingType.FurnaceRefinery);
            refineAlloy.AddIngredient(ResourceType.SpaceOre, 2);
            refineAlloy.AddIngredient(ResourceType.SpaceDebris, 1);
            refineAlloy.SetOutput(ResourceType.AlloyIngot, 1);
            RecipeDefinitions["RefineAlloy"] = refineAlloy;

            var makePart = new RecipeDefinition("制造机械零件", 8f, BuildingType.PartAssembly);
            makePart.AddIngredient(ResourceType.AlloyIngot, 2);
            makePart.SetOutput(ResourceType.MechanicalPart, 1);
            RecipeDefinitions["MakePart"] = makePart;

            var makeElectronic = new RecipeDefinition("制造电子元件", 12f, BuildingType.AdvancedFactory);
            makeElectronic.AddIngredient(ResourceType.MechanicalPart, 2);
            makeElectronic.AddIngredient(ResourceType.AlloyIngot, 1);
            makeElectronic.SetOutput(ResourceType.ElectronicComponent, 1);
            RecipeDefinitions["MakeElectronic"] = makeElectronic;

            var makeAdvancedAlloy = new RecipeDefinition("制造高级合金", 15f, BuildingType.AdvancedFactory);
            makeAdvancedAlloy.AddIngredient(ResourceType.AlloyIngot, 3);
            makeAdvancedAlloy.AddIngredient(ResourceType.SpaceDebris, 5);
            makeAdvancedAlloy.SetOutput(ResourceType.AdvancedAlloy, 1);
            RecipeDefinitions["MakeAdvancedAlloy"] = makeAdvancedAlloy;
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

        public static string GetCategoryName(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Core: return "核心设施";
                case BuildingCategory.Power: return "能源设施";
                case BuildingCategory.Production: return "生产设施";
                case BuildingCategory.Logistics: return "物流设施";
                case BuildingCategory.Storage: return "仓储设施";
                case BuildingCategory.Special: return "特殊设施";
                case BuildingCategory.Board: return "基础平台";
                default: return "未知";
            }
        }
    }
}