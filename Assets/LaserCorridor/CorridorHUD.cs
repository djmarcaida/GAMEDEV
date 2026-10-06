using UnityEngine;

public class CorridorHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private LaserCorridorGameManager gameManager;

    private Texture2D healthBarTex;
    private Texture2D bgBarTex;
    private Texture2D boxTex;

    private string lastBonusMessage = "";
    private float bonusMessageTimer;

    private GUIStyle titleStyle;
    private GUIStyle hpStyle;
    private GUIStyle shieldStyle;
    private GUIStyle speedStyle;
    private GUIStyle bonusStyle;
    private GUIStyle centerHeadStyle;
    private GUIStyle centerSubStyle;
    private bool stylesInitialized;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<LaserCorridorGameManager>();

        if (playerHealth != null)
            playerHealth.OnBonusAcquired += OnBonusAcquired;

        healthBarTex = CreateColorTexture(new Color(0.9f, 0.15f, 0.15f, 0.9f));
        bgBarTex = CreateColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.7f));
        boxTex = CreateColorTexture(new Color(0.05f, 0.05f, 0.08f, 0.75f));
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnBonusAcquired -= OnBonusAcquired;
    }

    private void OnBonusAcquired(string message)
    {
        lastBonusMessage = message;
        bonusMessageTimer = 3f;
    }

    private void Update()
    {
        if (bonusMessageTimer > 0f)
            bonusMessageTimer -= Time.deltaTime;
    }

    private Texture2D CreateColorTexture(Color color)
    {
        var tex = new Texture2D(2, 2);
        var pixels = new Color[] { color, color, color, color };
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private void EnsureStyles()
    {
        if (stylesInitialized) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft
        };
        titleStyle.normal.textColor = Color.white;

        hpStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        hpStyle.normal.textColor = Color.white;

        shieldStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };
        shieldStyle.normal.textColor = new Color(0.2f, 0.8f, 1f);

        speedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };
        speedStyle.normal.textColor = new Color(1.0f, 0.85f, 0.1f);

        bonusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        bonusStyle.normal.textColor = Color.green;

        centerHeadStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        centerSubStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };
        centerSubStyle.normal.textColor = Color.white;

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (playerHealth == null) return;

        EnsureStyles();

        const float margin = 25f;
        const float panelW = 280f;
        const float panelH = 75f;

        GUI.DrawTexture(new Rect(margin, margin, panelW, panelH), boxTex);
        GUI.Label(new Rect(margin + 12, margin + 8, 200, 20), "VITAL SIGNS", titleStyle);

        float barX = margin + 12;
        float barY = margin + 32;
        float barW = panelW - 24;
        const float barH = 18f;

        GUI.DrawTexture(new Rect(barX, barY, barW, barH), bgBarTex);

        float hpPercent = Mathf.Clamp01((float)playerHealth.CurrentHealth / playerHealth.MaxHealth);
        Color hpColor = hpPercent > 0.4f ? new Color(0.2f, 0.85f, 0.3f) : new Color(0.95f, 0.2f, 0.2f);

        GUI.color = hpColor;
        GUI.DrawTexture(new Rect(barX, barY, barW * hpPercent, barH), healthBarTex);
        GUI.color = Color.white;

        GUI.Label(new Rect(barX, barY - 1, barW, barH), $"{playerHealth.CurrentHealth} / {playerHealth.MaxHealth} HP", hpStyle);

        float buffY = margin + panelH + 10f;

        if (playerHealth.HasShield)
        {
            GUI.Label(new Rect(margin, buffY, 300, 24), $"SHIELD ({playerHealth.ShieldTimeRemaining:F1}s)", shieldStyle);
            buffY += 24f;
        }

        if (playerHealth.SpeedBoostTimeRemaining > 0f)
        {
            GUI.Label(new Rect(margin, buffY, 300, 24), $"SPEED BOOST ({playerHealth.SpeedBoostTimeRemaining:F1}s)", speedStyle);
            buffY += 24f;
        }

        if (bonusMessageTimer > 0f)
        {
            GUI.Label(new Rect(margin, buffY, 350, 24), lastBonusMessage, bonusStyle);
        }

        if (gameManager != null)
        {
            if (gameManager.State == LaserCorridorGameManager.GameState.GameOver)
            {
                DrawCenterOverlay("GAME OVER", "Resetting to corridor start...", new Color(1f, 0.2f, 0.2f));
            }
            else if (gameManager.State == LaserCorridorGameManager.GameState.Victory)
            {
                DrawCenterOverlay("CORRIDOR CLEARED", "Reached the exit safely.", new Color(0.2f, 0.95f, 0.35f));
            }
        }
    }

    private void DrawCenterOverlay(string title, string subtitle, Color titleColor)
    {
        const float w = 520f;
        const float h = 110f;
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.4f;

        GUI.DrawTexture(new Rect(x, y, w, h), boxTex);

        centerHeadStyle.normal.textColor = titleColor;
        GUI.Label(new Rect(x, y + 16, w, 40), title, centerHeadStyle);
        GUI.Label(new Rect(x, y + 62, w, 30), subtitle, centerSubStyle);
    }
}
