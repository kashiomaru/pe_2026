using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider atbSlider; // ATBゲージのスライダー
    [SerializeField] private GameObject damageTextPrefab; // ダメージ表示用のPrefab（UIDamageがアタッチされている）
    [SerializeField] private Transform damageTextParent; // ダメージテキストの親（Canvasなど）

    private PlayerController playerController; // プレイヤーコントローラー（タグから自動取得）

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
            // CanvasがScreen Space - Overlayの場合は、Camera.mainを使用
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition.Value);
                RectTransform rectTransform = damageTextObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.position = screenPos;
                }
            }
            else if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Screen Space - Cameraの場合は、Canvasのカメラを使用
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPosition.Value);
                RectTransform rectTransform = damageTextObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        screenPos,
                        canvas.worldCamera,
                        out Vector2 localPoint
                    );
                    rectTransform.localPosition = localPoint;
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
}

