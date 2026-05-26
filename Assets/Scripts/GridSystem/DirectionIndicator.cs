using UnityEngine;
using GameCore;

namespace GridSystem
{
    public class DirectionIndicator : MonoBehaviour
    {
        public static DirectionIndicator Create(Transform parent, BuildDirection direction, float size = 0.3f)
        {
            GameObject indicatorObj = new GameObject("DirectionIndicator");
            indicatorObj.transform.SetParent(parent);
            indicatorObj.transform.localPosition = Vector3.zero;
            indicatorObj.transform.localRotation = Quaternion.identity;
            indicatorObj.transform.localScale = Vector3.one;

            GameObject arrowObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowObj.transform.SetParent(indicatorObj.transform);
            arrowObj.transform.localPosition = new Vector3(0, 0, -0.05f);
            arrowObj.transform.localScale = new Vector3(size * 0.3f, size, 0.05f);
            
            Renderer arrowRenderer = arrowObj.GetComponent<Renderer>();
            if (arrowRenderer != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                mat.color = new Color(1f, 0.8f, 0f, 0.9f);
                arrowRenderer.material = mat;
            }

            GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headObj.transform.SetParent(indicatorObj.transform);
            headObj.transform.localPosition = new Vector3(0, size * 0.35f, -0.05f);
            headObj.transform.localScale = new Vector3(size * 0.5f, size * 0.3f, 0.05f);
            
            Renderer headRenderer = headObj.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                mat.color = new Color(1f, 0.8f, 0f, 0.9f);
                headRenderer.material = mat;
            }

            Destroy(arrowObj.GetComponent<Collider>());
            Destroy(headObj.GetComponent<Collider>());

            SetDirection(indicatorObj, direction);

            return indicatorObj.AddComponent<DirectionIndicator>();
        }

        public static void SetDirection(GameObject indicatorObj, BuildDirection direction)
        {
            Vector3 rotation = Vector3.zero;
            switch (direction)
            {
                case BuildDirection.East:
                    rotation = Vector3.zero;
                    break;
                case BuildDirection.South:
                    rotation = new Vector3(0, 0, -90);
                    break;
                case BuildDirection.West:
                    rotation = new Vector3(0, 0, -180);
                    break;
                case BuildDirection.North:
                    rotation = new Vector3(0, 0, -270);
                    break;
            }
            indicatorObj.transform.localRotation = Quaternion.Euler(rotation);
        }

        public void UpdateDirection(BuildDirection direction)
        {
            SetDirection(gameObject, direction);
        }
    }
}
