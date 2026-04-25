using System.Collections.Generic;

namespace GameCore
{
    public struct RecipeIngredient
    {
        public ResourceType resourceType;
        public int amount;

        public RecipeIngredient(ResourceType resourceType, int amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }
    }

    public class RecipeDefinition
    {
        public string name;
        public List<RecipeIngredient> ingredients;
        public ResourceStack output;
        public float productionTime;
        public BuildingType requiredBuilding;

        public RecipeDefinition(string name, float productionTime, BuildingType requiredBuilding)
        {
            this.name = name;
            this.productionTime = productionTime;
            this.requiredBuilding = requiredBuilding;
            this.ingredients = new List<RecipeIngredient>();
            this.output = new ResourceStack();
        }

        public void AddIngredient(ResourceType type, int amount)
        {
            ingredients.Add(new RecipeIngredient(type, amount));
        }

        public void SetOutput(ResourceType type, int amount)
        {
            output = new ResourceStack(type, amount);
        }

        public bool HasEnoughIngredients(Dictionary<ResourceType, int> availableResources)
        {
            foreach (var ingredient in ingredients)
            {
                if (!availableResources.ContainsKey(ingredient.resourceType) || 
                    availableResources[ingredient.resourceType] < ingredient.amount)
                {
                    return false;
                }
            }
            return true;
        }
    }
}