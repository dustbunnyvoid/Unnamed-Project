using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthComponent))]
public class RusherAI : MonoBehaviour
{
    [Header("Combat")]
    public float contactRange = 1.2f;
    public float contactDamage = 10f;
    public float damageInterval = 0.6f;

    NavMeshAgent _agent;
    Transform _player;
    float _damageTimer;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
        GetComponent<HealthComponent>().OnDeath += Die;
    }

    void Update()
    {
        if (_player == null) return;

        _agent.SetDestination(_player.position);

        _damageTimer -= Time.deltaTime;
        if (_damageTimer <= 0f && Vector3.Distance(transform.position, _player.position) <= contactRange)
        {
            _damageTimer = damageInterval;
            _player.GetComponent<HealthComponent>()?.TakeDamage(contactDamage);
        }
    }

    void Die()
    {
        _agent.enabled = false;
        Destroy(gameObject, 0.1f);
    }
}
