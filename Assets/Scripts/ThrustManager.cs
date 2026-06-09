using UnityEngine;
using System.Collections.Generic;
using GameCore;
using GridSystem;
using GameResources;

public class ThrustManager : MonoBehaviour
{
    public static ThrustManager Instance { get; private set; }

    [Header("Thrust Settings")]
    public float baseThrustPower = 200f;         // 单个推进器的基础推力
    public float dragCoefficient = 0.97f;        // 飞行阻力系数 (每帧速度乘以此值)
    public float stopThreshold = 0.05f;          // 速度低于此值时视为停止

    [Header("Scene References")]
    public Transform backgroundTransform;        // 背景图/背景容器
    // 矿石不再是容器的子物体，直接移动所有矿石实例

    [Header("Debug")]
    public bool enableDebugLogs = false;          // 是否输出调试日志

    private Vector2 currentVelocity = Vector2.zero;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.BuildMode;

    private Vector3 worldOffset = Vector3.zero;  // 世界坐标反向偏移量
    private Vector3 lastWorldOffset = Vector3.zero;  // 上一帧的偏移量（用于计算增量）

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
        // 切换视角的按键 (Tab)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePhase();
        }

        if (CurrentPhase == GamePhase.ShipMode)
        {
            HandleShipMovement();
        }
    }

    private void LateUpdate()
    {
        // 在 LateUpdate 中应用世界偏移（在相机之后，确保背景/矿石位置正确）
        ApplyWorldOffset();
    }

    /// <summary>
    /// 在建造模式和飞船操作模式之间切换
    /// </summary>
    public void TogglePhase()
    {
        if (CurrentPhase == GamePhase.BuildMode)
        {
            EnterShipMode();
        }
        else
        {
            EnterBuildMode();
        }
    }

    public void EnterShipMode()
    {
        CurrentPhase = GamePhase.ShipMode;
        CameraController camera = FindObjectOfType<CameraController>();
        if (camera != null)
        {
            camera.EnableShipMode(GetShipCenterWorldPosition());
        }

        // 禁用建筑放置器
        BuildingPlacer placer = FindObjectOfType<BuildingPlacer>();
        if (placer != null)
        {
            placer.enabled = false;
        }

        Debug.Log("[ThrustManager] Entered Ship Mode - Tab to return to Build Mode");
    }

    public void EnterBuildMode()
    {
        CurrentPhase = GamePhase.BuildMode;
        CameraController camera = FindObjectOfType<CameraController>();
        if (camera != null)
        {
            camera.DisableShipMode();
        }

        // 启用建筑放置器
        BuildingPlacer placer = FindObjectOfType<BuildingPlacer>();
        if (placer != null)
        {
            placer.enabled = true;
        }

        Debug.Log("[ThrustManager] Entered Build Mode - Tab to switch to Ship Mode");
    }

    /// <summary>
    /// 处理飞船移动输入和物理
    /// </summary>
    private void HandleShipMovement()
    {
        Vector2 thrustInput = Vector2.zero;

        // 收集四个方向的推进输入
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            thrustInput.y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            thrustInput.y -= 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            thrustInput.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            thrustInput.x += 1f;

        if (thrustInput != Vector2.zero && enableDebugLogs)
        {
            Debug.Log($"[ThrustManager] Thrust input: ({thrustInput.x}, {thrustInput.y})");
        }

        if (thrustInput != Vector2.zero)
        {
            // 计算总推力
            float totalThrust = CalculateTotalThrust(thrustInput);
            Vector2 thrustDirection = thrustInput.normalized;
            float shipMass = CalculateShipMass();

            if (enableDebugLogs)
            {
                Debug.Log($"[ThrustManager] Total thrust: {totalThrust:F2}, Ship mass: {shipMass:F2}, Thrust direction: {thrustDirection}");
            }

            // 加速度 = 总推力 / 质量
            Vector2 acceleration = thrustDirection * totalThrust / Mathf.Max(shipMass, 1f);
            currentVelocity += acceleration * Time.deltaTime;

            if (enableDebugLogs)
            {
                Debug.Log($"[ThrustManager] Velocity after acceleration: ({currentVelocity.x:F4}, {currentVelocity.y:F4})");
            }
        }

        // 应用阻力
        currentVelocity *= dragCoefficient;

        // 速度低于阈值时停止
        if (currentVelocity.magnitude < stopThreshold)
        {
            currentVelocity = Vector2.zero;
        }

        // 世界反向偏移：飞船向前飞 = 背景和世界向后移
        worldOffset.x -= currentVelocity.x * Time.deltaTime;
        worldOffset.y -= currentVelocity.y * Time.deltaTime;

        if (enableDebugLogs && currentVelocity.magnitude > 0.01f)
        {
            Debug.Log($"[ThrustManager] World offset: ({worldOffset.x:F4}, {worldOffset.y:F4})");
        }
    }

    /// <summary>
    /// 计算指定方向的总推力
    /// </summary>
    private float CalculateTotalThrust(Vector2 thrustDirection)
    {
        if (GridManager.Instance == null)
        {
            if (enableDebugLogs) Debug.LogWarning("[ThrustManager] GridManager is null!");
            return 0f;
        }

        float totalThrust = 0f;
        var placedBuildings = GridManager.Instance.GetAllPlacedBuildings();
        int thrusterCount = 0;
        int matchingThrusters = 0;

        if (enableDebugLogs)
        {
            Debug.Log($"[ThrustManager] Checking {placedBuildings.Count} placed buildings for thrusters...");
        }

        foreach (var kvp in placedBuildings)
        {
            PlacedBuilding pb = kvp.Value;
            if (pb.BuildingType != BuildingType.Thruster) continue;

            thrusterCount++;
            BuildDirection thrusterDir = pb.Direction;

            if (enableDebugLogs)
            {
                Debug.Log($"[ThrustManager] Found thruster at {kvp.Key}, direction: {thrusterDir}");
            }
            else
            {
                // 即使不启用详细日志，也输出关键信息用于对比
                Debug.Log($"[ThrustManager] Thruster: gridPos={kvp.Key}, direction={thrusterDir}");
            }

            if (IsThrustDirectionMatching(thrusterDir, thrustDirection))
            {
                matchingThrusters++;
                totalThrust += baseThrustPower;
                Debug.Log($"  -> MATCHED! Total thrust: {totalThrust:F2}");
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[ThrustManager] Thruster summary: {thrusterCount} total, {matchingThrusters} matching, total thrust: {totalThrust:F2}");
        }

        return totalThrust;
    }

    /// <summary>
    /// 判断推进器的朝向是否与推力方向匹配
    /// 推进器朝向 = 排气方向，飞船受力方向相反
    /// </summary>
    private bool IsThrustDirectionMatching(BuildDirection thrusterDirection, Vector2 thrustInput)
    {
        switch (thrusterDirection)
        {
            case BuildDirection.East:   // 向东排气 → 推力向西 (-X)
                return thrustInput.x < -0.5f;
            case BuildDirection.West:   // 向西排气 → 推力向东 (+X)
                return thrustInput.x > 0.5f;
            case BuildDirection.North:  // 向北排气 → 推力向南 (-Y)
                return thrustInput.y < -0.5f;
            case BuildDirection.South:  // 向南排气 → 推力向北 (+Y)
                return thrustInput.y > 0.5f;
            default:
                return false;
        }
    }

    /// <summary>
    /// 计算飞船总质量（基于所有建筑数量）
    /// </summary>
    private float CalculateShipMass()
    {
        if (GridManager.Instance == null) return 1f;

        var placedBuildings = GridManager.Instance.GetAllPlacedBuildings();
        float mass = 0f;

        foreach (var kvp in placedBuildings)
        {
            PlacedBuilding pb = kvp.Value;
            var def = pb.Definition;
            if (def == null) continue;
            mass += def.width * def.height;
        }

        return Mathf.Max(mass, 1f);
    }

    /// <summary>
    /// 获取飞船中心的世界坐标（避难仓位置）
    /// </summary>
    public Vector3 GetShipCenterWorldPosition()
    {
        if (GridManager.Instance == null) return Vector3.zero;

        var placedBuildings = GridManager.Instance.GetAllPlacedBuildings();
        foreach (var kvp in placedBuildings)
        {
            if (kvp.Value.BuildingType == BuildingType.EmergencyShelter)
            {
                Vector3 worldPos = kvp.Value.WorldPosition;
                worldPos.z = -10f;
                return worldPos;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 应用世界偏移到背景和所有矿石
    /// </summary>
    private void ApplyWorldOffset()
    {
        // 计算这一帧的增量（而不是累加总量）
        Vector3 deltaOffset = worldOffset - lastWorldOffset;
        lastWorldOffset = worldOffset;

        if (backgroundTransform != null)
        {
            backgroundTransform.position = new Vector3(worldOffset.x, worldOffset.y, backgroundTransform.position.z);
        }

        // 移动场景中所有矿石，只应用增量偏移
        SpaceOre[] allOres = FindObjectsOfType<SpaceOre>();
        foreach (SpaceOre ore in allOres)
        {
            ore.transform.position += new Vector3(deltaOffset.x, deltaOffset.y, 0f);
        }
    }

    /// <summary>
    /// 获取当前速度（用于UI显示等）
    /// </summary>
    public Vector2 GetCurrentVelocity() => currentVelocity;

    /// <summary>
    /// 获取当前飞船质量
    /// </summary>
    public float GetCurrentShipMass() => CalculateShipMass();

    /// <summary>
    /// 重置世界偏移（调试用）
    /// </summary>
    public void ResetWorldOffset()
    {
        worldOffset = Vector3.zero;
        currentVelocity = Vector2.zero;
        if (backgroundTransform != null) backgroundTransform.position = Vector3.zero;
    }
}
