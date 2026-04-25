using System.Collections.Generic;
using UnityEngine;
using GameCore;

namespace PowerSystem
{
    public class PowerManager : MonoBehaviour
    {
        public static PowerManager Instance { get; private set; }

        public float maxPowerStorage = 1000f;
        public float powerStorage = 500f;
        
        private List<PowerProducer> producers = new List<PowerProducer>();
        private List<PowerConsumer> consumers = new List<PowerConsumer>();

        public float TotalGenerated => CalculateTotalGeneration();
        public float TotalConsumed => CalculateTotalConsumption();
        public float NetPower => TotalGenerated - TotalConsumed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterProducer(PowerProducer producer)
        {
            if (!producers.Contains(producer))
            {
                producers.Add(producer);
            }
        }

        public void UnregisterProducer(PowerProducer producer)
        {
            producers.Remove(producer);
        }

        public void RegisterConsumer(PowerConsumer consumer)
        {
            if (!consumers.Contains(consumer))
            {
                consumers.Add(consumer);
            }
        }

        public void UnregisterConsumer(PowerConsumer consumer)
        {
            consumers.Remove(consumer);
        }

        public void UpdatePower(float deltaTime)
        {
            float generated = CalculateTotalGeneration() * deltaTime;
            float consumed = CalculateTotalConsumption() * deltaTime;

            powerStorage = Mathf.Clamp(powerStorage + generated - consumed, 0f, maxPowerStorage);

            bool hasEnoughPower = powerStorage >= consumed;
            foreach (var consumer in consumers)
            {
                consumer.SetPowerAvailable(hasEnoughPower);
            }
        }

        private float CalculateTotalGeneration()
        {
            float total = 0f;
            foreach (var producer in producers)
            {
                if (producer.IsActive())
                {
                    total += producer.GetPowerOutput();
                }
            }
            return total;
        }

        private float CalculateTotalConsumption()
        {
            float total = 0f;
            foreach (var consumer in consumers)
            {
                if (consumer.IsActive() && consumer.IsPowered())
                {
                    total += consumer.GetPowerInput();
                }
            }
            return total;
        }

        public bool HasEnoughPower(float amount)
        {
            return powerStorage >= amount;
        }

        public bool ConsumePower(float amount)
        {
            if (powerStorage >= amount)
            {
                powerStorage -= amount;
                return true;
            }
            return false;
        }

        public void AddPower(float amount)
        {
            powerStorage = Mathf.Clamp(powerStorage + amount, 0f, maxPowerStorage);
        }

        public float GetPowerPercentage()
        {
            return (powerStorage / maxPowerStorage) * 100f;
        }
    }
}