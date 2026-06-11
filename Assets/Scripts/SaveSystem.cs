using UnityEngine;
using System.Collections.Generic;
using System.IO;
using GameCore;
using GridSystem;

/// <summary>
/// 游戏存档系统
/// 按 Ctrl+S 保存，主菜单加载
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string SAVE_FILE_NAME = "savegame.json";
    private const string SAVE_FILE_NAME_BACKUP = "savegame_backup.json";

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
        // Ctrl+S 保存游戏
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }
    }

    public string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    public string GetBackupFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME_BACKUP);
    }

    public bool HasSaveFile()
    {
        return File.Exists(GetSaveFilePath());
    }

    public void SaveGame()
    {
        try
        {
            SaveData saveData = CollectSaveData();
            string json = JsonUtility.ToJson(saveData, true);
            
            // 先备份旧存档
            string savePath = GetSaveFilePath();
            if (File.Exists(savePath))
            {
                File.Copy(savePath, GetBackupFilePath(), true);
            }

            // 写入新存档
            File.WriteAllText(savePath, json);
            
            Debug.Log($"[SaveSystem] 游戏已保存: {savePath}");
            
            // 显示保存提示
            ShowSaveNotification("Game Saved!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] 保存失败: {e.Message}");
        }
    }

    public bool LoadGame()
    {
        try
        {
            string savePath = GetSaveFilePath();
            if (!File.Exists(savePath))
            {
                Debug.LogWarning("[SaveSystem] 没有找到存档文件");
                return false;
            }

            string json = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            
            ApplySaveData(saveData);
            
            Debug.Log($"[SaveSystem] 游戏已加载: {savePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] 加载失败: {e.Message}");
            return false;
        }
    }

    public void DeleteSaveFile()
    {
        string savePath = GetSaveFilePath();
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[SaveSystem] 存档已删除");
        }
    }

    private SaveData CollectSaveData()
    {
        SaveData data = new SaveData();

        // 1. 保存资源
        if (GameManager.Instance != null)
        {
            var resources = GameManager.Instance.GetAllResources();
            data.resources = new List<ResourceEntry>();
            foreach (var kvp in resources)
            {
                data.resources.Add(new ResourceEntry { type = kvp.Key, amount = kvp.Value });
            }
        }

        // 2. 保存建筑
        if (GridManager.Instance != null)
        {
            data.buildings = new List<BuildingEntry>();
            foreach (var kvp in GridManager.Instance.GetAllPlacedBuildings())
            {
                GridPosition pos = kvp.Key;
                PlacedBuilding placed = kvp.Value;
                if (placed != null)
                {
                    data.buildings.Add(new BuildingEntry
                    {
                        x = pos.x,
                        y = pos.y,
                        type = placed.BuildingType,
                        direction = placed.Direction
                    });
                }
            }
        }

        // 3. 保存游戏时间
        if (GameManager.Instance != null)
        {
            var gameTime = GameManager.Instance.GetGameTime();
            data.totalGameTime = gameTime.totalTime;
        }

        // 4. 保存飞船位置（如果有 ThrustManager）
        if (ThrustManager.Instance != null)
        {
            Transform shipTransform = ThrustManager.Instance.shipTransform;
            if (shipTransform != null)
            {
                data.shipPositionX = shipTransform.position.x;
                data.shipPositionY = shipTransform.position.y;
                data.shipRotation = shipTransform.rotation.eulerAngles.z;
            }
        }

        return data;
    }

    private void ApplySaveData(SaveData data)
    {
        // 1. 清除当前场景中的所有建筑
        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearAllBuildings();
        }

        // 2. 加载建筑：先放置太空板，再放置非板类建筑（跳过验证）
        if (data.buildings != null)
        {
            // 排序：板类优先
            var sortedBuildings = new List<BuildingEntry>(data.buildings);
            sortedBuildings.Sort((a, b) =>
            {
                var defA = DataConfig.GetBuilding(a.type);
                var defB = DataConfig.GetBuilding(b.type);
                bool isBoardA = defA != null && defA.isBoard;
                bool isBoardB = defB != null && defB.isBoard;
                return isBoardB.CompareTo(isBoardA); // board first
            });

            foreach (var building in sortedBuildings)
            {
                GridPosition pos = new GridPosition(building.x, building.y);
                GridManager.Instance.PlaceBuildingWithDirection(pos, building.type, building.direction, validate: false);
            }
        }

        // 3. 注册容器容量
        if (GameManager.Instance != null && GridManager.Instance != null)
        {
            foreach (var kvp in GridManager.Instance.GetAllPlacedBuildings())
            {
                PlacedBuilding placed = kvp.Value;
                if (placed == null || placed.GameObject == null) continue;

                var container = placed.GameObject.GetComponentInChildren<GridSystem.ContainerComponent>();
                if (container != null && container.resourceCapacities.Count > 0)
                {
                    var capacities = new Dictionary<ResourceType, int>();
                    foreach (var rc in container.resourceCapacities)
                    {
                        capacities[rc.resourceType] = rc.capacity;
                    }
                    GameManager.Instance.AddContainer(capacities, container.GetTotalCapacity());
                }
            }
        }

        // 4. 加载资源
        if (data.resources != null && GameManager.Instance != null)
        {
            foreach (var entry in data.resources)
            {
                // 使用不检查容量的方式添加
                var storageManagerField = typeof(GameManager).GetField("storageManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (storageManagerField != null)
                {
                    var storageManager = storageManagerField.GetValue(GameManager.Instance);
                    var addResourceMethod = storageManager.GetType().GetMethod("AddResource", 
                        new System.Type[] { typeof(ResourceType), typeof(int), typeof(bool) });
                    if (addResourceMethod != null)
                    {
                        addResourceMethod.Invoke(storageManager, new object[] { entry.type, entry.amount, false });
                    }
                }
            }
        }

        // 5. 加载飞船位置
        if (ThrustManager.Instance != null)
        {
            Transform shipTransform = ThrustManager.Instance.shipTransform;
            if (shipTransform != null)
            {
                shipTransform.position = new Vector3(data.shipPositionX, data.shipPositionY, 0);
                shipTransform.rotation = Quaternion.Euler(0, 0, data.shipRotation);
            }
        }

        // 6. 更新UI
        if (UI.BuildingUI.Instance != null)
        {
            UI.BuildingUI.Instance.UpdateAllAffordability();
        }
    }

    private void ShowSaveNotification(string message)
    {
        // 简单的控制台提示，可以扩展为 UI 通知
        Debug.Log($"[SaveSystem] {message}");
    }
}

/// <summary>
/// 存档数据结构
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<ResourceEntry> resources;
    public List<BuildingEntry> buildings;
    public float totalGameTime;
    public float shipPositionX;
    public float shipPositionY;
    public float shipRotation;
}

[System.Serializable]
public class ResourceEntry
{
    public ResourceType type;
    public int amount;
}

[System.Serializable]
public class BuildingEntry
{
    public int x;
    public int y;
    public BuildingType type;
    public BuildDirection direction;
}
