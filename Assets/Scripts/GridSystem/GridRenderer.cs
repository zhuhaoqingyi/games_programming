using UnityEngine;
using System.Collections.Generic;

namespace GridSystem
{
    public class GridRenderer : MonoBehaviour
    {
        public static GridRenderer Instance { get; private set; }

        [Header("Grid Settings")]
        public Camera mainCamera;
        public float cellSize = 1f;
        public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        public float baseLineWidth = 0.01f;
        public float lineWidthScale = 0.3f;
        public int linesMargin = 10;

        [Header("LOD Settings")]
        public float[] gridStepSizes = new float[] { 1f, 2f, 5f, 10f, 20f, 50f, 100f };
        public float lodThresholdFactor = 8f;

        private Material gridMaterial;
        private List<LineRenderer> lineRenderers;
        private float currentGridSize;
        private bool isGridVisible = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            CreateGridMaterial();
            lineRenderers = new List<LineRenderer>();
            
            SetGridVisible(false);
        }

        public void ShowGrid()
        {
            SetGridVisible(true);
        }

        public void HideGrid()
        {
            SetGridVisible(false);
        }

        private void SetGridVisible(bool visible)
        {
            isGridVisible = visible;
            gameObject.SetActive(visible);
        }

        private void CreateGridMaterial()
        {
            Shader unlitColorShader = Shader.Find("Unlit/Color");
            if (unlitColorShader == null)
            {
                unlitColorShader = Shader.Find("Sprites/Default");
            }
            gridMaterial = new Material(unlitColorShader);
            gridMaterial.color = gridColor;
        }

        private float CalculateGridSize()
        {
            float cameraSize = GetCameraSize();
            float targetGridSpacing = cameraSize / lodThresholdFactor;
            
            float bestStep = cellSize;
            foreach (float step in gridStepSizes)
            {
                if (step >= targetGridSpacing)
                {
                    bestStep = step;
                    break;
                }
                bestStep = step;
            }
            return bestStep;
        }

        private float CalculateLineWidth()
        {
            float scaleFactor = currentGridSize / cellSize;
            return baseLineWidth * Mathf.Pow(scaleFactor, lineWidthScale);
        }

        private void EnsureLineRenderers(int count)
        {
            while (lineRenderers.Count < count)
            {
                CreateLineRenderer("Line" + lineRenderers.Count);
            }

            for (int i = 0; i < lineRenderers.Count; i++)
            {
                lineRenderers[i].gameObject.SetActive(i < count);
            }
        }

        private void CreateLineRenderer(string name)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = baseLineWidth;
            lr.endWidth = baseLineWidth;
            lr.material = gridMaterial;
            lr.startColor = gridColor;
            lr.endColor = gridColor;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lineRenderers.Add(lr);
        }

        private void Update()
        {
            if (isGridVisible)
            {
                UpdateGrid();
            }
        }

        private void UpdateGrid()
        {
            if (mainCamera == null) return;

            float baseSize = cellSize;
            currentGridSize = CalculateGridSize();
            float currentLineWidth = CalculateLineWidth();

            Vector3 cameraPos = mainCamera.transform.position;
            float cameraSize = GetCameraSize();

            float halfViewSize = cameraSize * 1.5f;

            int linesNeededHorizontal = Mathf.CeilToInt(2 * halfViewSize / baseSize) + linesMargin;
            int linesNeededVertical = linesNeededHorizontal;
            int totalLinesNeeded = linesNeededHorizontal + linesNeededVertical;

            EnsureLineRenderers(totalLinesNeeded);

            foreach (var lr in lineRenderers)
            {
                lr.startWidth = currentLineWidth;
                lr.endWidth = currentLineWidth;
            }

            float startY = Mathf.Floor(cameraPos.y / baseSize - halfViewSize / baseSize) * baseSize;
            float startX = Mathf.Floor(cameraPos.x / baseSize - halfViewSize / baseSize) * baseSize;

            int lineIndex = 0;

            for (int i = 0; i < linesNeededHorizontal; i++)
            {
                float y = startY + i * baseSize;
                LineRenderer lr = lineRenderers[lineIndex];
                lr.SetPosition(0, new Vector3(cameraPos.x - halfViewSize, y, 0));
                lr.SetPosition(1, new Vector3(cameraPos.x + halfViewSize, y, 0));
                lineIndex++;
            }

            for (int i = 0; i < linesNeededVertical; i++)
            {
                float x = startX + i * baseSize;
                LineRenderer lr = lineRenderers[lineIndex];
                lr.SetPosition(0, new Vector3(x, cameraPos.y - halfViewSize, 0));
                lr.SetPosition(1, new Vector3(x, cameraPos.y + halfViewSize, 0));
                lineIndex++;
            }
        }

        private float GetCameraSize()
        {
            if (mainCamera.orthographic)
            {
                return mainCamera.orthographicSize * mainCamera.aspect;
            }
            else
            {
                float distance = Mathf.Abs(mainCamera.transform.position.z);
                return distance * Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad / 2f) * mainCamera.aspect;
            }
        }

        private void OnValidate()
        {
            if (gridMaterial != null)
            {
                gridMaterial.color = gridColor;
            }
        }
    }
}
