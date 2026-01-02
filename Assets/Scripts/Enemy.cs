using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHp = 3;
    private int _currentHp;

    void Start()
    {
        _currentHp = maxHp;
    }

    // ダメージを受ける処理
    public void TakeDamage(int damage)
    {
        _currentHp -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! HP: {_currentHp}");

        // 被弾演出（赤く点滅など）を入れるならここ

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy Defeated!");
        // 死亡エフェクトや音を入れるならここ
        Destroy(gameObject);
    }
}