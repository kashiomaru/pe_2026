using UnityEngine;

/// <summary>
/// 敵の基本クラス（HP管理など）
/// 移動はNavMeshAgent（EnemyBrain）が制御する
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Combat")]
    public int maxHp = 3;
    
    private int _currentHp;

    void Start()
    {
        _currentHp = maxHp;
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