using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CarnivalGame
{
    public class CarnivalUIController : MonoBehaviour
    {
        [SerializeField] private CarnivalBoothManager boothManager;
        [SerializeField] private FirstPersonPlayer player;

        private Canvas _canvas;
        private GameObject _menuModalPanel;
        private GameObject _hudPanel;
        private GameObject _outcomeModalPanel;

        private Text _modeTitleText;
        private Text _scoreText;
        private Text _targetsText;
        private Text _ballsText;

        private Text _outcomeBannerText;
        private Text _outcomeDetailText;

        private AudioSource _uiAudioSource;
        private AudioClip _clickSound;

        public void Configure(CarnivalBoothManager mgr, FirstPersonPlayer fpsPlayer)
        {
            boothManager = mgr;
            player = fpsPlayer;
        }

        private void EnsurePlayerReference()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<FirstPersonPlayer>();
            }
        }

        private void Awake()
        {
            if (boothManager == null)
            {
                boothManager = FindAnyObjectByType<CarnivalBoothManager>();
            }

            EnsurePlayerReference();

            _uiAudioSource = gameObject.AddComponent<AudioSource>();
            _uiAudioSource.playOnAwake = false;
            _clickSound = CarnivalAudioHelper.CreateButtonClickClip();

            EnsureEventSystem();
            BuildDynamicUI();
        }

        private void OnEnable()
        {
            if (boothManager != null)
            {
                boothManager.OnModeChanged += HandleModeChanged;
                boothManager.OnScoreChanged += HandleScoreChanged;
                boothManager.OnTargetsCountChanged += HandleTargetsCountChanged;
                boothManager.OnBallsCountChanged += HandleBallsCountChanged;
                boothManager.OnRoundEvaluated += HandleRoundEvaluated;
                boothManager.OnRoundReset += HandleRoundReset;
            }
        }

        private void OnDisable()
        {
            if (boothManager != null)
            {
                boothManager.OnModeChanged -= HandleModeChanged;
                boothManager.OnScoreChanged -= HandleScoreChanged;
                boothManager.OnTargetsCountChanged -= HandleTargetsCountChanged;
                boothManager.OnBallsCountChanged -= HandleBallsCountChanged;
                boothManager.OnRoundEvaluated -= HandleRoundEvaluated;
                boothManager.OnRoundReset -= HandleRoundReset;
            }
        }

        private void Start()
        {
            if (boothManager == null)
            {
                boothManager = FindAnyObjectByType<CarnivalBoothManager>();
            }

            EnsurePlayerReference();

            if (boothManager != null && boothManager.ActiveMode != CarnivalGameMode.None)
            {
                ShowInGameHUD(boothManager.ActiveMode);
            }
            else
            {
                ShowSelectionMenu();
            }
        }

        private void Update()
        {
            bool escapePressed = false;
            bool rPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                escapePressed = UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                                UnityEngine.InputSystem.Keyboard.current.mKey.wasPressedThisFrame;
                rPressed = UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
            }
#else
            escapePressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M);
            rPressed = Input.GetKeyDown(KeyCode.R);
#endif

            if (rPressed && boothManager != null && boothManager.ActiveMode != CarnivalGameMode.None)
            {
                OnClickRestart();
                return;
            }

            if (escapePressed && boothManager != null)
            {
                if (_menuModalPanel != null && _menuModalPanel.activeSelf)
                {
                    if (boothManager.ActiveMode != CarnivalGameMode.None)
                    {
                        HideSelectionMenu();
                    }
                }
                else
                {
                    boothManager.OpenModeSelectionMenu();
                }
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                esGO.AddComponent<InputSystemUIInputModule>();
#else
                esGO.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        private void HandleModeChanged(CarnivalGameMode newMode)
        {
            if (newMode == CarnivalGameMode.None)
            {
                ShowSelectionMenu();
            }
            else
            {
                ShowInGameHUD(newMode);
            }
        }

        private void HandleRoundReset()
        {
            ShowInGameHUD(boothManager != null ? boothManager.ActiveMode : CarnivalGameMode.StaticTargets);
        }

        private void HandleScoreChanged(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"SCORE: {score}";
            }
        }

        private void HandleTargetsCountChanged(int knocked, int total)
        {
            if (_targetsText != null)
            {
                _targetsText.text = $"TARGETS: {knocked} / {total}";
            }
        }

        private void HandleBallsCountChanged(int remaining, int max)
        {
            if (_ballsText != null)
            {
                _ballsText.text = $"BALLS: {remaining} / {max}";
            }
        }

        private void HandleRoundEvaluated(bool isWin, int finalScore)
        {
            EnsurePlayerReference();
            if (player != null)
            {
                player.SetInputLocked(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_outcomeModalPanel != null)
            {
                _outcomeModalPanel.SetActive(true);

                if (_outcomeBannerText != null)
                {
                    _outcomeBannerText.text = isWin ? "PERFECT STRIKE! YOU WON!" : "OUT OF BALLS! TRY AGAIN!";
                    _outcomeBannerText.color = isWin ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.35f, 0.35f);
                }

                if (_outcomeDetailText != null && boothManager != null)
                {
                    _outcomeDetailText.text = $"Final Score: {finalScore}  |  Targets Knocked: {boothManager.ToppledTargets}/{boothManager.TotalTargets}";
                }
            }
        }

        private void ShowSelectionMenu()
        {
            EnsurePlayerReference();
            if (player != null)
            {
                player.SetInputLocked(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_outcomeModalPanel != null) _outcomeModalPanel.SetActive(false);
            if (_menuModalPanel != null) _menuModalPanel.SetActive(true);
            if (_hudPanel != null) _hudPanel.SetActive(false);
        }

        private void HideSelectionMenu()
        {
            EnsurePlayerReference();
            if (player != null)
            {
                player.SetInputLocked(false);
                player.SetCursorLocked(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (_menuModalPanel != null) _menuModalPanel.SetActive(false);
            if (_hudPanel != null) _hudPanel.SetActive(true);
        }

        private void ShowInGameHUD(CarnivalGameMode mode)
        {
            EnsurePlayerReference();
            if (player != null)
            {
                player.SetInputLocked(false);
                player.SetCursorLocked(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (_outcomeModalPanel != null) _outcomeModalPanel.SetActive(false);
            if (_menuModalPanel != null) _menuModalPanel.SetActive(false);
            if (_hudPanel != null) _hudPanel.SetActive(true);

            if (_modeTitleText != null)
            {
                _modeTitleText.text = (mode == CarnivalGameMode.StaticTargets)
                    ? "GAME 1: STATIC TARGETS"
                    : "GAME 2: MOVING TARGETS";
            }
        }

        private void PlayClickSound()
        {
            if (_uiAudioSource != null && _clickSound != null)
            {
                _uiAudioSource.PlayOneShot(_clickSound);
            }
        }

        public void OnClickSelectGame1()
        {
            PlayClickSound();
            if (boothManager != null)
            {
                boothManager.SelectGameMode(CarnivalGameMode.StaticTargets);
            }
        }

        public void OnClickSelectGame2()
        {
            PlayClickSound();
            if (boothManager != null)
            {
                boothManager.SelectGameMode(CarnivalGameMode.MovingTargets);
            }
        }

        public void OnClickRestart()
        {
            PlayClickSound();
            if (boothManager != null)
            {
                boothManager.ResetRound();
            }
            ShowInGameHUD(boothManager != null ? boothManager.ActiveMode : CarnivalGameMode.StaticTargets);
        }

        public void OnClickOpenMenu()
        {
            PlayClickSound();
            if (boothManager != null)
            {
                boothManager.OpenModeSelectionMenu();
            }
        }

        private void BuildDynamicUI()
        {
            GameObject canvasGO = new GameObject("CarnivalHUD_Canvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            BuildHUD(canvasGO);
            BuildCrosshair(_hudPanel);
            BuildSelectionModal(canvasGO);
            BuildOutcomeModal(canvasGO);
        }

        private void BuildCrosshair(GameObject parent)
        {
            GameObject crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(parent.transform, false);

            RectTransform rt = crosshair.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(10, 10);

            Image img = crosshair.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.85f);
            img.raycastTarget = false;
        }

        private void BuildHUD(GameObject parent)
        {
            _hudPanel = new GameObject("HUD_Panel");
            _hudPanel.transform.SetParent(parent.transform, false);

            RectTransform panelRT = _hudPanel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            GameObject topBar = CreateUIPanel(_hudPanel, "TopBar", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -90), new Vector2(0, 90), new Color(0.08f, 0.08f, 0.12f, 0.85f));

            _modeTitleText = CreateUIText(topBar, "ModeTitle", "CARNIVAL TARGET KNOCKDOWN", 26, FontStyle.Bold,
                new Color(1f, 0.85f, 0.2f), new Vector2(0.02f, 0.5f), new Vector2(0.02f, 0.5f), new Vector2(250, 0), new Vector2(500, 60), TextAnchor.MiddleLeft);

            _scoreText = CreateUIText(topBar, "ScoreText", "SCORE: 0", 26, FontStyle.Bold,
                Color.white, new Vector2(0.40f, 0.5f), new Vector2(0.40f, 0.5f), Vector2.zero, new Vector2(220, 60), TextAnchor.MiddleCenter);

            _targetsText = CreateUIText(topBar, "TargetsText", "TARGETS: 0 / 6", 24, FontStyle.Bold,
                new Color(1f, 0.35f, 0.35f), new Vector2(0.58f, 0.5f), new Vector2(0.58f, 0.5f), Vector2.zero, new Vector2(220, 60), TextAnchor.MiddleCenter);

            _ballsText = CreateUIText(topBar, "BallsText", "BALLS: 8 / 8", 24, FontStyle.Bold,
                new Color(0.35f, 0.85f, 1f), new Vector2(0.74f, 0.5f), new Vector2(0.74f, 0.5f), Vector2.zero, new Vector2(200, 60), TextAnchor.MiddleCenter);

            CreateUIButton(topBar, "RestartButton", "Restart", 20, new Color(0.85f, 0.35f, 0.15f),
                new Vector2(0.88f, 0.5f), new Vector2(140, 50), OnClickRestart);

            CreateUIButton(topBar, "MenuButton", "Menu", 20, new Color(0.2f, 0.45f, 0.85f),
                new Vector2(0.96f, 0.5f), new Vector2(120, 50), OnClickOpenMenu);

            _hudPanel.SetActive(false);
        }

        private void BuildSelectionModal(GameObject parent)
        {
            _menuModalPanel = new GameObject("SelectionModal_Panel");
            _menuModalPanel.transform.SetParent(parent.transform, false);

            RectTransform bgRT = _menuModalPanel.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            Image bgImg = _menuModalPanel.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.04f, 0.08f, 0.92f);

            GameObject modalBox = CreateUIPanel(_menuModalPanel, "ModalBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(880, 620), new Color(0.12f, 0.12f, 0.18f, 0.98f));

            Outline outline = modalBox.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.8f, 0.2f, 0.9f);
            outline.effectDistance = new Vector2(4, 4);

            CreateUIText(modalBox, "MainTitle", "CARNIVAL TARGET KNOCKDOWN", 36, FontStyle.Bold,
                new Color(1f, 0.85f, 0.2f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(800, 70), TextAnchor.MiddleCenter);

            CreateUIText(modalBox, "SubTitle", "Select a game mode to play:", 22, FontStyle.Normal,
                new Color(0.9f, 0.9f, 0.95f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(750, 45), TextAnchor.MiddleCenter);

            CreateUIButton(modalBox, "Game1Button", "GAME 1: STATIC TARGETS", 24,
                new Color(0.85f, 0.22f, 0.22f), new Vector2(0.5f, 0.62f), new Vector2(700, 70), OnClickSelectGame1);
            CreateUIText(modalBox, "Game1Desc", "Stationary red cylinders arranged on the carnival shelves.", 18, FontStyle.Italic,
                new Color(0.8f, 0.85f, 0.9f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(700, 35), TextAnchor.MiddleCenter);

            CreateUIButton(modalBox, "Game2Button", "GAME 2: MOVING TARGETS", 24,
                new Color(0.18f, 0.65f, 0.35f), new Vector2(0.5f, 0.38f), new Vector2(700, 70), OnClickSelectGame2);
            CreateUIText(modalBox, "Game2Desc", "Red cylinders continuously glide across the booth shelves.", 18, FontStyle.Italic,
                new Color(0.8f, 0.85f, 0.9f), new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(700, 35), TextAnchor.MiddleCenter);

            _menuModalPanel.SetActive(false);
        }

        private void BuildOutcomeModal(GameObject parent)
        {
            _outcomeModalPanel = new GameObject("OutcomeModal_Panel");
            _outcomeModalPanel.transform.SetParent(parent.transform, false);

            RectTransform bgRT = _outcomeModalPanel.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            Image bgImg = _outcomeModalPanel.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.88f);

            GameObject box = CreateUIPanel(_outcomeModalPanel, "OutcomeBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(750, 420), new Color(0.12f, 0.12f, 0.18f, 0.98f));

            Outline outline = box.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.8f, 0.2f, 0.9f);
            outline.effectDistance = new Vector2(3, 3);

            _outcomeBannerText = CreateUIText(box, "BannerText", "PERFECT STRIKE!", 36, FontStyle.Bold,
                new Color(0.2f, 1f, 0.4f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(700, 70), TextAnchor.MiddleCenter);

            _outcomeDetailText = CreateUIText(box, "DetailText", "Score: 600  |  Targets Knocked: 6/6", 24, FontStyle.Normal,
                Color.white, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(700, 50), TextAnchor.MiddleCenter);

            CreateUIButton(box, "PlayAgainBtn", "Play Again", 22, new Color(0.18f, 0.65f, 0.35f),
                new Vector2(0.32f, 0.25f), new Vector2(240, 60), OnClickRestart);

            CreateUIButton(box, "SwitchModeBtn", "Select Game", 22, new Color(0.2f, 0.45f, 0.85f),
                new Vector2(0.68f, 0.25f), new Vector2(240, 60), OnClickOpenMenu);

            _outcomeModalPanel.SetActive(false);
        }

        private GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = panel.AddComponent<Image>();
            img.color = color;

            return panel;
        }

        private Text CreateUIText(GameObject parent, string name, string content, int fontSize, FontStyle style,
            Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor alignment)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform, false);

            RectTransform rt = textGO.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text txt = textGO.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = alignment;
            txt.raycastTarget = false;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return txt;
        }

        private GameObject CreateUIButton(GameObject parent, string name, string label, int fontSize,
            Color normalColor, Vector2 anchor, Vector2 sizeDelta, Action onClick)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent.transform, false);

            RectTransform rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeDelta;

            Image img = btnGO.AddComponent<Image>();
            img.color = normalColor;

            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = normalColor;
            cb.highlightedColor = normalColor * 1.25f;
            cb.pressedColor = normalColor * 0.8f;
            btn.colors = cb;

            btn.onClick.AddListener(() => onClick?.Invoke());

            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);

            RectTransform labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            Text txt = labelGO.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return btnGO;
        }
    }
}
