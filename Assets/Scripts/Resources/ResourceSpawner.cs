using UnityEngine;

namespace GameResources
{
    [System.Serializable]
    public class ResourceSpawnItem
    {
        public GameObject prefab;
        public float spawnWeight = 1f;
    }

    public class ResourceSpawner : MonoBehaviour
    {
        [Header("生成设置")]
        public ResourceSpawnItem[] resourcePrefabs;
        public float spawnInterval = 6f;
        public int maxResources = 25;
        public float spawnDistance = 15f;
        
        [Header("生成方向权重")]
        public float topWeight = 0.25f;
        public float bottomWeight = 0.25f;
        public float leftWeight = 0.25f;
        public float rightWeight = 0.25f;
        
        private float timer;
        private Camera mainCamera;
        private float totalWeight;

        private void Awake()
        {
            mainCamera = Camera.main;
            CalculateTotalWeight();
        }

        private void CalculateTotalWeight()
        {
            totalWeight = 0;
            foreach (var item in resourcePrefabs)
            {
                if (item.prefab != null)
                {
                    totalWeight += item.spawnWeight;
                }
            }
            
            if (totalWeight <= 0)
            {
                totalWeight = 1f;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            
            if (timer >= spawnInterval)
            {
                timer = 0;
                SpawnResource();
            }
        }

        private void SpawnResource()
        {
            int currentCount = FindObjectsOfType<SpaceOre>().Length;
            if (currentCount >= maxResources) return;
            
            GameObject prefab = GetRandomResourcePrefab();
            if (prefab == null) return;
            
            SpawnSide side = GetRandomSide();
            Vector3 spawnPos = GetSpawnPosition(side);
            Vector3 moveDir = GetMoveDirection(side);
            
            GameObject resource = Instantiate(prefab, spawnPos, Quaternion.identity);
            SpaceOre ore = resource.GetComponent<SpaceOre>();
            if (ore != null)
            {
                ore.Initialize(moveDir);
            }
        }

        private GameObject GetRandomResourcePrefab()
        {
            if (resourcePrefabs == null || resourcePrefabs.Length == 0)
            {
                Debug.LogWarning("No resource prefabs assigned!");
                return null;
            }
            
            float rand = Random.value * totalWeight;
            float cumulative = 0;
            
            foreach (var item in resourcePrefabs)
            {
                if (item.prefab == null) continue;
                
                cumulative += item.spawnWeight;
                if (rand < cumulative)
                {
                    return item.prefab;
                }
            }
            
            return resourcePrefabs[0].prefab;
        }

        private SpawnSide GetRandomSide()
        {
            float rand = Random.value;
            float cumulative = 0;
            
            cumulative += topWeight;
            if (rand < cumulative) return SpawnSide.Top;
            
            cumulative += bottomWeight;
            if (rand < cumulative) return SpawnSide.Bottom;
            
            cumulative += leftWeight;
            if (rand < cumulative) return SpawnSide.Left;
            
            return SpawnSide.Right;
        }

        private Vector3 GetSpawnPosition(SpawnSide side)
        {
            float screenWidth = mainCamera.orthographicSize * mainCamera.aspect;
            float screenHeight = mainCamera.orthographicSize;
            
            switch (side)
            {
                case SpawnSide.Top:
                    return new Vector3(
                        Random.Range(-screenWidth - spawnDistance, screenWidth + spawnDistance),
                        screenHeight + spawnDistance,
                        0
                    );
                case SpawnSide.Bottom:
                    return new Vector3(
                        Random.Range(-screenWidth - spawnDistance, screenWidth + spawnDistance),
                        -screenHeight - spawnDistance,
                        0
                    );
                case SpawnSide.Left:
                    return new Vector3(
                        -screenWidth - spawnDistance,
                        Random.Range(-screenHeight - spawnDistance, screenHeight + spawnDistance),
                        0
                    );
                default:
                    return new Vector3(
                        screenWidth + spawnDistance,
                        Random.Range(-screenHeight - spawnDistance, screenHeight + spawnDistance),
                        0
                    );
            }
        }

        private Vector3 GetMoveDirection(SpawnSide side)
        {
            switch (side)
            {
                case SpawnSide.Top:
                    return Vector3.down;
                case SpawnSide.Bottom:
                    return Vector3.up;
                case SpawnSide.Left:
                    return Vector3.right;
                default:
                    return Vector3.left;
            }
        }

        private enum SpawnSide
        {
            Top,
            Bottom,
            Left,
            Right
        }
    }
}