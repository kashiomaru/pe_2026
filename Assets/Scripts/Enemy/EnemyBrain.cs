using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

/// <summary>
/// 敵のAIブレイン（ステートマシンを使用）
/// </summary>
public class EnemyBrain : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolRadius = 8f;
    public float minPatrolDistance = 2f;
    public Vector2 waitTimeRange = new Vector2(0.5f, 1.5f);

    private NavMeshAgent _agent;
    private Vector3 _homePos;
    private StateMachine _sm;

    private EnemyPatrolState _patrol;
    private bool _isInitialized = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _homePos = transform.position;

        _sm = new StateMachine();
        _patrol = new EnemyPatrolState(_agent, _homePos, patrolRadius, minPatrolDistance, waitTimeRange);
    }

    private void OnEnable()
    {
        _isInitialized = false;
        // NavMeshが利用可能になるまで少し待つ
        InitializeAfterNavMeshReadyAsync().Forget();
    }

    private async UniTaskVoid InitializeAfterNavMeshReadyAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        
        // NavMeshが利用可能になるまで最大1秒待つ
        float timeout = 1f;
        float elapsed = 0f;
        
        while (!_agent.isOnNavMesh && elapsed < timeout)
        {
            await UniTask.Yield(token);
            elapsed += Time.deltaTime;
        }
        
        // NavMesh上に配置されていない場合は配置を試みる
        if (!_agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2.0f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }
        
        _isInitialized = true;
        _sm.ChangeState(_patrol);
    }

    private void Update()
    {
        if (_isInitialized)
        {
            _sm.Tick();
        }
    }
}
