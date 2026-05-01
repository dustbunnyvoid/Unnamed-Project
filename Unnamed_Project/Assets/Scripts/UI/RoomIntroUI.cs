using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoomIntroUI : MonoBehaviour
{
    public static RoomIntroUI Instance { get; private set; }

    public Text titleText;
    public Text descriptionText;
    public CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;
        SetAlpha(0f);
    }

    public IEnumerator Show(string title, string description, float holdDuration, float fadeTime)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        yield return Fade(0f, 1f, fadeTime);
        yield return new WaitForSeconds(Mathf.Max(0f, holdDuration - fadeTime * 2f));
        yield return Fade(1f, 0f, fadeTime);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = a;
            canvasGroup.blocksRaycasts = a > 0f;
        }
    }
}
