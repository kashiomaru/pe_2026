using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class MapGenerator : MonoBehaviour
{
    [Header("Assets")]
    // CubeではなくQuadのプレハブ（またはUnity標準のQuad）を指定
    public GameObject floorQuadPrefab; 
    public GameObject wallQuadPrefab;
    public GameObject playerPrefab;
    
    [Header("Cinemachine")]
    public CinemachineCamera cinemachineCamera;

    [Header("Map Data (JSON)")]
    [TextArea(10, 20)]
    public string jsonMapData = 
        "{\n" +
        "  \"tileSize\": 4.0,\n" +
        "  \"layout\": [\n" +
        "    \"WWWWWWWW\",\n" +
        "    \"W......W\",\n" +
        "    \"W.P....W\",\n" +
        "    \"W......W\",\n" +
        "    \"WWWWWWWW\"\n" +
        "  ]\n" +
        "}";

    // JSONデシリアライズ用のクラス定義
    [System.Serializable]
    public class MapSchema
    {
        public float tileSize;
        public string[] layout;
        // 将来的にここへ public EnemyData[] enemies; などを追加できる
    }

    private void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        // 1. JSONパース
        MapSchema mapData = JsonUtility.FromJson<MapSchema>(jsonMapData);

        if (mapData == null || mapData.layout == null)
        {
            Debug.LogError("JSON format error!");
            return;
        }

        float size = mapData.tileSize;
        string[] rows = mapData.layout;
        
        GameObject levelParent = new GameObject("Level_Generated");
        Transform playerTransform = null; // 生成されたプレイヤーのTransformを保持

        for (int z = 0; z < rows.Length; z++)
        {
            string row = rows[z].Trim();
            for (int x = 0; x < row.Length; x++)
            {
                char tileType = row[x];
                
                // 配置座標 (Quadの中心)
                Vector3 position = new Vector3(x * size, 0, -z * size);

                // --- 床の生成 (Quad) ---
                // 空白以外なら床を敷く
                if (tileType != ' ') 
                {
                    // Quadはデフォルトで垂直なので、X軸に90度回転させて水平にする
                    GameObject floor = Instantiate(floorQuadPrefab, position, Quaternion.Euler(90, 0, 0), levelParent.transform);
                    floor.transform.localScale = new Vector3(size, size, 1); // Quadは2D的なのでZではなくYスケールかも確認が必要（通常QuadはXY平面）
                }

                // --- 壁・オブジェクトの生成 ---
                switch (tileType)
                {
                    case 'W': // Wall
                        GenerateWall(x, z, rows, position, size, levelParent.transform);
                        break;

                    case 'P': // Player
                        if (playerPrefab != null)
                        {
                            // プレイヤーを生成
                            GameObject playerInstance = Instantiate(playerPrefab, position + Vector3.up * 0.1f, Quaternion.identity, levelParent.transform);
                            playerTransform = playerInstance.transform;
                            
                            // CharacterControllerへの配慮
                            var cc = playerInstance.GetComponent<CharacterController>();
                            if(cc != null) cc.enabled = false;
                            
                            playerInstance.transform.position = position + Vector3.up * 0.1f;
                            playerInstance.transform.rotation = Quaternion.identity;
                            
                            if(cc != null) cc.enabled = true;
                        }
                        break;
                }
            }
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
            Debug.LogWarning("Player was not found in the map layout (no 'P' tile)");
        }
    }

    /// <summary>
    /// 壁を生成する。周囲の外側判定を行い、適切な方向に壁を配置する
    /// </summary>
    void GenerateWall(int x, int z, string[] rows, Vector3 position, float size, Transform parent)
    {
        // 上下左右の外側判定
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