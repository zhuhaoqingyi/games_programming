using UnityEngine;
using GameCore;
using GridSystem;

/// <summary>
/// 推进器喷射特效控制脚本
/// 挂载到尾焰 GameObject 上（fire），控制自身的显隐和缩放
/// 
/// 架构说明：
/// Thruster1 (预制体，GridManager 添加 BuildingComponent)
/// └── ThrusterBuilding
///     └── fire (这个脚本)
/// </summary>
public class ThrusterEffectController : MonoBehaviour
{
    [Header("尾焰特效")]
    public float scaleUpDuration = 3f;        // 缩放到最大尺寸的时间
    public Vector3 maxScale = new Vector3(3, 5, 1);  // 最大缩放尺寸
    public Vector3 minScale = new Vector3(1, 1, 1);  // 最小缩放尺寸（隐藏时）
    
    [Header("推进器方向")]
    [Tooltip("推进器排气方向，可在Inspector中手动设置（留空则自动从GridManager获取）")]
    public BuildDirection thrusterDirection = BuildDirection.East;
    
    [Header("调试")]
    public bool enableDebugLogs = true;
    
    private float scaleTimer = 0f;
    private bool isFiringNow = false;
    private SpriteRenderer spriteRenderer;
    private bool directionInitialized = false;
    private bool useManualDirection = false;

    private void Awake()
    {
        // 获取 SpriteRenderer 用于控制显隐
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 初始隐藏尾焰
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        transform.localScale = minScale;
    }

    private void Start()
    {
        InitializeDirection();
    }

    private void InitializeDirection()
    {
        // 如果已经在Inspector中手动设置了方向（通过标记判断）
        if (useManualDirection)
        {
            directionInitialized = true;
            if (enableDebugLogs)
                Debug.Log($"[ThrusterEffectController] {gameObject.name} 使用手动设置的方向: {thrusterDirection}");
            return;
        }

        // 尝试从根物体的 BuildingComponent 获取方向
        BuildingComponent rootBuilding = transform.root.GetComponent<BuildingComponent>();
        if (rootBuilding != null && GridManager.Instance != null)
        {
            var placed = GridManager.Instance.GetPlacedBuildingAt(rootBuilding.GridPosition);
            if (placed != null)
            {
                thrusterDirection = placed.Direction;
                directionInitialized = true;
                if (enableDebugLogs)
                    Debug.Log($"[ThrusterEffectController] {gameObject.name} 从 GridManager 获取方向: {thrusterDirection}");
                return;
            }
        }

        // 如果都获取不到，使用默认方向
        directionInitialized = true;
        if (enableDebugLogs)
            Debug.LogWarning($"[ThrusterEffectController] {gameObject.name} 无法获取方向，使用默认: {thrusterDirection}");
    }

    /// <summary>
    /// 在Inspector中点击此按钮可标记为使用手动方向
    /// </summary>
    [ContextMenu("Use Manual Direction")]
    private void SetUseManualDirection()
    {
        useManualDirection = true;
    }

    private void Update()
    {
        // 如果方向未初始化，每帧重试
        if (!directionInitialized)
        {
            InitializeDirection();
            if (!directionInitialized) return;
        }
        
        // 只在飞船模式下检测输入
        if (ThrustManager.Instance == null || ThrustManager.Instance.CurrentPhase != GamePhase.ShipMode)
        {
            SetFlameActive(false);
            return;
        }

        bool wasFiring = isFiringNow;
        isFiringNow = IsThrusterFiring();
        
        if (enableDebugLogs && wasFiring != isFiringNow)
        {
            Debug.Log($"[ThrusterEffectController] {gameObject.name}: firing={isFiringNow}, direction={thrusterDirection}");
        }
        
        SetFlameActive(isFiringNow);
    }

    /// <summary>
    /// 判断当前推进器是否应该点火
    /// 按键方向 = 飞船移动方向，与排气方向相反
    /// </summary>
    private bool IsThrusterFiring()
    {
        bool result = false;
        switch (thrusterDirection)
        {
            case BuildDirection.East:   // 向东排气 → 飞船向西移动 → 按 A/左
                result = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
                break;
            case BuildDirection.West:   // 向西排气 → 飞船向东移动 → 按 D/右
                result = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
                break;
            case BuildDirection.North:  // 向北排气 → 飞船向南移动 → 按 S/下
                result = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
                break;
            case BuildDirection.South:  // 向南排气 → 飞船向北移动 → 按 W/上
                result = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
                break;
        }
        return result;
    }

    /// <summary>
    /// 设置尾焰显隐和缩放
    /// </summary>
    private void SetFlameActive(bool active)
    {
        if (active)
        {
            // 显示尾焰
            if (spriteRenderer != null && !spriteRenderer.enabled)
            {
                spriteRenderer.enabled = true;
                scaleTimer = 0f;
            }
            
            // 逐渐缩放
            scaleTimer += Time.deltaTime;
            float t = Mathf.Clamp01(scaleTimer / scaleUpDuration);
            t = t * t * (3 - 2 * t);  // Smoothstep 平滑插值
            
            transform.localScale = Vector3.Lerp(minScale, maxScale, t);
        }
        else
        {
            // 隐藏尾焰
            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                spriteRenderer.enabled = false;
                transform.localScale = minScale;
                scaleTimer = 0f;
            }
        }
    }
}
