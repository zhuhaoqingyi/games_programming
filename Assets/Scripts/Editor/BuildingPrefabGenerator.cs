using UnityEngine;
using UnityEditor;
using System.IO;
using GameCore;
using GridSystem;

public class BuildingPrefabGenerator : MonoBehaviour
{
    [MenuItem("Tools/生成建筑预制体")]
    public static void GenerateBuildingPrefabs()
    {
        string prefabPath = "Assets/Resources/Prefabs/Buildings";
        
        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }

        foreach (BuildingType buildingType in System.Enum.GetValues(typeof(BuildingType)))
        {
            if (buildingType == BuildingType.None) continue;

            var buildingDef = DataConfig.GetBuilding(buildingType);
            if (buildingDef == null) continue;

            CreateBuildingPrefab(buildingDef, prefabPath);
        }

        CreateBuildingPreviewPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("建筑预制体生成完成！");
    }

    private static void CreateBuildingPrefab(BuildingDefinition buildingDef, string prefabPath)
    {
        string prefabName = buildingDef.type.ToString();
        string fullPath = Path.Combine(prefabPath, prefabName + ".prefab");

        if (File.Exists(fullPath))
        {
            Debug.Log($"预制体已存在，跳过: {prefabName}");
            return;
        }

        GameObject buildingObj = new GameObject(buildingDef.name);
        
        GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualObj.transform.SetParent(buildingObj.transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localScale = new Vector3(buildingDef.width * 0.9f, buildingDef.height * 0.9f, 0.5f);

        Renderer renderer = visualObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (buildingDef.category)
            {
                case BuildingCategory.Core:
                    renderer.material.color = new Color(0.8f, 0.4f, 0.8f);
                    break;
                case BuildingCategory.Power:
                    renderer.material.color = new Color(0.4f, 0.8f, 0.4f);
                    break;
                case BuildingCategory.Production:
                    renderer.material.color = new Color(0.8f, 0.6f, 0.4f);
                    break;
                case BuildingCategory.Storage:
                    renderer.material.color = new Color(0.6f, 0.6f, 0.6f);
                    break;
                case BuildingCategory.Propulsion:
                    renderer.material.color = new Color(1f, 0.5f, 0.2f);
                    break;
            }
        }

        BuildingComponent component = buildingObj.AddComponent<BuildingComponent>();
        SerializedObject so = new SerializedObject(component);
        SerializedProperty typeProp = so.FindProperty("buildingType");
        typeProp.enumValueIndex = (int)buildingDef.type;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(buildingObj, fullPath);
        DestroyImmediate(buildingObj);

        Debug.Log($"生成预制体: {prefabName}");
    }

    private static void CreateBuildingPreviewPrefab()
    {
        string prefabPath = "Assets/Resources/Prefabs";
        string fullPath = Path.Combine(prefabPath, "BuildingPreview.prefab");

        if (File.Exists(fullPath))
        {
            Debug.Log("预览预制体已存在，跳过");
            return;
        }

        GameObject previewObj = new GameObject("BuildingPreview");
        
        GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualObj.transform.SetParent(previewObj.transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localScale = Vector3.one * 0.9f;

        Renderer renderer = visualObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material previewMat = new Material(Shader.Find("Transparent/Diffuse"));
            previewMat.color = new Color(0f, 1f, 0f, 0.5f);
            renderer.material = previewMat;
        }

        PrefabUtility.SaveAsPrefabAsset(previewObj, fullPath);
        DestroyImmediate(previewObj);

        Debug.Log("生成预览预制体: BuildingPreview");
    }
}
