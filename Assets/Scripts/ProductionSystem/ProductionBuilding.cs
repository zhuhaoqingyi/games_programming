using UnityEngine;
using GameCore;
using PowerSystem;

namespace ProductionSystem
{
    public class ProductionBuilding : GridSystem.BuildingComponent
    {
        [SerializeField] private RecipeDefinition currentRecipe;
        [SerializeField] private float productionProgress = 0f;
        [SerializeField] private bool isProducing = false;
        
        private PowerConsumer powerConsumer;

        protected override void Awake()
        {
            base.Awake();
            powerConsumer = GetComponent<PowerConsumer>();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (CanWork())
            {
                if (isProducing)
                {
                    productionProgress += deltaTime;
                    
                    if (productionProgress >= currentRecipe.productionTime)
                    {
                        CompleteProduction();
                    }
                }
            }
        }

        public void UpdateProduction(float deltaTime)
        {
            UpdateBuilding(deltaTime);
        }

        public override bool CanWork()
        {
            return base.CanWork() && powerConsumer != null && powerConsumer.CanWork();
        }

        public void StartProduction(RecipeDefinition recipe)
        {
            if (recipe == null) return;
            
            if (!CanProduceRecipe(recipe))
            {
                return;
            }

            currentRecipe = recipe;
            productionProgress = 0f;
            isProducing = true;
            
            ConsumeIngredients();
        }

        public void StopProduction()
        {
            isProducing = false;
            productionProgress = 0f;
        }

        private bool CanProduceRecipe(RecipeDefinition recipe)
        {
            if (!CanWork())
            {
                return false;
            }

            if (recipe.requiredBuilding != buildingType)
            {
                return false;
            }

            return GameManager.Instance.HasEnoughResource(recipe);
        }

        private void ConsumeIngredients()
        {
            if (currentRecipe == null) return;

            foreach (var ingredient in currentRecipe.ingredients)
            {
                GameManager.Instance.RemoveResource(ingredient.resourceType, ingredient.amount);
            }
        }

        private void CompleteProduction()
        {
            if (currentRecipe == null) return;

            GameManager.Instance.AddResource(currentRecipe.output.type, currentRecipe.output.amount);
            
            productionProgress = 0f;
            
            if (isProducing)
            {
                if (CanProduceRecipe(currentRecipe))
                {
                    ConsumeIngredients();
                }
                else
                {
                    StopProduction();
                }
            }
        }

        public float GetProductionProgress()
        {
            if (currentRecipe == null) return 0f;
            return (productionProgress / currentRecipe.productionTime) * 100f;
        }

        public bool IsProducing()
        {
            return isProducing;
        }

        public RecipeDefinition GetCurrentRecipe()
        {
            return currentRecipe;
        }
    }
}