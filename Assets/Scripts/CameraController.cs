using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 10f;
    public float fastMoveSpeed = 20f;
    public float edgeScrollSpeed = 5f;
    public bool enableEdgeScrolling = true;
    public float edgeScrollThreshold = 50f;

    [Header("缩放设置")]
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    [Header("飞船视角设置")]
    public float shipModeZoom = 12f;
    public Vector3 shipCameraOffset = new Vector3(0, 0, -10f);

    [Header("边界设置")]
    public bool enableBounds = true;
    public float leftBound = -50f;
    public float rightBound = 50f;
    public float bottomBound = -50f;
    public float topBound = 50f;

    [Header("调试模式")]
    public bool debugMode = false;

    private Camera mainCamera;
    private float currentZoom;
    private Vector3 targetPosition;
    
    // 飞船模式相关
    private bool isShipMode = false;
    private float buildModeZoom;
    private Transform shipCenter;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        currentZoom = mainCamera.orthographicSize;
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (isShipMode)
        {
            HandleShipModeMovement();
        }
        else
        {
            HandleMovement();
        }
        HandleZoom();
    }
    
    /// <summary>
    /// 飞船模式下的相机移动 - 跟随飞船中心，不响应WASD移动
    /// </summary>
    private void HandleShipModeMovement()
    {
        // 在飞船模式下，相机位置由飞船决定
        // 从 ThrustManager 获取当前飞船中心位置
        if (ThrustManager.Instance != null)
        {
            Vector3 shipCenter = ThrustManager.Instance.GetShipCenterWorldPosition();
            shipCenter.z = transform.position.z; // 保持相机的Z轴
            transform.position = Vector3.Lerp(transform.position, shipCenter, Time.deltaTime * 8f);
        }
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.zero;
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

        bool useEdgeScrolling = enableEdgeScrolling && !debugMode;

        if (Input.GetKey(KeyCode.W) || (useEdgeScrolling && IsMouseAtTopEdge()))
        {
            movement.y += currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || (useEdgeScrolling && IsMouseAtBottomEdge()))
        {
            movement.y -= currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || (useEdgeScrolling && IsMouseAtLeftEdge()))
        {
            movement.x -= currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || (useEdgeScrolling && IsMouseAtRightEdge()))
        {
            movement.x += currentSpeed * Time.deltaTime;
        }

        if (movement != Vector3.zero)
        {
            targetPosition += movement;
            ClampToBounds();
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            currentZoom -= scrollInput * zoomSpeed * 10f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            mainCamera.orthographicSize = currentZoom;
        }
    }

    private bool IsMouseAtLeftEdge()
    {
        return Input.mousePosition.x < edgeScrollThreshold;
    }

    private bool IsMouseAtRightEdge()
    {
        return Input.mousePosition.x > Screen.width - edgeScrollThreshold;
    }

    private bool IsMouseAtTopEdge()
    {
        return Input.mousePosition.y > Screen.height - edgeScrollThreshold;
    }

    private bool IsMouseAtBottomEdge()
    {
        return Input.mousePosition.y < edgeScrollThreshold;
    }

    private void ClampToBounds()
    {
        if (!enableBounds) return;

        targetPosition.x = Mathf.Clamp(targetPosition.x, leftBound, rightBound);
        targetPosition.y = Mathf.Clamp(targetPosition.y, bottomBound, topBound);
    }

    public void SetBounds(float left, float right, float bottom, float top)
    {
        leftBound = left;
        rightBound = right;
        bottomBound = bottom;
        topBound = top;
        enableBounds = true;
    }

    public void CenterCameraOn(Vector3 position)
    {
        targetPosition = new Vector3(position.x, position.y, transform.position.z);
        ClampToBounds();
        transform.position = targetPosition;
    }

    /// <summary>
    /// 启用飞船模式 - 相机锁定到飞船中心
    /// </summary>
    public void EnableShipMode(Vector3 shipCenterPosition)
    {
        isShipMode = true;
        buildModeZoom = currentZoom;
        
        // 保存飞船中心引用
        // 这里我们创建一个临时Transform来跟踪飞船位置
        if (shipCenter == null)
        {
            GameObject shipCenterObj = new GameObject("ShipCenter");
            shipCenter = shipCenterObj.transform;
        }
        shipCenter.position = shipCenterPosition;
        
        // 平滑过渡到飞船视角缩放
        currentZoom = Mathf.Lerp(currentZoom, shipModeZoom, 0.5f);
        
        Debug.Log("[CameraController] 切换到飞船操作视角");
    }

    /// <summary>
    /// 禁用飞船模式 - 返回建造自由视角
    /// </summary>
    public void DisableShipMode()
    {
        isShipMode = false;
        
        // 恢复到建造模式的缩放
        currentZoom = Mathf.Lerp(currentZoom, buildModeZoom, 0.5f);
        mainCamera.orthographicSize = currentZoom;
        
        // 将目标位置设置回当前位置，避免跳跃
        targetPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        
        Debug.Log("[CameraController] 切换到建造自由视角");
    }

    /// <summary>
    /// 更新飞船中心位置（每帧调用）
    /// </summary>
    public void UpdateShipCenter(Vector3 position)
    {
        if (shipCenter != null)
        {
            shipCenter.position = position;
        }
    }

    public bool IsShipModeActive() => isShipMode;

    public float GetCurrentZoom()
    {
        return currentZoom;
    }
}