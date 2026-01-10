using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class RangeDomeController : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float scaleUpDuration = 0.2f; // スケールアップにかかる時間（秒）
    
    private Vector3 _initialScale = Vector3.zero; // 初期スケール（0から開始）
    private Vector3 _targetScaleVector; // 目標スケール
    private CancellationTokenSource _scaleAnimationCts; // アニメーションのキャンセル用
    
    void Awake()
    {
        // 初期状態は非表示でスケール0
        _initialScale = Vector3.zero;
        transform.localScale = _initialScale;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// ドームを表示し、0.2秒かけてスケールアップする
    /// </summary>
    /// <param name="scale">目標スケールサイズ（指定しない場合は5.0fを使用）</param>
    public void Show(float scale = 5.0f)
    {
        // 既存のアニメーションをキャンセル
        CancelScaleAnimation();
        
        // 目標スケールを設定
        _targetScaleVector = Vector3.one * scale;
        
        // オブジェクトをアクティブにする
        gameObject.SetActive(true);
        
        // スケールを0にリセット
        transform.localScale = _initialScale;
        
        // スケールアップアニメーションを開始
        ScaleUpAsync().Forget();
    }
    
    /// <summary>
    /// ドームを即座に非表示にする
    /// </summary>
    public void Hide()
    {
        // アニメーションをキャンセル
        CancelScaleAnimation();
        
        // 即座に非表示
        gameObject.SetActive(false);
        
        // スケールをリセット
        transform.localScale = _initialScale;
    }
    
    /// <summary>
    /// スケールアップアニメーション（0.2秒）
    /// </summary>
    private async UniTaskVoid ScaleUpAsync()
    {
        _scaleAnimationCts = new CancellationTokenSource();
        var token = _scaleAnimationCts.Token;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < scaleUpDuration)
        {
            // キャンセルチェック
            if (token.IsCancellationRequested)
            {
                return;
            }
            
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / scaleUpDuration);
            
            // イージング関数（EaseOut）を使用してスムーズなアニメーション
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // EaseOut Cubic
            
            // スケールを補間
            transform.localScale = Vector3.Lerp(_initialScale, _targetScaleVector, easedT);
            
            await UniTask.Yield();
        }
        
        // 最終的に目標スケールに設定
        transform.localScale = _targetScaleVector;
    }
    
    /// <summary>
    /// スケールアニメーションをキャンセル
    /// </summary>
    private void CancelScaleAnimation()
    {
        if (_scaleAnimationCts != null)
        {
            _scaleAnimationCts.Cancel();
            _scaleAnimationCts.Dispose();
            _scaleAnimationCts = null;
        }
    }
    
    void OnDestroy()
    {
        CancelScaleAnimation();
    }
    
}
