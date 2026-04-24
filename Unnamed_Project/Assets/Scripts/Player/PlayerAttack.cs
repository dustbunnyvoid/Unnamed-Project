using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.8f;
    public float attackDamage = 34f;
    public float attackCooldown = 0.5f;

    float _cooldownTimer;

    void Update()
    {
        _cooldownTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && _cooldownTimer <= 0f)
            Attack();
    }

    void Attack()
    {
        _cooldownTimer = attackCooldown;

        Vector3 hitCenter = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(hitCenter, attackRange * 0.5f);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            var health = hit.GetComponent<HealthComponent>();
            if (health == null) continue;
            health.TakeDamage(attackDamage);
            FXHelper.SpawnBurst(hit.transform.position + Vector3.up, new Color(1f, 0.35f, 0f));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange * 0.5f);
    }
}
