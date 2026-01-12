/// <summary>
/// ステートマシンクラス
/// </summary>
public class StateMachine
{
    private IState _current;
    
    /// <summary>
    /// 現在のステートを変更する
    /// </summary>
    /// <param name="next">次のステート</param>
    public void ChangeState(IState next)
    {
        _current?.Exit();
        _current = next;
        _current?.Enter();
    }
    
    /// <summary>
    /// 現在のステートのTickを呼び出す
    /// </summary>
    public void Tick() => _current?.Tick();
    
    /// <summary>
    /// 現在のステートを取得する
    /// </summary>
    public IState GetCurrentState() => _current;
}
