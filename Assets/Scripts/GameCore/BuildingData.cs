using System.Collections.Generic;

namespace GameCore
{
    public struct BuildingCost
    {
        public ResourceType resourceType;
        public int amount;

        public BuildingCost(ResourceType resourceType, int amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }
    }

    public class BuildingDefinition
    {
        public BuildingType type;
        public BuildingCategory category;
        public string name;
        public string description;
        public int width;
        public int height;
        public int functionalAreaWidth;
        public int functionalAreaHeight;
        public BuildDirection direction;
        public int powerConsumption;
        public int powerProduction;
        public int storageCapacity;
        public bool isProductionBuilding;
        public bool isBoard;
        public List<BuildingCost> costs;
        public string prefabPath;
        public string iconPath;

        public BuildingDefinition(BuildingType type, BuildingCategory category, string name, string description, 
            int width = 1, int height = 1, int functionalAreaWidth = 0, int functionalAreaHeight = 0,
            BuildDirection direction = BuildDirection.North,
            int powerConsumption = 0, int powerProduction = 0,
            int storageCapacity = 0, bool isProductionBuilding = false, bool isBoard = false, string prefabPath = "", string iconPath = "")
        {
            this.type = type;
            this.category = category;
            this.name = name;
            this.description = description;
            this.width = width;
            this.height = height;
            this.functionalAreaWidth = functionalAreaWidth;
            this.functionalAreaHeight = functionalAreaHeight;
            this.direction = direction;
            this.powerConsumption = powerConsumption;
            this.powerProduction = powerProduction;
            this.storageCapacity = storageCapacity;
            this.isProductionBuilding = isProductionBuilding;
            this.isBoard = isBoard;
            this.prefabPath = prefabPath;
            this.iconPath = iconPath;
            this.costs = new List<BuildingCost>();
        }

        public bool HasCost()
        {
            return costs != null && costs.Count > 0;
        }

        public bool CanAfford(Dictionary<ResourceType, int> inventory)
        {
            foreach (var cost in costs)
            {
                if (!inventory.ContainsKey(cost.resourceType) || inventory[cost.resourceType] < cost.amount)
                {
                    return false;
                }
            }
            return true;
        }
    }
}