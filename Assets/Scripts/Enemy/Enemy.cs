using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵の基本クラス（HP管理など）
/// 移動はNavMeshAgent（EnemyBrain）が制御する
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Combat")]
    public int maxHp = 3;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    
    private int _currentHp;

    void Start()
    {
        _currentHp = maxHp;
        
        // Animatorが設定されていない場合は自動取得
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // NavMeshAgentが設定されていない場合は自動取得
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }
    
    void Update()
    {
        // AnimatorとNavMeshAgentが存在する場合、Speedパラメータを更新
        if (animator != null && agent != null)
        {
            UpdateAnimatorSpeed();
        }
    }
    
    /// <summary>
    /// NavMeshAgentの速度に基づいてAnimatorのSpeedパラメータを更新
    /// PlayerControllerと同じように0=Idle, 0.5=Walk, 1.0=Runになるように正規化
    /// </summary>
    private void UpdateAnimatorSpeed()
    {
        // NavMeshAgentの現在の速度を取得
        float currentSpeed = agent.velocity.magnitude;
        
        // 最大速度で正規化（0.0～1.0の範囲に）
        float normalizedSpeed = 0f;
        if (agent.speed > 0f)
        {
            normalizedSpeed = Mathf.Clamp01(currentSpeed / agent.speed);
        }
        
        // PlayerControllerと同じように、DampTimeを使って数値の急変を防ぐ
        animator.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);
    }

    // ダメージを受ける処理（BattleManager経由で呼ばれる）
    public void TakeDamage(int damage)
    {
        // BattleManagerを通してダメージ処理を行う
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.DealDamageToEnemy(this, damage);
        }
        else
        {
            // BattleManagerが存在しない場合は直接処理（フォールバック）
            Debug.LogWarning("BattleManager not found. Using fallback damage handling.");
            _currentHp -= damage;
            if (_currentHp <= 0)
            {
                Die();
            }
        }
    }
    
    // BattleManagerから呼ばれる：現在のHPを取得
    public int GetCurrentHp()
    {
        return _currentHp;
    }
    
    // BattleManagerから呼ばれる：現在のHPを設定
    public void SetCurrentHp(int hp)
    {
        _currentHp = Mathf.Max(0, hp);
        // 死亡処理はBattleManagerで行うため、ここでは呼ばない
    }
    
    // BattleManagerから呼ばれる：最大HPを取得
    public int GetMaxHp()
    {
        return maxHp;
    }

    public void Die()
    {
        Debug.Log("Enemy Defeated!");
        // 死亡エフェクトや音を入れるならここ
        Destroy(gameObject);
    }
}