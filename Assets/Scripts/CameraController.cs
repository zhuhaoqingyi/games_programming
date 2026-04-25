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

    [Header("边界设置")]
    public bool enableBounds = true;
    public float leftBound = -50f;
    public float rightBound = 50f;
    public float bottomBound = -50f;
    public float topBound = 50f;

    private Camera mainCamera;
    private float currentZoom;
    private Vector3 targetPosition;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        currentZoom = mainCamera.orthographicSize;
        targetPosition = transform.position;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.zero;
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

        if (Input.GetKey(KeyCode.W) || (enableEdgeScrolling && IsMouseAtTopEdge()))
        {
            movement.y += currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || (enableEdgeScrolling && IsMouseAtBottomEdge()))
        {
            movement.y -= currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || (enableEdgeScrolling && IsMouseAtLeftEdge()))
        {
            movement.x -= currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || (enableEdgeScrolling && IsMouseAtRightEdge()))
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

    public float GetCurrentZoom()
    {
        return currentZoom;
    }
}