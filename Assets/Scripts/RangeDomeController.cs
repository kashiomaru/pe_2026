using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

public class RangeDomeController : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float scaleUpDuration = 0.2f; // スケールアップにかかる時間（秒）
    
    private Vector3 _initialScale = Vector3.zero; // 初期スケール（0から開始）
    private Vector3 _targetScaleVector; // 目標スケール
    private CancellationTokenSource _scaleAnimationCts; // アニメーションのキャンセル用
    
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Mesh _originalMesh; // 元のメッシュ（読み取り専用の可能性があるため保持）
    private Mesh _wireframeMesh; // ワイヤフレーム用のメッシュ
    
    void Awake()
    {
        // 初期状態は非表示でスケール0
        _initialScale = Vector3.zero;
        transform.localScale = _initialScale;
        gameObject.SetActive(false);
        
        // MeshRendererとMeshFilterを取得
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        
        if (_meshFilter != null && _meshFilter.sharedMesh != null)
        {
            // 元のメッシュを保持
            _originalMesh = _meshFilter.sharedMesh;
            
            // ワイヤフレーム用のメッシュを作成
            CreateWireframeMesh();
        }
    }
    
    /// <summary>
    /// ワイヤフレーム用のメッシュを作成
    /// </summary>
    private void CreateWireframeMesh()
    {
        if (_originalMesh == null) return;
        
        // メッシュのコピーを作成（読み取り専用のメッシュを変更するため）
        _wireframeMesh = new Mesh();
        _wireframeMesh.name = _originalMesh.name + "_Wireframe";
        
        // 頂点と法線をコピー
        _wireframeMesh.vertices = _originalMesh.vertices;
        _wireframeMesh.normals = _originalMesh.normals;
        _wireframeMesh.uv = _originalMesh.uv;
        
        // エッジを抽出してLinesトポロジーに変換
        List<int> lineIndices = new List<int>();
        int[] triangles = _originalMesh.triangles;
        
        // 各三角形の3つのエッジを抽出
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];
            
            // 3つのエッジを追加（v0-v1, v1-v2, v2-v0）
            lineIndices.Add(v0);
            lineIndices.Add(v1);
            lineIndices.Add(v1);
            lineIndices.Add(v2);
            lineIndices.Add(v2);
            lineIndices.Add(v0);
        }
        
        // Linesトポロジーで設定
        _wireframeMesh.SetIndices(lineIndices.ToArray(), MeshTopology.Lines, 0);
        
        // ワイヤフレームメッシュを適用
        _meshFilter.mesh = _wireframeMesh;
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
        
        // ワイヤフレームメッシュをクリーンアップ
        if (_wireframeMesh != null)
        {
            Destroy(_wireframeMesh);
        }
    }
    
}
