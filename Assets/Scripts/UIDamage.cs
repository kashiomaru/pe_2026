using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIDamage : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayDuration = 1.0f; // 表示時間（秒）
    
    private TextMeshProUGUI _textMeshPro;
    private CancellationTokenSource _cancellationTokenSource;

    void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        if (_textMeshPro == null)
        {
            Debug.LogError("UIDamage: TextMeshProUGUI component not found!");
        }
    }

    /// <summary>
    /// ダメージを表示して、指定時間後に自動削除
    /// </summary>
    /// <param name="damage">表示するダメージ値</param>
    public void ShowDamage(int damage)
    {
        if (_textMeshPro == null)
        {
            Debug.LogError("UIDamage: TextMeshProUGUI is null!");
            return;
        }

        // ダメージテキストを設定
        _textMeshPro.text = damage.ToString();
        
        // 既存のタスクをキャンセル
        CancelAutoDestroy();
        
        // 自動削除タスクを開始
        AutoDestroyAsync().Forget();
    }

    /// <summary>
    /// 指定時間後に自動削除
    /// </summary>
    private async UniTaskVoid AutoDestroyAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            // 指定時間待機
            await UniTask.Delay(System.TimeSpan.FromSeconds(displayDuration), cancellationToken: token);
            
            // 削除（UIManager経由で削除される想定だが、念のため自分で削除）
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
