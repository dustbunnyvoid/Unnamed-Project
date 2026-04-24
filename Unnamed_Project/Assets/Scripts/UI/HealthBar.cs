using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    HealthComponent _health;

    void Start()
    {
        _health = GameObject.FindWithTag("Player")?.GetComponent<HealthComponent>();
        if (_health == null) return;

        _health.OnHealthChanged += UpdateBar;
        UpdateBar(_health.NormalizedHealth);
    }

    void UpdateBar(float normalized)
    {
        if (slider != null) slider.value = normalized;
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnHealthChanged -= UpdateBar;
    }
}
