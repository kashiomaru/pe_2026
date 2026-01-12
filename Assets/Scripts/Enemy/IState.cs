using UnityEngine;

/// <summary>
/// ステートのインターフェース
/// </summary>
public interface IState
{
    /// <summary>
    /// ステートに入った時に呼ばれる
    /// </summary>
    void Enter();
    
    /// <summary>
    /// 毎フレーム呼ばれる
    /// </summary>
    void Tick();
    
    /// <summary>
    /// ステートから出る時に呼ばれる
    /// </summary>
    void Exit();
}
