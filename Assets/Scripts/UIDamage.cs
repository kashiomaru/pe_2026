using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(RectTransform))]
public class UIDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textMeshPro; // TextMeshProUGUIコンポーネント（オプショナル、未設定の場合は自動取得）
    [SerializeField] private RectTransform rectTransform; // RectTransformコンポーネント（オプショナル、未設定の場合は自動取得）
    
    [Header("Settings")]
    [SerializeField] private float displayDuration = 1.0f; // 表示時間（秒）
    [SerializeField] private float moveDistance = 50f; // 移動距離（ピクセル）
    [SerializeField] private Vector2 moveDirection = Vector2.up; // 移動方向（正規化）
    
    private CancellationTokenSource _cancellationTokenSource;
    private Vector2 _initialPosition;
    private Color _initialColor;

    void Awake()
    {
        // 参照が設定されていない場合は自動取得
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
            if (textMeshPro == null)
            {
                Debug.LogError("UIDamage: TextMeshProUGUI component not found!");
            }
        }
        
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("UIDamage: RectTransform component not found!");
            }
        }
    }

    /// <summary>
    /// ダメージを表示して、指定時間後に自動削除
    /// </summary>
    /// <param name="damage">表示するダメージ値</param>
    public void ShowDamage(int damage)
    {
        if (textMeshPro == null)
        {
            Debug.LogError("UIDamage: TextMeshProUGUI is null!");
            return;
        }

        if (rectTransform == null)
        {
            Debug.LogError("UIDamage: RectTransform is null!");
            return;
        }

        // ダメージテキストを設定
        textMeshPro.text = damage.ToString();
        
        // 初期位置と色を保存
        _initialPosition = rectTransform.anchoredPosition;
        _initialColor = textMeshPro.color;
        
        // 既存のタスクをキャンセル
        CancelAutoDestroy();
        
        // アニメーションと自動削除タスクを開始
        AnimateAndDestroyAsync().Forget();
    }

    /// <summary>
    /// 移動とアルファアウトのアニメーションを行い、指定時間後に自動削除
    /// </summary>
    private async UniTaskVoid AnimateAndDestroyAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            float elapsedTime = 0f;
            Vector2 targetPosition = _initialPosition + moveDirection.normalized * moveDistance;
            
            while (elapsedTime < displayDuration)
            {
                // キャンセルチェック
                token.ThrowIfCancellationRequested();
                
                // 経過時間の割合（0.0～1.0）
                float t = elapsedTime / displayDuration;
                
                // 位置を補間（線形）
                rectTransform.anchoredPosition = Vector2.Lerp(_initialPosition, targetPosition, t);
                
                // アルファ値を補間（1.0 → 0.0）
                Color currentColor = _initialColor;
                currentColor.a = Mathf.Lerp(1f, 0f, t);
                textMeshPro.color = currentColor;
                
                // フレーム待機
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                
                elapsedTime += Time.deltaTime;
            }
            
            // 最終位置とアルファ値を設定
            rectTransform.anchoredPosition = targetPosition;
            Color finalColor = _initialColor;
            finalColor.a = 0f;
            textMeshPro.color = finalColor;
            
            // 削除
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        catch (System.OperationCanceledException)
        {
            // キャンセルされた場合は何もしない
        }
    }

    /// <summary>
    /// 自動削除タスクをキャンセル
    /// </summary>
    private void CancelAutoDestroy()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    void OnDestroy()
    {
        CancelAutoDestroy();
    }
}
