using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GamedevVFX
{
    public class VFXShowcaseController : MonoBehaviour
    {
        [Header("Particle Prefab Instances")]
        public ParticleSystem magicalGirlHenshin;
        public ParticleSystem elementalBendingStrike;
        public ParticleSystem finalBossAuramaxxing;

        [Header("Camera Presets")]
        public Transform mainCamera;
        public Transform[] cameraViews;
        public string[] viewLabels =
        {
            "Overview (All 3)",
            "VFX 1: Magical Girl Henshin (Sky Starfall)",
            "VFX 2: Azula Blue Firebending",
            "VFX 3: Final Boss Auramaxxing"
        };

        [Header("Camera Smoothing")]
        public float moveLerpSpeed = 6f;

        private int _activeViewIndex;
        private GUIStyle _boxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;

        private void Start()
        {
            if (mainCamera == null && Camera.main != null)
                mainCamera = Camera.main.transform;

            // Start the continuous Boss Auramaxxing VFX active immediately
            if (finalBossAuramaxxing != null && !finalBossAuramaxxing.isPlaying)
                finalBossAuramaxxing.Play(true);

            // Play one-shots once at start so the showcase immediately comes alive
            PlayVFX1();
            PlayVFX2();
        }

        private void Update()
        {
            if (WasKeyPressed(KeyCode.Alpha1))
                PlayVFX1();

            if (WasKeyPressed(KeyCode.Alpha2))
                PlayVFX2();

            if (WasKeyPressed(KeyCode.Alpha3))
                ToggleVFX3();

            if (WasKeyPressed(KeyCode.Space))
                PlayAll();

            if (WasKeyPressed(KeyCode.Tab))
                CycleCameraView();

            UpdateCameraTransform();
        }

        public void PlayVFX1()
        {
            if (magicalGirlHenshin == null) return;
            magicalGirlHenshin.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            magicalGirlHenshin.Play(true);
        }

        public void PlayVFX2()
        {
            if (elementalBendingStrike == null) return;
            elementalBendingStrike.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            elementalBendingStrike.Play(true);
        }

        public void ToggleVFX3()
        {
            if (finalBossAuramaxxing == null) return;
            if (finalBossAuramaxxing.isPlaying)
                finalBossAuramaxxing.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            else
                finalBossAuramaxxing.Play(true);
        }

        public void PlayAll()
        {
            PlayVFX1();
            PlayVFX2();
            if (finalBossAuramaxxing != null && !finalBossAuramaxxing.isPlaying)
                finalBossAuramaxxing.Play(true);
        }

        public void CycleCameraView()
        {
            if (cameraViews == null || cameraViews.Length == 0) return;
            _activeViewIndex = (_activeViewIndex + 1) % cameraViews.Length;
        }

        private void UpdateCameraTransform()
        {
            if (mainCamera == null || cameraViews == null || cameraViews.Length == 0) return;
            Transform target = cameraViews[_activeViewIndex];
            if (target == null) return;

            float t = 1f - Mathf.Exp(-moveLerpSpeed * Time.deltaTime);
            mainCamera.position = Vector3.Lerp(mainCamera.position, target.position, t);
            mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, target.rotation, t);
        }

        private static bool WasKeyPressed(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return false;
            return code switch
            {
                KeyCode.Alpha1 => kb.digit1Key.wasPressedThisFrame,
                KeyCode.Alpha2 => kb.digit2Key.wasPressedThisFrame,
                KeyCode.Alpha3 => kb.digit3Key.wasPressedThisFrame,
                KeyCode.Space => kb.spaceKey.wasPressedThisFrame,
                KeyCode.Tab => kb.tabKey.wasPressedThisFrame,
                _ => false
            };
#else
            return Input.GetKeyDown(code);
#endif
        }

        private void EnsureStyles()
        {
            if (_boxStyle != null) return;

            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.06f, 0.07f, 0.11f, 0.82f));
            bgTex.Apply();

            var btnTex = new Texture2D(1, 1);
            btnTex.SetPixel(0, 0, new Color(0.16f, 0.18f, 0.26f, 0.92f));
            btnTex.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex },
                padding = new RectOffset(14, 14, 10, 10)
            };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.96f, 0.97f, 1f) }
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.78f, 0.82f, 0.90f) }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { background = btnTex, textColor = Color.white }
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            GUILayout.BeginArea(new Rect(18, 18, 340, 215), _boxStyle);
            GUILayout.Label("URP Particle VFX Showcase", _titleStyle);
            string camName = (viewLabels != null && _activeViewIndex < viewLabels.Length)
                ? viewLabels[_activeViewIndex]
                : "Default";
            GUILayout.Label($"Camera [Tab]: {camName}", _labelStyle);
            GUILayout.Space(6);

            if (GUILayout.Button("[1] Play VFX 1 — Magical Girl Starfall", _buttonStyle, GUILayout.Height(26)))
                PlayVFX1();
            if (GUILayout.Button("[2] Play VFX 2 — Azula Blue Flames", _buttonStyle, GUILayout.Height(26)))
                PlayVFX2();
            bool bossOn = finalBossAuramaxxing != null && finalBossAuramaxxing.isPlaying;
            if (GUILayout.Button($"[3] Toggle VFX 3 — Boss Auramaxxing ({(bossOn ? "ON" : "OFF")})", _buttonStyle, GUILayout.Height(26)))
                ToggleVFX3();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("[Space] Play All", _buttonStyle, GUILayout.Height(26)))
                PlayAll();
            if (GUILayout.Button("[Tab] Next Cam", _buttonStyle, GUILayout.Height(26)))
                CycleCameraView();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
