using System.Collections.Generic;
using GameCore;

namespace ProductionSystem
{
    public class ProductionManager
    {
        private List<ProductionBuilding> productionBuildings = new List<ProductionBuilding>();

        public void RegisterProductionBuilding(ProductionBuilding building)
        {
            if (!productionBuildings.Contains(building))
            {
                productionBuildings.Add(building);
            }
        }

        public void UnregisterProductionBuilding(ProductionBuilding building)
        {
            productionBuildings.Remove(building);
        }

        public void UpdateProduction(float deltaTime)
        {
            foreach (var building in productionBuildings)
            {
                building.UpdateProduction(deltaTime);
            }
        }

        public List<RecipeDefinition> GetAvailableRecipes(BuildingType buildingType)
        {
            return DataConfig.GetRecipesForBuilding(buildingType);
        }

        public bool CanProduce(ProductionBuilding building, RecipeDefinition recipe)
        {
            if (!building.CanWork())
            {
                return false;
            }

            if (recipe.requiredBuilding != building.Type)
            {
                return false;
            }

            return recipe.HasEnoughIngredients(GameManager.Instance.GetAllResources());
        }
    }
}