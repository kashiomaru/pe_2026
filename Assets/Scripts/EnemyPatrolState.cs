using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵のパトロールステート
/// </summary>
public class EnemyPatrolState : IState
{
    private readonly NavMeshAgent _agent;
    private readonly Vector3 _homePos;
    private readonly float _radius;
    private readonly float _minDist;
    private readonly Vector2 _waitRange;

    private float _waitTimer;
    private float _waitDuration;

    public EnemyPatrolState(NavMeshAgent agent, Vector3 homePos, float radius, float minDist, Vector2 waitRange)
    {
        _agent = agent;
        _homePos = homePos;
        _radius = radius;
        _minDist = minDist;
        _waitRange = waitRange;
    }

    public void Enter()
    {
        // NavMesh上に配置されていない場合は配置する
        if (!_agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(_agent.transform.position, out var hit, 2.0f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
            else
            {
                // NavMesh上に配置できない場合は停止状態のままにする
                _agent.isStopped = true;
                return;
            }
        }
        
        _agent.isStopped = false;
        PickNextDestination();
    }

    public void Tick()
    {
        Debug.Log($"Path pending: {_agent.pathPending}");
        if (_agent.pathPending) return;

        // 到着判定
        bool arrived =
            _agent.hasPath == false &&
            _agent.remainingDistance <= _agent.stoppingDistance + 0.1f &&
            _agent.velocity.sqrMagnitude < 0.01f;

        if (!arrived) return;

        // 到着後、少し待ってから次の目的地
        if (_waitTimer <= 0f)
        {
            _waitDuration = Random.Range(_waitRange.x, _waitRange.y);
            _waitTimer = _waitDuration;
        }

        _waitTimer -= Time.deltaTime;
        if (_waitTimer <= 0f)
        {
            PickNextDestination();
        }
    }

    public void Exit()
    {
        // 特に何もしない（後で必要なら停止とか）
    }

    private void PickNextDestination()
    {
        // NavMesh上に配置されていない場合は処理をスキップ
        if (!_agent.isOnNavMesh)
        {
            return;
        }
        
        _waitTimer = 0f;

        for (int i = 0; i < 6; i++)
        {
            Vector3 random = _homePos + new Vector3(
                Random.Range(-_radius, _radius),
                0f,
                Random.Range(-_radius, _radius)
            );

            if (NavMesh.SamplePosition(random, out var hit, 2.0f, NavMesh.AllAreas))
            {
                // 近すぎる目的地は避ける
                if (Vector3.Distance(_agent.transform.position, hit.position) < _minDist)
                    continue;

                _agent.SetDestination(hit.position);
                return;
            }
        }

        // 失敗したらホームに戻す（保険）
        if (NavMesh.SamplePosition(_homePos, out var homeHit, 2.0f, NavMesh.AllAreas))
        {
            _agent.SetDestination(homeHit.position);
        }
    }
}
