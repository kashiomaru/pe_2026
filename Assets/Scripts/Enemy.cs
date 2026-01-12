using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Enemy : MonoBehaviour
{
    [Header("Combat")]
    public int maxHp = 3;
    
    [Header("Physics")]
    public float gravity = -9.81f;
    
    private int _currentHp;
    private CharacterController _characterController;
    private Vector3 _velocity;

    void Start()
    {
        _currentHp = maxHp;
        _characterController = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        // NavMeshAgentがアタッチされている場合は、NavMeshAgentが移動を制御するため
        // CharacterControllerでの移動処理は行わない（重力のみ適用）
        // NavMeshAgentは自動的にTransformを更新するため、手動での移動は不要
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null || !agent.enabled)
        {
            // NavMeshAgentがない場合のみ重力を適用
            ApplyGravity();
        }
        // NavMeshAgentがある場合は、NavMeshAgentが移動を制御するため何もしない
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
    
    void ApplyGravity()
    {
        // 接地しているなら重力リセット（少し押し付ける）
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}