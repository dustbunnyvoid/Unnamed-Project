using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControlsHUD : MonoBehaviour
{
    public float holdWhiteDuration = 1.5f;
    public float fadeDuration = 2.5f;
    public Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    Text[] _texts;

    void Start()
    {
        _texts = GetComponentsInChildren<Text>(true);
        foreach (var t in _texts) t.color = Color.white;
        StartCoroutine(FadeToGray());
    }

    IEnumerator FadeToGray()
    {
        yield return new WaitForSeconds(holdWhiteDuration);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Color c = Color.Lerp(Color.white, grayColor, elapsed / fadeDuration);
            foreach (var t in _texts) t.color = c;
            yield return null;
        }
        foreach (var t in _texts) t.color = grayColor;
    }
}
