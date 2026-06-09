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

        [Header("边界范围")]
        public float boundaryMin = -150f;       // 固定边界最小值
        public float boundaryMax = 150f;        // 固定边界最大值

        [Header("生成方向权重")]
        public float topWeight = 0.25f;
        public float bottomWeight = 0.25f;
        public float leftWeight = 0.25f;
        public float rightWeight = 0.25f;

        [Header("调试")]
        public bool enableDebug = true;

        private float timer;
        private Camera mainCamera;
        private float totalWeight;

        private void Awake()
        {
            mainCamera = Camera.main;
            CalculateTotalWeight();
            LogDebug($"ResourceSpawner 初始化完成，预制件数量: {resourcePrefabs?.Length ?? 0}");
        }

        private void CalculateTotalWeight()
        {
            totalWeight = 0;
            if (resourcePrefabs != null)
            {
                foreach (var item in resourcePrefabs)
                {
                    if (item.prefab != null)
                    {
                        totalWeight += item.spawnWeight;
                    }
                }
            }

            if (totalWeight <= 0)
            {
                totalWeight = 1f;
                LogDebug("警告: 没有配置预制件或权重为0");
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
            LogDebug($"当前矿石数量: {currentCount}/{maxResources}");

            if (currentCount >= maxResources)
            {
                LogDebug("已达到最大数量，跳过生成");
                return;
            }

            GameObject prefab = GetRandomResourcePrefab();
            if (prefab == null)
            {
                LogDebug("错误: 无法获取预制件");
                return;
            }

            SpawnSide side = GetRandomSide();
            Vector3 worldSpawnPos = GetSpawnPosition(side);
            Vector3 moveDir = GetMoveDirection(side);

            LogDebug($"生成矿石：世界位置={worldSpawnPos}, 方向={moveDir}, 预制件={prefab.name}");

            // 直接在世界坐标生成，不作为容器子物体，避免容器偏移影响
            GameObject resource = Instantiate(prefab, worldSpawnPos, Quaternion.identity);

            SpaceOre ore = resource.GetComponent<SpaceOre>();
            if (ore != null)
            {
                ore.Initialize(moveDir);
                LogDebug("矿石初始化成功");
            }
            else
            {
                LogDebug("错误: 预制件没有SpaceOre组件");
            }
        }

        private GameObject GetRandomResourcePrefab()
        {
            if (resourcePrefabs == null || resourcePrefabs.Length == 0)
            {
                Debug.LogWarning("[ResourceSpawner] 没有配置资源预制件!");
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
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    LogDebug("错误: 找不到主摄像机");
                    return Vector3.zero;
                }
            }

            float screenWidth = mainCamera.orthographicSize * mainCamera.aspect;
            float screenHeight = mainCamera.orthographicSize;

            LogDebug($"屏幕尺寸: width={screenWidth}, height={screenHeight}");

            switch (side)
            {
                case SpawnSide.Top:
                    return new Vector3(
                        Mathf.Clamp(Random.Range(-screenWidth - spawnDistance, screenWidth + spawnDistance), boundaryMin, boundaryMax),
                        boundaryMax,
                        0
                    );
                case SpawnSide.Bottom:
                    return new Vector3(
                        Mathf.Clamp(Random.Range(-screenWidth - spawnDistance, screenWidth + spawnDistance), boundaryMin, boundaryMax),
                        boundaryMin,
                        0
                    );
                case SpawnSide.Left:
                    return new Vector3(
                        boundaryMin,
                        Mathf.Clamp(Random.Range(-screenHeight - spawnDistance, screenHeight + spawnDistance), boundaryMin, boundaryMax),
                        0
                    );
                default:
                    return new Vector3(
                        boundaryMax,
                        Mathf.Clamp(Random.Range(-screenHeight - spawnDistance, screenHeight + spawnDistance), boundaryMin, boundaryMax),
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

        private void LogDebug(string message)
        {
            if (enableDebug)
            {
                Debug.Log($"[ResourceSpawner] {message}");
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