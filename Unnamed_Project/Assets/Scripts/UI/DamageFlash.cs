using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    public Image flashImage;
    public float flashDuration = 0.35f;

    HealthComponent _health;

    void Start()
    {
        _health = GameObject.FindWithTag("Player")?.GetComponent<HealthComponent>();
        if (_health != null) _health.OnDamageTaken += TriggerFlash;
        if (flashImage != null) flashImage.color = new Color(1f, 0f, 0f, 0f);
    }

    void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        flashImage.color = new Color(1f, 0f, 0f, 0.45f);
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            flashImage.color = new Color(1f, 0f, 0f, Mathf.Lerp(0.45f, 0f, t / flashDuration));
            yield return null;
        }
        flashImage.color = new Color(1f, 0f, 0f, 0f);
    }

    void OnDestroy()
    {
        if (_health != null) _health.OnDamageTaken -= TriggerFlash;
    }
}
