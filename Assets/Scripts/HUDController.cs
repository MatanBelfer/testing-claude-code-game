using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    Text scoreText;
    Text hitsText;
    GameObject gameOverPanel;

    void Start()
    {
        BuildUI();
    }

    void BuildUI()
    {
        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemGo.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        var canvasGo = new GameObject("HUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        scoreText = CreateText(canvasGo.transform, "ScoreText", new Vector2(0, 1), new Vector2(20, -20), "Score: 0");
        hitsText = CreateText(canvasGo.transform, "HitsText", new Vector2(0, 1), new Vector2(20, -55), "Hits: 0 / " + GameConfig.MaxCityHits);

        BuildGameOverPanel(canvasGo.transform);
        gameOverPanel.SetActive(false);
    }

    void BuildGameOverPanel(Transform parent)
    {
        gameOverPanel = new GameObject("GameOverPanel", typeof(Image));
        gameOverPanel.transform.SetParent(parent, false);
        var panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        gameOverPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        var titleText = CreateText(gameOverPanel.transform, "GameOverText", new Vector2(0.5f, 0.6f), Vector2.zero, "GAME OVER");
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontSize = 48;
        var titleRect = titleText.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(600, 100);

        var buttonGo = new GameObject("RestartButton", typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(gameOverPanel.transform, false);
        var btnRect = buttonGo.GetComponent<RectTransform>();
        btnRect.anchorMin = btnRect.anchorMax = new Vector2(0.5f, 0.45f);
        btnRect.sizeDelta = new Vector2(200, 60);
        buttonGo.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.9f);
        buttonGo.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.RestartGame());

        var btnText = CreateText(buttonGo.transform, "RestartText", new Vector2(0.5f, 0.5f), Vector2.zero, "Restart");
        btnText.alignment = TextAnchor.MiddleCenter;
        var btnTextRect = btnText.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
    }

    Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 anchoredPos, string content)
    {
        var go = new GameObject(name, typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(400, 50);

        var text = go.GetComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.color = Color.white;
        return text;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        scoreText.text = "Score: " + GameManager.Instance.Score;
        hitsText.text = "Hits: " + GameManager.Instance.TotalHits + " / " + GameConfig.MaxCityHits;

        if (GameManager.Instance.IsGameOver && !gameOverPanel.activeSelf)
            gameOverPanel.SetActive(true);
    }
}
