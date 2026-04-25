namespace GameCore
{
    public struct ResourceStack
    {
        public ResourceType type;
        public int amount;

        public ResourceStack(ResourceType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }

        public bool IsValid()
        {
            return type != ResourceType.None && amount > 0;
        }

        public bool CanAdd(int addAmount)
        {
            return amount + addAmount >= 0;
        }

        public void Add(int addAmount)
        {
            amount += addAmount;
        }

        public bool TryConsume(int consumeAmount)
        {
            if (amount >= consumeAmount)
            {
                amount -= consumeAmount;
                return true;
            }
            return false;
        }
    }

    public class ResourceDefinition
    {
        public ResourceType type;
        public string name;
        public string description;
        public float density;
        public bool isFluid;

        public ResourceDefinition(ResourceType type, string name, string description, float density = 1f, bool isFluid = false)
        {
            this.type = type;
            this.name = name;
            this.description = description;
            this.density = density;
            this.isFluid = isFluid;
        }
    }
}