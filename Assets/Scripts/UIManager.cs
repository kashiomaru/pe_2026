using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // シングルトンインスタンス

    [Header("UI References")]
    [SerializeField] private Slider atbSlider; // ATBゲージのスライダー
    [SerializeField] private GameObject damageTextPrefab; // ダメージ表示用のPrefab（UIDamageがアタッチされている）
    [SerializeField] private Transform damageTextParent; // ダメージテキストの親（Canvasなど）

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

        // 親を決定（指定されていない場合は自分自身）
        Transform parent = damageTextParent != null ? damageTextParent : transform;

        // プレハブをインスタンス化
        GameObject damageTextObj = Instantiate(damageTextPrefab, parent);
        
        // ワールド座標が指定されている場合は、ワールド座標をスクリーン座標に変換
        if (worldPosition.HasValue)
        {
            // Canvasを取得（親から探す、なければ自分自身から探す）
            Canvas canvas = damageTextParent != null 
                ? damageTextParent.GetComponentInParent<Canvas>() 
                : GetComponentInParent<Canvas>();
            
            if (canvas != null)
            {
                RectTransform rectTransform = damageTextObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // RenderTextureに描画しているカメラを探す
                    Camera renderCamera = FindRenderTextureCamera();
                    
                    // RenderTextureを使っている場合は、そのカメラでViewport座標を取得
                    if (renderCamera != null)
                    {
                        // Viewport座標を取得（0-1の範囲）
                        Vector3 viewportPos = renderCamera.WorldToViewportPoint(worldPosition.Value);
                        
                        // RawImageを探す（RenderTextureを表示しているRawImage）
                        UnityEngine.UI.RawImage rawImage = canvas.GetComponentInChildren<UnityEngine.UI.RawImage>();
                        if (rawImage != null)
                        {
                            RectTransform rawImageRect = rawImage.GetComponent<RectTransform>();
                            if (rawImageRect != null)
                            {
                                // RawImageのRectTransformのサイズと位置を取得
                                Rect rawImageRectWorld = GetWorldRect(rawImageRect);
                                
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
                            else
                            {
                                // RawImageが見つからない場合は通常の処理にフォールバック
                                FallbackDamageTextPosition(canvas, rectTransform, worldPosition.Value, renderCamera);
                            }
                        }
                        else
                        {
                            // RawImageが見つからない場合は通常の処理にフォールバック
                            FallbackDamageTextPosition(canvas, rectTransform, worldPosition.Value, renderCamera);
                        }
                    }
                    else
                    {
                        // RenderTextureを使っていない場合は通常の処理
                        FallbackDamageTextPosition(canvas, rectTransform, worldPosition.Value, Camera.main);
                    }
                }
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
    /// ダメージテキストを削除（通常は自動削除されるが、手動で削除する場合に使用）
    /// </summary>
    /// <param name="damageTextObj">削除するダメージテキストのGameObject</param>
    public void RemoveDamageText(GameObject damageTextObj)
    {
        if (damageTextObj != null)
        {
            Destroy(damageTextObj);
        }
    }

    /// <summary>
    /// RenderTextureに描画しているカメラを検索
    /// </summary>
    /// <returns>RenderTextureに描画しているカメラ、見つからない場合はnull</returns>
    Camera FindRenderTextureCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in cameras)
        {
            if (cam.targetTexture != null)
            {
                return cam;
            }
        }
        return null;
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

    /// <summary>
    /// 通常の座標変換処理（フォールバック）
    /// </summary>
    void FallbackDamageTextPosition(Canvas canvas, RectTransform rectTransform, Vector3 worldPosition, Camera camera)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Screen Space - Overlayの場合は、Camera.mainを使用
            Vector2 screenPos = camera.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPos;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            // Screen Space - Cameraの場合は、Canvasのカメラを使用
            Camera canvasCamera = canvas.worldCamera != null ? canvas.worldCamera : camera;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvasCamera,
                out Vector2 localPoint
            );
            rectTransform.localPosition = localPoint;
        }
        else if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // World Spaceの場合は、直接ワールド座標を使用
            rectTransform.position = worldPosition;
        }
    }
}

