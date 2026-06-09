using UnityEngine;

namespace GameResources
{
    public class SpaceOre : MonoBehaviour
    {
        [Header("漂浮设置")]
        public float baseSpeed = 0.8f;
        public float speedVariation = 0.4f;
        public float rotationSpeed = 15f;
        public float bobAmplitude = 0.15f;
        public float bobFrequency = 2f;

        [Header("边界设置")]
        public float boundaryMin = -75f;       // 边界最小值
        public float boundaryMax = 75f;        // 边界最大值
        public float protectedTime = 3f;

        [Header("伤害设置")]
        public int damageToBuilding = 1;

        private Vector3 moveDirection;
        private float currentSpeed;
        private float time;
        private bool isCollected;
        private float spawnTime;
        private CircleCollider2D oreCollider;
        private Rigidbody2D rb;

        private void Awake()
        {
            SetupCollider();
            SetupRigidbody();

            Vector3 defaultDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0
            ).normalized;

            Initialize(defaultDirection);
            spawnTime = Time.time;
        }

        private void SetupRigidbody()
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.isKinematic = true;
            rb.gravityScale = 0;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        private void SetupCollider()
        {
            oreCollider = gameObject.GetComponent<CircleCollider2D>();
            if (oreCollider == null)
            {
                oreCollider = gameObject.AddComponent<CircleCollider2D>();
                oreCollider.radius = 0.5f;
            }
            oreCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            ProductionSystem.BuildingBase building = other.GetComponent<ProductionSystem.BuildingBase>();

            if (building != null)
            {
                building.TakeDamage(damageToBuilding);
                Collect();
                return;
            }
        }

        public void Initialize(Vector3 spawnDirection)
        {
            Vector3 deviation = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f),
                0
            );

            moveDirection = (spawnDirection.normalized + deviation).normalized;
            currentSpeed = baseSpeed + Random.Range(-speedVariation, speedVariation);
            isCollected = false;
            spawnTime = Time.time;
        }

        private void Update()
        {
            if (isCollected) return;

            time += Time.deltaTime;

            // 直接移动 transform 以支持碰撞检测（Rigidbody2D kinematic + trigger collider 可以触发 OnTriggerEnter2D）
            Vector3 worldMove = moveDirection * currentSpeed * Time.deltaTime;
            transform.position += worldMove;

            // Rotation
            transform.localEulerAngles += new Vector3(0, 0, rotationSpeed * Time.deltaTime);

            // Bobbing on Z axis
            float bob = Mathf.Sin(time * bobFrequency) * bobAmplitude;
            Vector3 worldPos = transform.position;
            worldPos.z = bob;
            transform.position = worldPos;

            CheckBoundary();
        }

        private void CheckBoundary()
        {
            if (Time.time - spawnTime < protectedTime)
                return;

            // 使用世界坐标进行边界检测，固定范围 [-150, 150]，不随容器移动而变化
            Vector3 worldPos = transform.position;

            if (worldPos.x < boundaryMin || worldPos.x > boundaryMax ||
                worldPos.y < boundaryMin || worldPos.y > boundaryMax)
            {
                Destroy(gameObject);
            }
        }

        public void Collect()
        {
            isCollected = true;
            Destroy(gameObject);
        }

        public bool IsCollected()
        {
            return isCollected;
        }
    }
}
