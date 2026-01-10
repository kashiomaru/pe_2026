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
        // 重力を常に適用
        ApplyGravity();
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