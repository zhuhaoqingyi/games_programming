using System.Collections.Generic;
using UnityEngine;
using GameCore;

namespace PowerSystem
{
    public class PowerManager : MonoBehaviour
    {
        public static PowerManager Instance { get; private set; }

        private List<PowerProducer> producers = new List<PowerProducer>();
        private List<PowerConsumer> consumers = new List<PowerConsumer>();

        public float TotalGenerated { get; private set; }
        public float TotalConsumed { get; private set; }
        public float TotalDemand { get; private set; }
        public float NetPower => TotalGenerated - TotalConsumed;
        public bool IsPowerSatisfied => TotalGenerated >= TotalDemand;

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

        private void Update()
        {
            UpdatePower();
        }

        public void RegisterProducer(PowerProducer producer)
        {
            if (producer != null && !producers.Contains(producer))
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
            if (consumer != null && !consumers.Contains(consumer))
            {
                consumers.Add(consumer);
            }
        }

        public void UnregisterConsumer(PowerConsumer consumer)
        {
            consumers.Remove(consumer);
        }

        public void UpdatePower()
        {
            TotalGenerated = 0f;
            foreach (var producer in producers)
            {
                if (producer.IsActive())
                {
                    TotalGenerated += producer.GetPowerOutput();
                }
            }

            float totalDemand = 0f;
            foreach (var consumer in consumers)
            {
                if (consumer.IsActive())
                {
                    totalDemand += consumer.GetPowerInput();
                }
            }
            TotalDemand = totalDemand;

            if (TotalGenerated >= totalDemand)
            {
                TotalConsumed = totalDemand;
                foreach (var consumer in consumers)
                {
                    if (consumer.IsActive())
                    {
                        consumer.SetPowerAvailable(true);
                    }
                }
            }
            else
            {
                var sortedConsumers = new List<PowerConsumer>(consumers);
                sortedConsumers.Sort((a, b) =>
                {
                    int pA = a.Priority;
                    int pB = b.Priority;
                    if (pA != pB) return pA.CompareTo(pB);
                    return a.GetInstanceID().CompareTo(b.GetInstanceID());
                });

                float remainingPower = TotalGenerated;
                TotalConsumed = 0f;

                foreach (var consumer in sortedConsumers)
                {
                    if (!consumer.IsActive())
                    {
                        consumer.SetPowerAvailable(false);
                        continue;
                    }

                    float needed = consumer.GetPowerInput();
                    if (remainingPower >= needed)
                    {
                        remainingPower -= needed;
                        TotalConsumed += needed;
                        consumer.SetPowerAvailable(true);
                    }
                    else
                    {
                        consumer.SetPowerAvailable(false);
                    }
                }
            }
        }

        public bool HasEnoughPower(float amount)
        {
            return NetPower >= amount;
        }

        public float GetPowerPercentage()
        {
            if (TotalConsumed <= 0f) return 100f;
            return (TotalGenerated / TotalConsumed) * 100f;
        }
    }
}
