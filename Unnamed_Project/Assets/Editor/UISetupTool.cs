#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

public static class UISetupTool
{
    static readonly Color PanelBg       = new Color(0f,    0f,    0f,    0.60f);
    static readonly Color LabelColor    = new Color(0.85f, 0.85f, 0.85f, 1f);
    static readonly Color ControlsColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    static readonly Color BarBg         = new Color(0.15f, 0.15f, 0.15f, 1f);
    static readonly Color PlayerFill    = new Color(0.20f, 0.82f, 0.25f, 1f);
    static readonly Color EnemyFill     = new Color(0.85f, 0.20f, 0.20f, 1f);

    [MenuItem("Game/Setup Battle UI")]
    static void SetupBattleUI()
    {
        var canvasGo = EnsureCanvas();
        var t = canvasGo.transform;

        EnsureEventSystem();

        // Destroy old panels so the tool is idempotent
        DestroyChild(t, "PlayerPanel");
        DestroyChild(t, "EnemyPanel");
        DestroyChild(t, "ControlsPanel");
        DestroyChild(t, "DamageFlash");
        DestroyChild(t, "EnemyIntroPanel");

        // EnemyHealthUI and RoomIntroUI live on the Canvas root; remove stale instances
        foreach (var stale in canvasGo.GetComponents<EnemyHealthUI>())
            Object.DestroyImmediate(stale);
        foreach (var stale in canvasGo.GetComponents<RoomIntroUI>())
            Object.DestroyImmediate(stale);

        BuildPlayerPanel(t);
        BuildEnemyPanel(canvasGo, t);
        BuildControlsPanel(t);
        BuildDamageFlash(t);
        BuildIntroPanel(canvasGo, t);

        EditorUtility.SetDirty(canvasGo);
        Debug.Log("[UISetupTool] Battle UI built. Save the scene to keep it.");
    }

    // -------------------------------------------------------------------------
    //  Player panel — top-left
    // -------------------------------------------------------------------------
    static void BuildPlayerPanel(Transform canvas)
    {
        var panel = MakePanel(canvas, "PlayerPanel",
            anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
            pos: new Vector2(20, -20), size: new Vector2(220, 72));

        MakeLabel(panel.transform, "TitleLabel", "PLAYER",
            anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
            pos: new Vector2(10, -8), size: new Vector2(200, 22),
            fontSize: 13, color: LabelColor, bold: true);

        var slider = MakeHealthSlider(panel.transform, PlayerFill, yOffset: -40);

        var hb = panel.AddComponent<HealthBar>();
        hb.slider = slider;
    }

    // -------------------------------------------------------------------------
    //  Enemy panel — top-right
    //  EnemyHealthUI goes on the Canvas root (always active) so its Start()
    //  runs even while the panel itself is hidden.
    // -------------------------------------------------------------------------
    static void BuildEnemyPanel(GameObject canvasGo, Transform canvas)
    {
        var panel = MakePanel(canvas, "EnemyPanel",
            anchor: new Vector2(1, 1), pivot: new Vector2(1, 1),
            pos: new Vector2(-20, -20), size: new Vector2(220, 72));

        var nameLbl = MakeLabel(panel.transform, "EnemyNameLabel", "—",
            anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
            pos: new Vector2(10, -8), size: new Vector2(200, 22),
            fontSize: 13, color: LabelColor, bold: true);

        var slider = MakeHealthSlider(panel.transform, EnemyFill, yOffset: -40);

        // Component on Canvas root so it runs while panel is inactive
        var ehui = canvasGo.AddComponent<EnemyHealthUI>();
        ehui.panel        = panel;
        ehui.nameLabel    = nameLbl;
        ehui.healthSlider = slider;

        panel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    //  Controls panel — bottom-right
    // -------------------------------------------------------------------------
    static void BuildControlsPanel(Transform canvas)
    {
        var panel = new GameObject("ControlsPanel");
        panel.transform.SetParent(canvas, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(1, 0);
        rt.anchorMax        = new Vector2(1, 0);
        rt.pivot            = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-20, 20);
        rt.sizeDelta        = new Vector2(180, 0);

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment        = TextAnchor.LowerRight;
        vlg.spacing               = 4;
        vlg.childControlHeight    = false;
        vlg.childControlWidth     = false;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth  = false;
        vlg.padding               = new RectOffset(0, 0, 0, 0);

        var csf = panel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        string[] lines = { "WASD  —  Move", "LMB  —  Attack", "T  —  Lock On", "Y  —  Release" };
        foreach (var line in lines)
        {
            var go = new GameObject(line);
            go.transform.SetParent(panel.transform, false);
            var lrt = go.AddComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(180, 18);
            var txt = go.AddComponent<Text>();
            txt.text      = line;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize  = 12;
            txt.color     = ControlsColor;
            txt.alignment = TextAnchor.MiddleRight;
        }

        panel.AddComponent<ControlsHUD>();
    }

    // -------------------------------------------------------------------------
    //  Damage flash — full-screen red overlay
    // -------------------------------------------------------------------------
    static void BuildDamageFlash(Transform canvas)
    {
        var go = new GameObject("DamageFlash");
        go.transform.SetParent(canvas, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color         = new Color(1f, 0f, 0f, 0f);
        img.raycastTarget = false;

        var df = go.AddComponent<DamageFlash>();
        df.flashImage = img;
    }

    // -------------------------------------------------------------------------
    //  Enemy intro panel — centered, fades in/out between waves
    //  RoomIntroUI lives on Canvas root so it's always active.
    // -------------------------------------------------------------------------
    static void BuildIntroPanel(GameObject canvasGo, Transform canvas)
    {
        // Outer panel — centered, slightly above screen midpoint
        var panel = new GameObject("EnemyIntroPanel");
        panel.transform.SetParent(canvas, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 80f);
        rt.sizeDelta        = new Vector2(520f, 130f);

        var bg = panel.AddComponent<Image>();
        bg.color         = new Color(0f, 0f, 0f, 0.72f);
        bg.raycastTarget = false;

        var cg = panel.AddComponent<CanvasGroup>();
        cg.alpha         = 0f;
        cg.blocksRaycasts = false;

        // Title
        var titleLbl = MakeLabel(panel.transform, "TitleLabel", "ENEMY NAME",
            anchor: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
            pos: new Vector2(0f, -16f), size: new Vector2(490f, 40f),
            fontSize: 24, color: Color.white, bold: true);
        titleLbl.alignment = TextAnchor.MiddleCenter;

        // Description
        var descLbl = MakeLabel(panel.transform, "DescLabel", "Description goes here.",
            anchor: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
            pos: new Vector2(0f, -62f), size: new Vector2(490f, 48f),
            fontSize: 14, color: new Color(0.80f, 0.80f, 0.80f, 1f));
        descLbl.alignment  = TextAnchor.UpperCenter;
        descLbl.horizontalOverflow = HorizontalWrapMode.Wrap;

        // Wire RoomIntroUI on Canvas root
        var ui = canvasGo.AddComponent<RoomIntroUI>();
        ui.titleText      = titleLbl;
        ui.descriptionText = descLbl;
        ui.canvasGroup    = cg;
    }

    // =========================================================================
    //  Helpers
    // =========================================================================

    static GameObject MakePanel(Transform parent, string name,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchor;
        rt.anchorMax        = anchor;
        rt.pivot            = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        var img = go.AddComponent<Image>();
        img.color         = PanelBg;
        img.raycastTarget = false;
        return go;
    }

    static Text MakeLabel(Transform parent, string name, string text,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
        int fontSize, Color color, bool bold = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchor;
        rt.anchorMax        = anchor;
        rt.pivot            = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        var txt = go.AddComponent<Text>();
        txt.text      = text;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = fontSize;
        txt.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        txt.color     = color;
        txt.alignment = TextAnchor.MiddleLeft;
        return txt;
    }

    // Builds a fill-only slider (no handle). The Slider component is set
    // non-interactable so mouse events pass through.
    static Slider MakeHealthSlider(Transform parent, Color fillColor, float yOffset)
    {
        var root = new GameObject("HealthSlider");
        root.transform.SetParent(parent, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin        = new Vector2(0, 1);
        rootRT.anchorMax        = new Vector2(1, 1);
        rootRT.pivot            = new Vector2(0.5f, 1);
        rootRT.anchoredPosition = new Vector2(0, yOffset);
        rootRT.sizeDelta        = new Vector2(-20, 14);

        // Background track
        var bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        StretchFull(bg.AddComponent<RectTransform>());
        bg.AddComponent<Image>().color = BarBg;

        // Fill area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(root.transform, false);
        StretchFull(fillArea.AddComponent<RectTransform>());

        // Fill rect — Unity's Slider drives its width from value
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0);
        fillRT.anchorMax = new Vector2(0, 1);
        fillRT.pivot     = new Vector2(0, 0.5f);
        fillRT.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = fillColor;

        var slider = root.AddComponent<Slider>();
        slider.fillRect     = fillRT;
        slider.direction    = Slider.Direction.LeftToRight;
        slider.minValue     = 0f;
        slider.maxValue     = 1f;
        slider.value        = 1f;
        slider.interactable = false;

        return slider;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    static GameObject EnsureCanvas()
    {
        var existing = Object.FindAnyObjectByType<Canvas>();
        if (existing != null) return existing.gameObject;

        var go     = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    static void DestroyChild(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child != null) Object.DestroyImmediate(child.gameObject);
    }
}
#endif
