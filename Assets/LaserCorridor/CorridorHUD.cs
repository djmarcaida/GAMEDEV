using UnityEngine;

/// <summary>
/// Minimal first-person HUD: a thin health bar, a detection meter that only appears while a hazard is
/// noticing the player, a crosshair dot, a pickup prompt and short status messages.
/// </summary>
public class CorridorHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private LaserCorridorGameManager gameManager;

    private static readonly Color Calm = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color Warning = new Color(1f, 0.7f, 0.15f, 0.95f);
    private static readonly Color Danger = new Color(1f, 0.2f, 0.15f, 1f);

    private Player player;
    private Texture2D pixel;
    private GUIStyle smallStyle;
    private GUIStyle titleStyle;
    private float detectionAlpha;
    private string bonusMessage = "";
    private float bonusTimer;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (gameManager == null)
            gameManager = FindAnyObjectByType<LaserCorridorGameManager>();
        player = FindAnyObjectByType<Player>();

        if (playerHealth != null)
            playerHealth.OnBonusAcquired += OnBonusAcquired;

        pixel = new Texture2D(1, 1);
        pixel.SetPixel(0, 0, Color.white);
        pixel.Apply();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnBonusAcquired -= OnBonusAcquired;
    }

    private void OnBonusAcquired(string message)
    {
        bonusMessage = message;
        bonusTimer = 2.5f;
    }

    private void Update()
    {
        if (bonusTimer > 0f)
            bonusTimer -= Time.deltaTime;

        float suspicion = StealthManager.Instance != null ? StealthManager.Instance.HighestSuspicion : 0f;
        detectionAlpha = Mathf.MoveTowards(detectionAlpha, suspicion > 0.01f ? 1f : 0f, Time.deltaTime * 3f);
    }

    private void OnGUI()
    {
        if (smallStyle == null)
        {
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 34, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        }

        DrawHealth();
        DrawDetection();
        DrawCrosshairAndPrompt();
        DrawMessages();
    }

    private void DrawHealth()
    {
        if (playerHealth == null) return;

        const float w = 180f, h = 5f, margin = 28f;
        float y = Screen.height - margin;
        float pct = Mathf.Clamp01((float)playerHealth.CurrentHealth / playerHealth.MaxHealth);

        Fill(new Rect(margin, y, w, h), new Color(0f, 0f, 0f, 0.5f));
        Fill(new Rect(margin, y, w * pct, h), pct > 0.35f ? Calm : Danger);

        smallStyle.alignment = TextAnchor.MiddleLeft;
        Label(new Rect(margin, y - 24f, 300f, 20f), playerHealth.CurrentHealth.ToString(), smallStyle, Calm);
        smallStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void DrawDetection()
    {
        var stealth = StealthManager.Instance;
        if (stealth == null || detectionAlpha <= 0f) return;

        float suspicion = stealth.HighestSuspicion;
        bool alerted = stealth.HighestState == StealthHazardBase.HazardAlertState.Alerted;
        Color c = alerted ? Danger : Color.Lerp(Calm, Warning, suspicion);
        c.a *= detectionAlpha;

        const float w = 220f, h = 4f;
        float x = (Screen.width - w) * 0.5f;
        Fill(new Rect(x, 32f, w, h), new Color(0f, 0f, 0f, 0.5f * detectionAlpha));
        Fill(new Rect(x, 32f, w * suspicion, h), c);

        if (alerted)
            Label(new Rect(x, 40f, w, 20f), "DETECTED", smallStyle, c);
    }

    private void DrawCrosshairAndPrompt()
    {
        if (player != null && player.isThirdPerson) return;

        float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
        Fill(new Rect(cx - 1.5f, cy - 1.5f, 3f, 3f), new Color(1f, 1f, 1f, 0.6f));

        var target = StealthManager.Instance != null ? StealthManager.Instance.CurrentInteractable : null;
        if (target != null)
            Label(new Rect(cx - 200f, cy + 24f, 400f, 22f), $"[E]  {target.ObjectName}", smallStyle, Calm);
    }

    private void DrawMessages()
    {
        var stealth = StealthManager.Instance;
        string message = stealth != null && !string.IsNullOrEmpty(stealth.StatusNotification) ? stealth.StatusNotification
            : bonusTimer > 0f ? bonusMessage : "";
        if (message.Length > 0)
            Label(new Rect(0f, Screen.height - 90f, Screen.width, 22f), message, smallStyle, Calm);

        if (gameManager == null) return;

        if (gameManager.State == LaserCorridorGameManager.GameState.GameOver)
        {
            DrawCentre("YOU WERE CAUGHT", "Restarting...", Danger);
        }
        else if (gameManager.State == LaserCorridorGameManager.GameState.Victory)
        {
            int found = stealth != null ? stealth.SecretsCollected : 0;
            int total = stealth != null ? stealth.TotalSecrets : 0;
            DrawCentre("ESCAPED", $"Intel recovered: {found} / {total}", Calm);
        }
    }

    private void DrawCentre(string title, string subtitle, Color color)
    {
        float y = Screen.height * 0.4f;
        Label(new Rect(0f, y, Screen.width, 44f), title, titleStyle, color);
        Label(new Rect(0f, y + 44f, Screen.width, 24f), subtitle, smallStyle, Calm);
    }

    private void Fill(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = Color.white;
    }

    /// <summary>Text with a soft drop shadow so it reads against both dark and lit areas.</summary>
    private static void Label(Rect rect, string text, GUIStyle style, Color color)
    {
        style.normal.textColor = new Color(0f, 0f, 0f, color.a * 0.7f);
        GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
        style.normal.textColor = color;
        GUI.Label(rect, text, style);
    }
}
