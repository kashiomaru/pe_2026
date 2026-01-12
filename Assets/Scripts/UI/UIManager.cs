using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // シングルトンインスタンス

    [Header("UI References")]
    [SerializeField] private Slider atbSlider; // ATBゲージのスライダー
    [SerializeField] private GameObject damageTextPrefab; // ダメージ表示用のPrefab（UIDamageがアタッチされている）
    [SerializeField] private Transform damageTextParent; // ダメージテキストの親（Canvasなど）
    [SerializeField] private Canvas canvas; // ダメージテキストを表示するCanvas
    
    [Header("RenderTexture Settings")]
    [SerializeField] private Camera renderTextureCamera; // RenderTextureに描画しているカメラ
    [SerializeField] private RectTransform renderTextureRawImageRect; // RenderTextureを表示しているRawImageのRectTransform

    private PlayerController playerController; // プレイヤーコントローラー（タグから自動取得）

    void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("UIManager instance already exists. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // プレイヤーが未設定の場合はタグから取得
        TryGetPlayerController();
    }

    void Update()
    {
        UpdateATBSlider();
    }

    void TryGetPlayerController()
    {
        // 既に取得済みの場合は何もしない
        if (playerController != null)
        {
            return;
        }

        // タグから取得を試みる
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerController = playerObject.GetComponent<PlayerController>();
        }
    }

    void UpdateATBSlider()
    {
        // スライダーが設定されていない場合は処理しない
        if (atbSlider == null)
        {
            return;
        }

        // プレイヤーが取得できていない場合は取得を試みる
        if (playerController == null)
        {
            TryGetPlayerController();
        }

        // プレイヤーが取得できていない場合は処理しない
        if (playerController == null)
        {
            return;
        }

        // プレイヤーからATBゲージの値を取得してスライダーを更新
        float chargeRatio = playerController.GetChargeRatio();
        atbSlider.value = chargeRatio;
    }

    /// <summary>
    /// ダメージテキストを生成して表示
    /// </summary>
    /// <param name="damage">表示するダメージ値</param>
    /// <param name="worldPosition">ワールド座標（オプショナル、指定しない場合は画面中央）</param>
    public void ShowDamageText(int damage, Vector3? worldPosition = null)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("UIManager: Damage text prefab is not assigned!");
            return;
        }

        // ワールド座標が指定されていない場合は処理を中断
        if (worldPosition == null)
        {
            return;
        }

        // Canvasの参照を確認
        if (canvas == null)
        {
            Debug.LogWarning("UIManager: Canvas is not assigned! Please assign it in the Inspector.");
            return;
        }
        
        // RenderTextureに描画しているカメラの参照を確認
        if (renderTextureCamera == null)
        {
            Debug.LogWarning("UIManager: RenderTexture Camera is not assigned! Please assign it in the Inspector.");
            return;
        }
        
        // RenderTextureを表示しているRawImageのRectTransformの参照を確認
        if (renderTextureRawImageRect == null)
        {
            Debug.LogWarning("UIManager: RenderTexture RawImage RectTransform is not assigned! Please assign it in the Inspector.");
            return;
        }

        // 親を決定（指定されていない場合は自分自身）
        Transform parent = damageTextParent != null ? damageTextParent : transform;

        // すべてのチェックが完了したら、プレハブをインスタンス化
        GameObject damageTextObj = Instantiate(damageTextPrefab, parent);
        
        // ワールド座標をスクリーン座標に変換
        RectTransform rectTransform = damageTextObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Viewport座標を取得（0-1の範囲）
            Vector3 viewportPos = renderTextureCamera.WorldToViewportPoint(worldPosition.Value);
            
            // RawImageのRectTransformのサイズと位置を取得
            Rect rawImageRectWorld = GetWorldRect(renderTextureRawImageRect);
            
            // Viewport座標をRawImageのローカル座標に変換
            float x = rawImageRectWorld.x + viewportPos.x * rawImageRectWorld.width;
            float y = rawImageRectWorld.y + viewportPos.y * rawImageRectWorld.height;
            
            // Canvas座標に変換
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.position = new Vector3(x, y, 0);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Camera canvasCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
                Vector3 worldPos = new Vector3(x, y, canvasCamera.nearClipPlane);
                Vector3 screenPos = canvasCamera.WorldToScreenPoint(worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPos,
                    canvasCamera,
                    out Vector2 localPoint
                );
                rectTransform.localPosition = localPoint;
            }
        }

        // UIDamageコンポーネントを取得してダメージを表示
        UIDamage uidamage = damageTextObj.GetComponent<UIDamage>();
        if (uidamage != null)
        {
            uidamage.ShowDamage(damage);
        }
        else
        {
            Debug.LogWarning("UIManager: UIDamage component not found on prefab!");
        }
    }

    /// <summary>
    /// RectTransformのワールド座標での矩形を取得
    /// </summary>
    Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        
        foreach (Vector3 corner in corners)
        {
            if (corner.x < minX) minX = corner.x;
            if (corner.x > maxX) maxX = corner.x;
            if (corner.y < minY) minY = corner.y;
            if (corner.y > maxY) maxY = corner.y;
        }
        
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
}

