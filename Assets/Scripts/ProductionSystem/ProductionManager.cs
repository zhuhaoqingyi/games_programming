using UnityEngine;
using System.Collections.Generic;

namespace ProductionSystem
{
    public class ProductionManager
    {
        private List<Productor> productors = new List<Productor>();

        public void RegisterProductor(Productor productor)
        {
            if (!productors.Contains(productor))
            {
                productors.Add(productor);
            }
        }

        public void UnregisterProductor(Productor productor)
        {
            productors.Remove(productor);
        }
    }
}
