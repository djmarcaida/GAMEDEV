using UnityEngine;

/// <summary>
/// Heads-Up Display for the Laser Corridor.
/// Features immediate OnGUI rendering so it works instantly without manual UI Canvas setup,
/// with support for custom GUI styles, health bar, and active buff notifications.
/// </summary>
public class CorridorHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private LaserCorridorGameManager gameManager;

    private Texture2D healthBarTex;
    private Texture2D bgBarTex;
    private Texture2D boxTex;

    private string lastBonusMessage = "";
    private float bonusMessageTimer = 0f;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<LaserCorridorGameManager>();
        }

        if (playerHealth != null)
        {
            playerHealth.OnBonusAcquired += ShowBonusNotice;
        }

        CreateTextures();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnBonusAcquired -= ShowBonusNotice;
        }
    }

    private void ShowBonusNotice(string bonusName)
    {
        lastBonusMessage = bonusName;
        bonusMessageTimer = 3.0f;
    }

    private void Update()
    {
        if (bonusMessageTimer > 0f)
        {
            bonusMessageTimer -= Time.deltaTime;
        }
    }

    private void CreateTextures()
    {
        healthBarTex = MakeColorTex(2, 2, new Color(0.9f, 0.15f, 0.15f, 0.9f));
        bgBarTex = MakeColorTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.7f));
        boxTex = MakeColorTex(2, 2, new Color(0.05f, 0.05f, 0.08f, 0.75f));
    }

    private Texture2D MakeColorTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnGUI()
    {
        if (playerHealth == null) return;

        // --- HEALTH BAR BOX (Top-Left) ---
        float margin = 25f;
        float panelW = 280f;
        float panelH = 75f;

        GUI.DrawTexture(new Rect(margin, margin, panelW, panelH), boxTex);

        // Header Label
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft
        };
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(margin + 12, margin + 8, 200, 20), "VITAL SIGNS", titleStyle);

        // Health Bar
        float barX = margin + 12;
        float barY = margin + 32;
        float barW = panelW - 24;
        float barH = 18f;

        GUI.DrawTexture(new Rect(barX, barY, barW, barH), bgBarTex);

        float hpPercent = Mathf.Clamp01((float)playerHealth.CurrentHealth / playerHealth.MaxHealth);
        Color hpColor = hpPercent > 0.4f ? new Color(0.2f, 0.85f, 0.3f) : new Color(0.95f, 0.2f, 0.2f);
        
        GUI.color = hpColor;
        GUI.DrawTexture(new Rect(barX, barY, barW * hpPercent, barH), healthBarTex);
        GUI.color = Color.white;

        // Health Text
        GUIStyle hpStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        hpStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(barX, barY - 1, barW, barH), $"{playerHealth.CurrentHealth} / {playerHealth.MaxHealth} HP", hpStyle);

        // --- BUFF / BONUS INDICATORS (Below Health Bar) ---
        float buffY = margin + panelH + 10f;

        if (playerHealth.HasShield)
        {
            GUIStyle shieldStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            shieldStyle.normal.textColor = new Color(0.2f, 0.8f, 1f);
            GUI.Label(new Rect(margin, buffY, 300, 24), $"SHIELD ACTIVE ({playerHealth.ShieldTimeRemaining:F1}s)", shieldStyle);
            buffY += 24f;
        }

        if (playerHealth.SpeedBoostTimeRemaining > 0f)
        {
            GUIStyle speedStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            speedStyle.normal.textColor = new Color(1.0f, 0.85f, 0.1f);
            GUI.Label(new Rect(margin, buffY, 300, 24), $"SPEED BOOST ({playerHealth.SpeedBoostTimeRemaining:F1}s)", speedStyle);
            buffY += 24f;
        }

        if (bonusMessageTimer > 0f)
        {
            GUIStyle popStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            popStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(margin, buffY, 350, 24), $"[BONUS] {lastBonusMessage}", popStyle);
        }

        // --- GAME OVER & VICTORY OVERLAYS (Center Screen) ---
        if (gameManager != null)
        {
            if (gameManager.State == LaserCorridorGameManager.GameState.GameOver)
            {
                RenderCenterMessage("GAME OVER", "Eliminated by defense grid. Resetting to start position...", new Color(1f, 0.15f, 0.15f));
            }
            else if (gameManager.State == LaserCorridorGameManager.GameState.Victory)
            {
                RenderCenterMessage("CORRIDOR CLEARED!", "You reached the end of the corridor alive!", new Color(0.2f, 0.95f, 0.35f));
            }
        }
    }

    private void RenderCenterMessage(string header, string subtitle, Color col)
    {
        float centerW = 550f;
        float centerH = 120f;
        float x = (Screen.width - centerW) * 0.5f;
        float y = (Screen.height - centerH) * 0.4f;

        GUI.DrawTexture(new Rect(x, y, centerW, centerH), boxTex);

        GUIStyle headStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headStyle.normal.textColor = col;
        GUI.Label(new Rect(x, y + 15, centerW, 40), header, headStyle);

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };
        subStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + 65, centerW, 30), subtitle, subStyle);
    }
}
