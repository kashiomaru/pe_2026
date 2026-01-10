using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // シングルトンインスタンス

    void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("BattleManager instance already exists. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 敵にダメージを与える
    /// </summary>
    /// <param name="enemy">ダメージを受ける敵</param>
    /// <param name="damage">ダメージ量</param>
    /// <param name="attacker">攻撃者（プレイヤーなど、オプショナル）</param>
    public void DealDamageToEnemy(Enemy enemy, int damage, GameObject attacker = null)
    {
        if (enemy == null)
        {
            Debug.LogWarning("BattleManager: Enemy is null");
            return;
        }

        // 敵のHPを取得
        int currentHp = enemy.GetCurrentHp();
        int maxHp = enemy.GetMaxHp();
        
        // ダメージを適用
        int newHp = currentHp - damage;
        enemy.SetCurrentHp(newHp);
        
        Debug.Log($"{enemy.gameObject.name} took {damage} damage! HP: {currentHp} -> {newHp}/{maxHp}");

        // ダメージ表示（UIManager経由）
        if (UIManager.Instance != null)
        {
            // 敵の位置にダメージテキストを表示
            Vector3 enemyPosition = enemy.transform.position + Vector3.up * 1.5f; // 敵の頭上に表示
            UIManager.Instance.ShowDamageText(damage, enemyPosition);
        }

        // 被弾演出（赤く点滅など）を入れるならここ
        // 例: enemy.PlayHitEffect();

        // HPが0以下になったら死亡処理
        if (newHp <= 0)
        {
            OnEnemyDefeated(enemy, attacker);
        }
    }

    /// <summary>
    /// プレイヤーにダメージを与える（将来的に実装）
    /// </summary>
    /// <param name="player">ダメージを受けるプレイヤー</param>
    /// <param name="damage">ダメージ量</param>
    /// <param name="attacker">攻撃者（敵など、オプショナル）</param>
    public void DealDamageToPlayer(PlayerController player, int damage, GameObject attacker = null)
    {
        if (player == null)
        {
            Debug.LogWarning("BattleManager: Player is null");
            return;
        }

        // プレイヤーのダメージ処理（将来的に実装）
        // 例: player.TakeDamage(damage);
        Debug.Log($"Player took {damage} damage from {attacker?.name ?? "unknown"}");
        
        // ダメージ表示（UIManager経由）
        if (UIManager.Instance != null)
        {
            // プレイヤーの位置にダメージテキストを表示
            Vector3 playerPosition = player.transform.position + Vector3.up * 1.5f; // プレイヤーの頭上に表示
            UIManager.Instance.ShowDamageText(damage, playerPosition);
        }
    }

    /// <summary>
    /// 敵が倒された時の処理
    /// </summary>
    private void OnEnemyDefeated(Enemy enemy, GameObject attacker)
    {
        if (enemy == null) return;
        
        Debug.Log($"Enemy {enemy.gameObject.name} Defeated!");
        
        // 死亡エフェクトや音を入れるならここ
        // 例: enemy.PlayDeathEffect();
        
        // 敵の死亡処理を呼び出す
        enemy.Die();
        
        // クリア条件チェックなど、将来的に実装
        // CheckClearCondition();
    }
}
