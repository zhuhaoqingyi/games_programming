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
        public float destroyDistance = 25f;
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
            SetupRigidbody();
            SetupCollider();

            Vector3 defaultDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0
            ).normalized;

            Initialize(defaultDirection);
            spawnTime = Time.time;
            Debug.Log($"[SpaceOre] {name} 已生成，方向: {moveDirection}, 速度: {currentSpeed}");
        }

        private void Start()
        {
            if (rb != null)
            {
                rb.velocity = (Vector2)moveDirection * currentSpeed;
                Debug.Log($"[SpaceOre] {name} Start: 设置速度 = {rb.velocity}");
            }
        }

        private void SetupRigidbody()
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                Debug.Log($"[SpaceOre] {name} 添加 Rigidbody2D");
            }
            rb.isKinematic = false;
            rb.gravityScale = 0;
            rb.drag = 0;
            rb.angularDrag = 0;
            Debug.Log($"[SpaceOre] {name} Rigidbody2D 设置: isKinematic={rb.isKinematic}");
        }

        private void SetupCollider()
        {
            oreCollider = gameObject.GetComponent<CircleCollider2D>();
            if (oreCollider == null)
            {
                oreCollider = gameObject.AddComponent<CircleCollider2D>();
                oreCollider.radius = 0.5f;
                Debug.Log($"[SpaceOre] {name} 添加 CircleCollider2D");
            }
            oreCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected)
            {
                Debug.Log($"[SpaceOre] {name} 已被采集，忽略碰撞");
                return;
            }

            Debug.Log($"[SpaceOre] {name} 检测到碰撞: {other.gameObject.name}");

            ProductionSystem.MiningCollector collector = other.GetComponent<ProductionSystem.MiningCollector>();
            ProductionSystem.BuildingBase building = other.GetComponent<ProductionSystem.BuildingBase>();

            if (collector != null)
            {
                Debug.Log($"[SpaceOre] {name} 接触 Collector，被采集");
                Collect();
                return;
            }

            if (building != null)
            {
                building.TakeDamage(damageToBuilding);
                Debug.Log($"[SpaceOre] {name} 接触建筑，造成 {damageToBuilding} 点伤害");
                Collect();
                return;
            }

            Debug.Log($"[SpaceOre] {name} 碰撞对象不是 Collector 或 Building");
        }

        public Vector3 GetPosition()
        {
            return transform.position;
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

        private void FixedUpdate()
        {
            if (isCollected || rb == null) return;

            rb.velocity = (Vector2)moveDirection * currentSpeed;
        }

        private void Update()
        {
            if (isCollected) return;

            time += Time.deltaTime;

            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            float bob = Mathf.Sin(time * bobFrequency) * bobAmplitude;
            Vector3 pos = transform.position;
            pos.z = bob;
            transform.position = pos;

            CheckBoundary();
        }

        private void CheckBoundary()
        {
            if (Time.time - spawnTime < protectedTime)
                return;

            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            float screenWidth = mainCam.orthographicSize * mainCam.aspect;
            float screenHeight = mainCam.orthographicSize;

            Vector3 pos = transform.position;

            if (Mathf.Abs(pos.x) > screenWidth + destroyDistance ||
                Mathf.Abs(pos.y) > screenHeight + destroyDistance)
            {
                Destroy(gameObject);
            }
        }

        public void Collect()
        {
            isCollected = true;
            Debug.Log($"[SpaceOre] {name} 被采集，销毁对象");
            Destroy(gameObject);
        }

        public bool IsCollected()
        {
            return isCollected;
        }
    }
}