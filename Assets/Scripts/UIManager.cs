using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider atbSlider; // ATBゲージのスライダー

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
}

