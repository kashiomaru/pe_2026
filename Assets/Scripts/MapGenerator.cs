using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Assets")]
    // CubeではなくQuadのプレハブ（またはUnity標準のQuad）を指定
    public GameObject floorQuadPrefab; 
    public GameObject wallQuadPrefab;
    public GameObject playerPrefab;

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
                        // 壁もQuadで作る（ビルボード状、または箱にするには4枚必要）
                        // ※簡易的に「カメラから見て正面」の一枚板にするか、交差させる手法（Cross Quad）があります。
                        // ここでは「垂直に立った板」として配置します。
                        
                        // 位置調整: Quadの中心が足元なら y = size/2 上げる
                        Vector3 wallPos = position + Vector3.up * (size * 0.5f);
                        GameObject wall = Instantiate(wallQuadPrefab, wallPos, Quaternion.identity, levelParent.transform);
                        wall.transform.localScale = new Vector3(size, size, 1);
                        break;

                    case 'P': // Player
                        if (playerPrefab != null)
                        {
                            // CharacterControllerへの配慮（前回同様）
                            var cc = playerPrefab.GetComponent<CharacterController>();
                            if(cc != null) cc.enabled = false;
                            
                            playerPrefab.transform.position = position + Vector3.up * 0.1f;
                            playerPrefab.transform.rotation = Quaternion.identity;
                            
                            if(cc != null) cc.enabled = true;
                        }
                        break;
                }
            }
        }
    }
}