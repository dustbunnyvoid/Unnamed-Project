using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthComponent))]
public class SwingerAI : MonoBehaviour
{
    [Header("Movement")]
    public float approachStopDistance = 2.5f;

    [Header("Attack")]
    public float attackRange = 2.8f;
    public float attackDamage = 25f;
    public float swingArcAngle = 100f;

    [Header("Timing")]
    public float windupDuration = 1.2f;
    public float swingDuration = 0.25f;
    public float cooldownDuration = 1.8f;

    enum State { Approach, Windup, Swing, Cooldown }
    State _state = State.Approach;
    float _stateTimer;

    NavMeshAgent _agent;
    Transform _player;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = approachStopDistance;
    }

    void Start()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
        GetComponent<HealthComponent>().OnDeath += Die;
    }

    void Update()
    {
        if (_player == null) return;

        _stateTimer -= Time.deltaTime;

        switch (_state)
        {
            case State.Approach:
                _agent.SetDestination(_player.position);
                if (Vector3.Distance(transform.position, _player.position) <= approachStopDistance)
                    EnterState(State.Windup, windupDuration);
                break;

            case State.Windup:
                _agent.ResetPath();
                FacePlayer();
                if (_stateTimer <= 0f)
                    Swing();
                break;

            case State.Swing:
                if (_stateTimer <= 0f)
                    EnterState(State.Cooldown, cooldownDuration);
                break;

            case State.Cooldown:
                if (_stateTimer <= 0f)
                    EnterState(State.Approach, 0f);
                break;
        }
    }

    void EnterState(State next, float duration)
    {
        _state = next;
        _stateTimer = duration;
    }

    void Swing()
    {
        EnterState(State.Swing, swingDuration);

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            float angle = Vector3.Angle(transform.forward, hit.transform.position - transform.position);
            if (angle <= swingArcAngle * 0.5f)
                hit.GetComponent<HealthComponent>()?.TakeDamage(attackDamage);
        }
    }

    void FacePlayer()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
    }

    void Die()
    {
        _agent.enabled = false;
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
