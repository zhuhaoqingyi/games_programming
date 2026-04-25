using System.Collections.Generic;

namespace GameCore
{
    public static class DataConfig
    {
        public static Dictionary<ResourceType, ResourceDefinition> ResourceDefinitions = new Dictionary<ResourceType, ResourceDefinition>();
        public static Dictionary<BuildingType, BuildingDefinition> BuildingDefinitions = new Dictionary<BuildingType, BuildingDefinition>();
        public static Dictionary<string, RecipeDefinition> RecipeDefinitions = new Dictionary<string, RecipeDefinition>();

        static DataConfig()
        {
            InitializeResources();
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

        private static void InitializeBuildings()
        {
            var emergencyShelter = new BuildingDefinition(
                BuildingType.EmergencyShelter, "太空紧急避难仓", "玩家开局核心根基",
                width: 2, height: 2, powerConsumption: 0, powerProduction: 0,
                storageCapacity: 100);
            BuildingDefinitions[BuildingType.EmergencyShelter] = emergencyShelter;

            var miningPlatform = new BuildingDefinition(
                BuildingType.MiningPlatform, "太空漂浮采矿平台", "自动开采太空矿石和垃圾",
                width: 1, height: 1, powerConsumption: 10, powerProduction: 0);
            miningPlatform.costs.Add(new BuildingCost(ResourceType.SpaceOre, 20));
            BuildingDefinitions[BuildingType.MiningPlatform] = miningPlatform;

            var nuclearReactor = new BuildingDefinition(
                BuildingType.NuclearReactor, "太空核能发电模块", "消耗矿石发电",
                width: 2, height: 2, powerConsumption: 0, powerProduction: 50);
            nuclearReactor.costs.Add(new BuildingCost(ResourceType.SpaceOre, 50));
            BuildingDefinitions[BuildingType.NuclearReactor] = nuclearReactor;

            var solarArray = new BuildingDefinition(
                BuildingType.SolarArray, "太空太阳能发电阵列", "无限清洁能源",
                width: 3, height: 1, powerConsumption: 0, powerProduction: 30);
            solarArray.costs.Add(new BuildingCost(ResourceType.AlloyIngot, 30));
            solarArray.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 10));
            BuildingDefinitions[BuildingType.SolarArray] = solarArray;

            var storageDock = new BuildingDefinition(
                BuildingType.StorageDock, "太空仓储对接舱", "存储各类物资",
                width: 2, height: 1, powerConsumption: 2, powerProduction: 0,
                storageCapacity: 500);
            storageDock.costs.Add(new BuildingCost(ResourceType.SpaceOre, 30));
            BuildingDefinitions[BuildingType.StorageDock] = storageDock;

            var furnaceRefinery = new BuildingDefinition(
                BuildingType.FurnaceRefinery, "熔炉精炼厂", "矿石精炼成合金锭",
                width: 2, height: 1, powerConsumption: 15, powerProduction: 0,
                isProductionBuilding: true);
            furnaceRefinery.costs.Add(new BuildingCost(ResourceType.SpaceOre, 40));
            BuildingDefinitions[BuildingType.FurnaceRefinery] = furnaceRefinery;

            var partAssembly = new BuildingDefinition(
                BuildingType.PartAssembly, "零件组装厂", "合金锭制成机械零件",
                width: 2, height: 1, powerConsumption: 20, powerProduction: 0,
                isProductionBuilding: true);
            partAssembly.costs.Add(new BuildingCost(ResourceType.AlloyIngot, 30));
            partAssembly.costs.Add(new BuildingCost(ResourceType.SpaceDebris, 20));
            BuildingDefinitions[BuildingType.PartAssembly] = partAssembly;

            var advancedFactory = new BuildingDefinition(
                BuildingType.AdvancedFactory, "高级加工厂", "生产电子元件和高级合金",
                width: 3, height: 2, powerConsumption: 40, powerProduction: 0,
                isProductionBuilding: true);
            advancedFactory.costs.Add(new BuildingCost(ResourceType.AlloyIngot, 50));
            advancedFactory.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 30));
            BuildingDefinitions[BuildingType.AdvancedFactory] = advancedFactory;

            var conveyorBelt = new BuildingDefinition(
                BuildingType.ConveyorBelt, "传送带", "自动运输物资",
                width: 1, height: 1, powerConsumption: 1, powerProduction: 0);
            conveyorBelt.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 5));
            BuildingDefinitions[BuildingType.ConveyorBelt] = conveyorBelt;

            var sorter = new BuildingDefinition(
                BuildingType.Sorter, "分拣器", "分类不同材料",
                width: 1, height: 1, powerConsumption: 5, powerProduction: 0);
            sorter.costs.Add(new BuildingCost(ResourceType.MechanicalPart, 10));
            sorter.costs.Add(new BuildingCost(ResourceType.ElectronicComponent, 2));
            BuildingDefinitions[BuildingType.Sorter] = sorter;

            var shipAssembly = new BuildingDefinition(
                BuildingType.ShipAssembly, "飞船组装平台", "建造星际飞船",
                width: 5, height: 3, powerConsumption: 100, powerProduction: 0);
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
    }
}