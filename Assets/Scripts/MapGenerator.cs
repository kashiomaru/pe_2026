using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class MapGenerator : MonoBehaviour
{
    [Header("Assets")]
    // CubeではなくQuadのプレハブ（またはUnity標準のQuad）を指定
    public GameObject floorQuadPrefab; 
    public GameObject wallQuadPrefab;
    public GameObject doorQuadPrefab;
    public GameObject playerPrefab;
    
    [Header("Cinemachine")]
    public CinemachineCamera cinemachineCamera;

    // 生成されたプレイヤーインスタンス（使いまわし用）
    private GameObject playerInstance = null;

    /// <summary>
    /// JSONからマップを生成する（MapManagerから呼び出される）
    /// </summary>
    public void GenerateMapFromJson(string json, int spawnIndex)
    {
        // 今あるマップを消す（プレイヤーインスタンスは除外）
        foreach (Transform child in transform)
        {
            if (child.gameObject != playerInstance)
            {
                Destroy(child.gameObject);
            }
        }

        MapData data = JsonUtility.FromJson<MapData>(json);
        
        if (data == null || data.layout == null)
        {
            Debug.LogError("JSON format error!");
            return;
        }

        float size = data.tileSize;
        string[] rows = data.layout;
        
        // ポータル情報を辞書化して検索しやすくする
        Dictionary<string, PortalDef> portalDict = new Dictionary<string, PortalDef>();
        if (data.portals != null)
        {
            foreach (var p in data.portals)
            {
                portalDict[p.triggerChar] = p;
            }
        }

        // スポーン候補位置リスト
        List<Vector3> spawnPoints = new List<Vector3>();

        GameObject levelParent = new GameObject("Level_Generated");
        levelParent.transform.SetParent(transform);

        for (int z = 0; z < rows.Length; z++)
        {
            string row = rows[z].Trim();
            for (int x = 0; x < row.Length; x++)
            {
                char tileType = row[x];
                string charStr = tileType.ToString();
                
                // 配置座標 (Quadの中心)
                Vector3 position = new Vector3(x * size, 0, -z * size);

                // --- 床の生成 (Quad) ---
                // 空白以外なら床を敷く
                if (tileType != ' ') 
                {
                    // Quadはデフォルトで垂直なので、X軸に90度回転させて水平にする
                    GameObject floor = Instantiate(floorQuadPrefab, position, Quaternion.Euler(90, 0, 0), levelParent.transform);
                    floor.transform.localScale = new Vector3(size, size, 1);
                }

                // --- 壁・オブジェクトの生成 ---
                switch (tileType)
                {
                    case 'W': // Wall
                        GenerateWall(x, z, rows, position, size, levelParent.transform, false);
                        break;

                    case 'P': // Player
                        // スポーン地点として記録（0番扱い）
                        spawnPoints.Insert(0, position);
                        break;
                }
                
                // 数字（'0'～'9'）の場合は壁とドアを配置、かつポータル処理
                if (char.IsDigit(tileType))
                {
                    // 壁とドアを配置
                    GenerateWall(x, z, rows, position, size, levelParent.transform, true);
                    
                    // ポータルの処理
                    if (portalDict.ContainsKey(charStr))
                    {
                        PortalDef def = portalDict[charStr];
                        
                        // ドアの位置を取得するため、壁生成と同じロジックで位置を計算
                        List<Vector2Int> outsideDirections = GetOutsideDirections(x, z, rows);
                        if (outsideDirections.Count > 0 && outsideDirections.Count < 3)
                        {
                            var dir = outsideDirections[0]; // 最初の外側方向を使用
                            Vector3 offset = new Vector3(dir.x * size * 0.5f, 0, -dir.y * size * 0.5f);
                            Vector3 wallPos = position + offset + Vector3.up * (size * 0.5f);
                            Vector3 doorOffset = new Vector3(-dir.x * 0.01f, 0, dir.y * 0.01f);
                            float doorHeight = doorQuadPrefab != null ? doorQuadPrefab.transform.localScale.y : size;
                            Vector3 doorPos = wallPos + doorOffset;
                            doorPos.y = doorHeight * 0.5f;
                            
                            // ポータル用のトリガーオブジェクトを作成
                            GameObject portalTrigger = new GameObject($"Portal_{charStr}");
                            portalTrigger.transform.position = doorPos;
                            portalTrigger.transform.SetParent(levelParent.transform);
                            
                            // BoxColliderを追加（トリガー）
                            BoxCollider trigger = portalTrigger.AddComponent<BoxCollider>();
                            trigger.isTrigger = true;
                            trigger.size = new Vector3(size * 0.5f, doorHeight, size * 0.5f);
                            
                            // MapPortalコンポーネントを追加
                            MapPortal portalScript = portalTrigger.AddComponent<MapPortal>();
                            portalScript.targetMapId = def.targetMapId;
                            portalScript.targetSpawnId = def.targetSpawnId;
                        }
                        
                        // スポーン地点として記録
                        spawnPoints.Add(position);
                    }
                }
            }
        }
        
        // プレイヤーを指定の位置へ移動
        Transform playerTransform = null;
        if (spawnPoints.Count > spawnIndex && playerPrefab != null)
        {
            Vector3 spawnPos = spawnPoints[spawnIndex] + Vector3.up * 0.1f;
            
            // 最初の生成時のみインスタンス化、その後は使いまわし
            playerInstance = playerInstance ?? Instantiate(playerPrefab, spawnPos, Quaternion.identity, transform);
            playerTransform = playerInstance.transform;
                
            // 既存のインスタンスを使いまわし
            playerTransform = playerInstance.transform;
            
            // CharacterControllerへの配慮
            var cc = playerInstance.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            
            playerInstance.transform.position = spawnPos;
            playerInstance.transform.rotation = Quaternion.identity;
            
            if (cc != null) cc.enabled = true;
        }
        
        // CinemachineのTrackingTargetにプレイヤーのTransformを設定
        if (cinemachineCamera != null && playerTransform != null)
        {
            cinemachineCamera.Target.TrackingTarget = playerTransform;
            Debug.Log("Cinemachine TrackingTarget set to Player");
        }
        else if (cinemachineCamera == null)
        {
            Debug.LogWarning("CinemachineCamera is not assigned in MapGenerator");
        }
        else if (playerTransform == null)
        {
            Debug.LogWarning("Player was not found in the map layout (no spawn point)");
        }
    }


    /// <summary>
    /// 外側方向のリストを取得する
    /// </summary>
    List<Vector2Int> GetOutsideDirections(int x, int z, string[] rows)
    {
        List<Vector2Int> outsideDirections = new List<Vector2Int>();
        
        // 上（Zマイナス方向、rowsのインデックスが小さい方）
        if (z == 0 || IsOutside(rows[z - 1], x))
        {
            outsideDirections.Add(new Vector2Int(0, -1)); // 上
        }
        
        // 下（Zプラス方向、rowsのインデックスが大きい方）
        if (z >= rows.Length - 1 || IsOutside(rows[z + 1], x))
        {
            outsideDirections.Add(new Vector2Int(0, 1)); // 下
        }
        
        // 左（Xマイナス方向）
        if (x == 0 || IsOutside(rows[z], x - 1))
        {
            outsideDirections.Add(new Vector2Int(-1, 0)); // 左
        }
        
        // 右（Xプラス方向）
        if (x >= rows[z].Length - 1 || IsOutside(rows[z], x + 1))
        {
            outsideDirections.Add(new Vector2Int(1, 0)); // 右
        }
        
        return outsideDirections;
    }

    /// <summary>
    /// 壁を生成する。周囲の外側判定を行い、適切な方向に壁を配置する
    /// </summary>
    /// <param name="placeDoor">ドアを配置するかどうか</param>
    void GenerateWall(int x, int z, string[] rows, Vector3 position, float size, Transform parent, bool placeDoor)
    {
        // 上下左右の外側判定
        List<Vector2Int> outsideDirections = GetOutsideDirections(x, z, rows);
        
        // 外側が3つ以上ある場合は壁を配置しない
        if (outsideDirections.Count >= 3)
        {
            return;
        }
        
        // 外側が1つまたは2つの場合、壁を生成
        foreach (var dir in outsideDirections)
        {
            // 壁の位置を外側に寄せる（外側方向にタイルサイズの半分だけ移動）
            Vector3 offset = new Vector3(dir.x * size * 0.5f, 0, -dir.y * size * 0.5f);
            Vector3 wallPos = position + offset + Vector3.up * (size * 0.5f);
            Quaternion rotation = GetWallRotation(dir);
            GameObject wall = Instantiate(wallQuadPrefab, wallPos, rotation, parent);
            wall.transform.localScale = new Vector3(size, size, 1);
            
            // ドアを配置する場合、壁より0.01だけ内側に配置
            if (placeDoor && doorQuadPrefab != null)
            {
                // 内側方向に0.01移動（外側方向の逆方向）
                Vector3 doorOffset = new Vector3(-dir.x * 0.01f, 0, dir.y * 0.01f);
                
                // ドアの高さをScaleから取得
                float doorHeight = doorQuadPrefab.transform.localScale.y;
                
                // 壁の位置を基準に、内側に0.01移動し、床に設置する位置を計算
                // 壁のY座標（size * 0.5f）から、ドアの高さの半分（doorHeight * 0.5f）に変更
                Vector3 doorPos = wallPos + doorOffset;
                doorPos.y = doorHeight * 0.5f; // 床に設置
                
                // 実際のドアを配置（元のサイズのまま）
                GameObject door = Instantiate(doorQuadPrefab, doorPos, rotation, parent);
            }
        }
    }
    
    /// <summary>
    /// 指定位置が外側（マップ外）かどうかを判定
    /// </summary>
    bool IsOutside(string row, int x)
    {
        if (string.IsNullOrEmpty(row) || x < 0 || x >= row.Length)
        {
            return true;
        }
        char tile = row[x];
        return tile == ' ';
    }
    
    /// <summary>
    /// 外側方向に応じた壁の回転角度を取得
    /// 壁の表はX軸プラス方向（デフォルト）
    /// </summary>
    Quaternion GetWallRotation(Vector2Int outsideDirection)
    {
        // 外側方向に応じて、内側（反対方向）を向くように回転（逆向きに修正）
        if (outsideDirection == new Vector2Int(0, -1)) // 上が外側 → 上向き（0度、デフォルト）
        {
            return Quaternion.identity;
        }
        else if (outsideDirection == new Vector2Int(0, 1)) // 下が外側 → 下向き（180度Y軸回転）
        {
            return Quaternion.Euler(0, 180, 0);
        }
        else if (outsideDirection == new Vector2Int(-1, 0)) // 左が外側 → 左向き（90度Y軸回転）
        {
            return Quaternion.Euler(0, -90, 0);
        }
        else if (outsideDirection == new Vector2Int(1, 0)) // 右が外側 → 右向き（-90度Y軸回転）
        {
            return Quaternion.Euler(0, 90, 0);
        }
        
        return Quaternion.identity;
    }
}