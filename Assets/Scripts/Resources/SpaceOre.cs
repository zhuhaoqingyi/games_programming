using UnityEngine;
using GameCore;

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

        public ResourceType resourceType;

        [Header("边界设置")]
        public float destroyDistance = 25f;
        public float protectedTime = 3f;

        private Vector3 moveDirection;
        private float currentSpeed;
        private float time;
        private bool isCollected;
        private float spawnTime;

        private void Awake()
        {
            Vector3 defaultDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0
            ).normalized;

            Initialize(defaultDirection);
            spawnTime = Time.time;
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

        private void Update()
        {
            if (isCollected) return;

            time += Time.deltaTime;

            transform.position += moveDirection * currentSpeed * Time.deltaTime;

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
            GameManager.Instance?.AddResource(resourceType, 1);
            Destroy(gameObject);
        }

        public bool IsCollected()
        {
            return isCollected;
        }
    }
}