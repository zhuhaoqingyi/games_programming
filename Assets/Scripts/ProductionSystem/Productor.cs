using UnityEngine;
using GameCore;
using PowerSystem;
using GridSystem;
using System.Linq;

namespace ProductionSystem
{
    /// <summary>
    /// 产品生产脚本 - 挂载到生产建筑上
    /// 工作逻辑：右键点击建筑切换开关（正常=开启，暗淡+半透明=关闭），开启时有电+有原料就自动生产
    /// </summary>
    public class Productor : MonoBehaviour
    {
        [Header("Production Settings")]
        [Tooltip("原料类型")]
        public ResourceType inputResourceType;
        
        [Tooltip("原料数量")]
        public int inputAmount;
        
        [Tooltip("产出类型")]
        public ResourceType outputResourceType;
        
        [Tooltip("产出数量")]
        public int outputAmount;
        
        [Tooltip("生产时间（秒）")]
        public float productionTime;
        
        [Header("Visual Settings")]
        [Tooltip("关闭状态的亮度 (0=全黑, 1=不变)")]
        [Range(0f, 1f)]
        public float disabledBrightness = 0.3f;
        
        [Tooltip("关闭状态的透明度 (0.7=轻微透明, 1=不变)")]
        [Range(0.5f, 1f)]
        public float disabledAlpha = 0.75f;
        
        [Header("Power")]
        private PowerConsumer powerConsumer;
        
        // 运行状态
        private bool isOn = true;  // 默认开启
        private bool isProducing = false;
        private float currentProgress = 0f;
        private Color[] originalColors;
        private Color[] spriteOriginalColors;
        private Renderer[] renderers;
        private SpriteRenderer[] spriteRenderers;
        
        private void Awake()
        {
            powerConsumer = GetComponent<PowerConsumer>();
            
            // 收集所有Renderer
            renderers = GetComponentsInChildren<Renderer>();
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            
            // 记录原始颜色
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] is SpriteRenderer) continue;
                originalColors[i] = renderers[i].material.color;
            }
            
            // 记录SpriteRenderer原始颜色
            spriteOriginalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteOriginalColors[i] = spriteRenderers[i].color;
            }
        }
        
        private void Start()
        {
            UpdateVisual();
            Debug.Log($"[Productor] {gameObject.name} 启动 | 配方: {GetRecipeDescription()} | 初始状态: {(isOn ? "开启" : "关闭")}");
        }
        
        private void Update()
        {
            // 右键点击检测（与删除建筑相同的方式）
            if (Input.GetMouseButtonDown(1))
            {
                CheckProductorClick();
            }
            
            if (!isOn) return;
            
            bool hasPower = powerConsumer != null && powerConsumer.CanWork();
            if (!hasPower)
            {
                if (isProducing)
                {
                    Debug.Log($"[Productor] {gameObject.name} 生产暂停 | 原因: 电力不足");
                    isProducing = false;
                    currentProgress = 0f;
                }
                return;
            }
            
            bool hasInput = HasEnoughInput();
            if (!hasInput)
            {
                if (isProducing)
                {
                    Debug.Log($"[Productor] {gameObject.name} 生产暂停 | 原因: 原料不足 (库存: {GameManager.Instance.GetResourceAmount(inputResourceType)} < 需要: {inputAmount})");
                    isProducing = false;
                    currentProgress = 0f;
                }
                return;
            }
            
            if (!isProducing)
            {
                ConsumeInput();
                isProducing = true;
                currentProgress = 0f;
                
                Debug.Log($"[Productor] {gameObject.name} 开始生产 | 扣除原料: {inputAmount} {inputResourceType} | 当前库存: {GameManager.Instance.GetResourceAmount(inputResourceType)}");
            }
            
            currentProgress += Time.deltaTime;
            
            if (currentProgress >= productionTime)
            {
                CompleteProduction();
            }
        }
        
        /// <summary>
        /// 检测是否点击了当前建筑（通过网格系统，与删除建筑方式一致）
        /// </summary>
        private void CheckProductorClick()
        {
            if (GridManager.Instance == null) return;
            
            Camera mainCam = Camera.main;
            if (mainCam == null) return;
            
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            GridPosition gridPos = GridManager.Instance.WorldToGrid(mouseWorldPos);
            GridPosition originPos = GridManager.Instance.GetBuildingOrigin(gridPos);
            
            var bc = GetComponent<GridSystem.BuildingComponent>();
            if (bc != null && bc.GridPosition == originPos)
            {
                TogglePower();
            }
        }
        
        public void TogglePower()
        {
            isOn = !isOn;
            
            if (!isOn)
            {
                isProducing = false;
                currentProgress = 0f;
            }
            
            UpdateVisual();
            Debug.Log($"[Productor] {gameObject.name} 开关切换 → {(isOn ? "开启" : "关闭")} | 状态: {GetStatusString()}");
        }
        
        private void CompleteProduction()
        {
            if (GameManager.Instance != null)
            {
                int beforeAmount = GameManager.Instance.GetResourceAmount(outputResourceType);
                GameManager.Instance.AddResource(outputResourceType, outputAmount);
                int afterAmount = GameManager.Instance.GetResourceAmount(outputResourceType);
                
                Debug.Log($"[Productor] {gameObject.name} 生产完成 | 产出: +{outputAmount} {outputResourceType} | {beforeAmount} → {afterAmount}");
            }
            
            currentProgress = 0f;
            isProducing = false;
        }
        
        private bool HasEnoughInput()
        {
            if (GameManager.Instance == null) return false;
            return GameManager.Instance.GetResourceAmount(inputResourceType) >= inputAmount;
        }
        
        private void ConsumeInput()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RemoveResource(inputResourceType, inputAmount);
            }
        }
        
        /// <summary>
        /// 更新视觉效果：关闭时整体变暗淡+半透明
        /// </summary>
        private void UpdateVisual()
        {
            float brightness = isOn ? 1f : disabledBrightness;
            float alpha = isOn ? 1f : disabledAlpha;
            
            // 更新所有Renderer的颜色（跳过SpriteRenderer）
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] is SpriteRenderer) continue;
                
                Color original = originalColors[i];
                renderers[i].material.color = new Color(
                    original.r * brightness,
                    original.g * brightness,
                    original.b * brightness,
                    original.a * alpha
                );
            }
            
            // 单独更新SpriteRenderer（从原始颜色计算）
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Color original = spriteOriginalColors[i];
                spriteRenderers[i].color = new Color(
                    original.r * brightness,
                    original.g * brightness,
                    original.b * brightness,
                    original.a * alpha
                );
            }
        }
        
        private string GetStatusString()
        {
            if (!isOn) return "关闭";
            if (powerConsumer == null || !powerConsumer.CanWork()) return "等待电力";
            if (!HasEnoughInput()) return "等待原料";
            if (isProducing) return $"生产中 ({currentProgress:F1}/{productionTime}s)";
            return "准备就绪";
        }
        
        public string GetRecipeDescription()
        {
            return $"{inputAmount} {inputResourceType} → {outputAmount} {outputResourceType} ({productionTime}s)";
        }
    }
}
