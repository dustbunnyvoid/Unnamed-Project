using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    public Slider healthSlider;
    public Text nameLabel;
    public GameObject panel;

    LockOnSystem _lockOn;
    HealthComponent _current;

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) _lockOn = player.GetComponent<LockOnSystem>();
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (_lockOn == null) return;

        var target = _lockOn.TargetEnemy;

        if (target == null)
        {
            SetTarget(null);
            return;
        }

        var health = target.GetComponent<HealthComponent>();
        if (health != _current)
            SetTarget(target);

        if (healthSlider != null && _current != null)
            healthSlider.value = _current.NormalizedHealth;
    }

    void SetTarget(GameObject enemy)
    {
        if (_current != null)
            _current.OnDeath -= OnTargetDied;

        _current = enemy != null ? enemy.GetComponent<HealthComponent>() : null;

        if (_current != null)
        {
            _current.OnDeath += OnTargetDied;
            if (nameLabel != null) nameLabel.text = enemy.name.ToUpper();
            if (healthSlider != null) healthSlider.value = _current.NormalizedHealth;
        }

        if (panel != null) panel.SetActive(_current != null);
    }

    void OnTargetDied() => SetTarget(null);
}
