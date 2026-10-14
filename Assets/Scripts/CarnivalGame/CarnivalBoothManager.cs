using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace CarnivalGame
{
    public enum RoundState
    {
        Ready,
        Playing,
        Settling,
        Completed
    }

    public enum WinCondition
    {
        KnockAllTargets,
        ReachScoreThreshold,
        KnockPercentage
    }

    public class CarnivalBoothManager : MonoBehaviour
    {
        [SerializeField] private CarnivalGameMode activeMode = CarnivalGameMode.StaticTargets;
        [SerializeField] private GameObject staticTargetsGroup;
        [SerializeField] private GameObject movingTargetsGroup;
        [SerializeField] private List<ToppleTarget> staticTargets = new List<ToppleTarget>();
        [SerializeField] private List<MovingTargetConveyor> movingTargets = new List<MovingTargetConveyor>();
        [SerializeField] private BallThrower ballThrower;
        [SerializeField] private List<ToppleTarget> targets = new List<ToppleTarget>();
        [SerializeField] private List<MovingPlatformTrack> movingTracks = new List<MovingPlatformTrack>();
        [SerializeField] private WinCondition winCondition = WinCondition.KnockAllTargets;
        [SerializeField] private int targetScoreThreshold = 800;
        [Range(0.1f, 1f)] [SerializeField] private float requiredKnockPercentage = 1.0f;
        [SerializeField] private float physicsSettleDelay = 3.0f;

        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text ballsLeftText;
        [SerializeField] private TMP_Text targetsLeftText;
        [SerializeField] private TMP_Text resultBannerText;
        [SerializeField] private GameObject restartButtonPrompt;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip roundStartClip;
        [SerializeField] private AudioClip roundWinClip;
        [SerializeField] private AudioClip roundLoseClip;

        [SerializeField] private UnityEvent<int> onScoreUpdatedEvent;
        [SerializeField] private UnityEvent<bool> onRoundCompletedEvent;

        private RoundState _currentState = RoundState.Ready;
        private int _currentScore;
        private int _toppledCount;
        private Coroutine _settleCoroutine;

        public CarnivalGameMode ActiveMode => activeMode;
        public RoundState CurrentState => _currentState;
        public int CurrentScore => _currentScore;
        public int TotalTargets => targets.Count;
        public int ToppledTargets => _toppledCount;
        public int RemainingTargets => Mathf.Max(0, TotalTargets - _toppledCount);
        public BallThrower Thrower => ballThrower;

        public event Action<CarnivalGameMode> OnModeChanged;
        public event Action<RoundState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int, int> OnTargetsCountChanged;
        public event Action<int, int> OnBallsCountChanged;
        public event Action<bool, int> OnRoundEvaluated;
        public event Action OnRoundReset;

        public void ConfigureTargets(GameObject staticGroup, GameObject movingGroup, List<ToppleTarget> sTargets, List<MovingTargetConveyor> mTargets, BallThrower thrower)
        {
            staticTargetsGroup = staticGroup;
            movingTargetsGroup = movingGroup;
            staticTargets = sTargets;
            movingTargets = mTargets;
            ballThrower = thrower;
        }

        private void Awake()
        {
            if (ballThrower == null)
            {
                ballThrower = FindAnyObjectByType<BallThrower>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 0.1f;
                    audioSource.playOnAwake = false;
                }
            }

            if (roundStartClip == null) roundStartClip = CarnivalAudioHelper.CreateRoundStartClip();
            if (roundWinClip == null) roundWinClip = CarnivalAudioHelper.CreateWinClip();
            if (roundLoseClip == null) roundLoseClip = CarnivalAudioHelper.CreateLoseClip();

            DiscoverSceneObjects();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            if (activeMode != CarnivalGameMode.None)
            {
                SelectGameMode(activeMode);
            }
            else
            {
                if (ballThrower != null)
                {
                    ballThrower.SetCanThrow(false);
                }
                UpdateUI();
            }
        }

        private void DiscoverSceneObjects()
        {
            if (staticTargets.Count == 0 && staticTargetsGroup != null)
            {
                staticTargets.AddRange(staticTargetsGroup.GetComponentsInChildren<ToppleTarget>(true));
            }

            if (movingTargets.Count == 0 && movingTargetsGroup != null)
            {
                movingTargets.AddRange(movingTargetsGroup.GetComponentsInChildren<MovingTargetConveyor>(true));
            }

            if (movingTracks.Count == 0)
            {
                movingTracks.AddRange(FindObjectsByType<MovingPlatformTrack>());
            }
        }

        public void SelectGameMode(CarnivalGameMode newMode)
        {
            UnsubscribeFromEvents();

            activeMode = newMode;

            if (activeMode == CarnivalGameMode.StaticTargets)
            {
                if (staticTargetsGroup != null) staticTargetsGroup.SetActive(true);
                if (movingTargetsGroup != null) movingTargetsGroup.SetActive(false);

                targets.Clear();
                targets.AddRange(staticTargets);
            }
            else if (activeMode == CarnivalGameMode.MovingTargets)
            {
                if (staticTargetsGroup != null) staticTargetsGroup.SetActive(false);
                if (movingTargetsGroup != null) movingTargetsGroup.SetActive(true);

                targets.Clear();
                for (int i = 0; i < movingTargets.Count; i++)
                {
                    if (movingTargets[i] != null && movingTargets[i].ToppleTargetComponent != null)
                    {
                        targets.Add(movingTargets[i].ToppleTargetComponent);
                    }
                }
            }
            else
            {
                if (staticTargetsGroup != null) staticTargetsGroup.SetActive(false);
                if (movingTargetsGroup != null) movingTargetsGroup.SetActive(false);
                targets.Clear();
            }

            SubscribeToEvents();

            OnModeChanged?.Invoke(activeMode);

            if (activeMode != CarnivalGameMode.None)
            {
                ResetRound();
            }
            else
            {
                if (ballThrower != null) ballThrower.SetCanThrow(false);
            }
        }

        public void OpenModeSelectionMenu()
        {
            if (_settleCoroutine != null)
            {
                StopCoroutine(_settleCoroutine);
                _settleCoroutine = null;
            }

            if (ballThrower != null)
            {
                ballThrower.SetCanThrow(false);
                ballThrower.ClearAllActiveBalls();
            }

            activeMode = CarnivalGameMode.None;
            SetState(RoundState.Ready);
            OnModeChanged?.Invoke(CarnivalGameMode.None);
        }

        private void SubscribeToEvents()
        {
            if (ballThrower != null)
            {
                ballThrower.OnBallsRemainingChanged += HandleBallsRemainingChanged;
                ballThrower.OnOutOfBalls += HandleOutOfBalls;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].OnToppled += HandleTargetToppled;
                }
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (ballThrower != null)
            {
                ballThrower.OnBallsRemainingChanged -= HandleBallsRemainingChanged;
                ballThrower.OnOutOfBalls -= HandleOutOfBalls;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].OnToppled -= HandleTargetToppled;
                }
            }
        }

        private void HandleTargetToppled(ToppleTarget target)
        {
            if (_currentState == RoundState.Completed) return;

            _toppledCount++;
            _currentScore += target.PointValue;

            UpdateUI();

            OnScoreChanged?.Invoke(_currentScore);
            OnTargetsCountChanged?.Invoke(_toppledCount, TotalTargets);
            onScoreUpdatedEvent?.Invoke(_currentScore);

            if (_toppledCount >= TotalTargets && TotalTargets > 0)
            {
                if (_settleCoroutine != null)
                {
                    StopCoroutine(_settleCoroutine);
                    _settleCoroutine = null;
                }
                EvaluateRoundOutcome(isInstantWin: true);
            }
        }

        private void HandleBallsRemainingChanged(int remaining, int max)
        {
            if (_currentState == RoundState.Ready && remaining < max)
            {
                SetState(RoundState.Playing);
            }

            UpdateUI();
            OnBallsCountChanged?.Invoke(remaining, max);
        }

        private void HandleOutOfBalls()
        {
            if (_currentState != RoundState.Playing) return;

            SetState(RoundState.Settling);

            if (_settleCoroutine != null)
            {
                StopCoroutine(_settleCoroutine);
            }
            _settleCoroutine = StartCoroutine(WaitForPhysicsSettleRoutine());
        }

        private IEnumerator WaitForPhysicsSettleRoutine()
        {
            if (resultBannerText != null)
            {
                resultBannerText.gameObject.SetActive(true);
                resultBannerText.text = "SETTLING...";
            }

            float timer = 0f;
            while (timer < physicsSettleDelay)
            {
                if (_toppledCount >= TotalTargets && TotalTargets > 0)
                {
                    EvaluateRoundOutcome(isInstantWin: true);
                    yield break;
                }

                if (ballThrower != null && ballThrower.ActiveBallsInPlay == 0 && timer > 1.0f)
                {
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            _settleCoroutine = null;
            EvaluateRoundOutcome(isInstantWin: false);
        }

        private void EvaluateRoundOutcome(bool isInstantWin)
        {
            SetState(RoundState.Completed);

            bool isWin = false;

            if (isInstantWin)
            {
                isWin = true;
            }
            else
            {
                switch (winCondition)
                {
                    case WinCondition.KnockAllTargets:
                        isWin = _toppledCount >= TotalTargets && TotalTargets > 0;
                        break;

                    case WinCondition.ReachScoreThreshold:
                        isWin = _currentScore >= targetScoreThreshold;
                        break;

                    case WinCondition.KnockPercentage:
                        float knockRatio = TotalTargets > 0 ? (float)_toppledCount / TotalTargets : 0f;
                        isWin = knockRatio >= requiredKnockPercentage;
                        break;
                }
            }

            if (audioSource != null)
            {
                AudioClip clipToPlay = isWin ? roundWinClip : roundLoseClip;
                if (clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay);
                }
            }

            if (resultBannerText != null)
            {
                resultBannerText.gameObject.SetActive(true);
                if (isWin)
                {
                    resultBannerText.text = (_toppledCount >= TotalTargets) ? "PERFECT STRIKE!\nYOU WIN!" : "WINNER!\nTARGET REACHED!";
                    resultBannerText.color = new Color(0.2f, 1f, 0.4f);
                }
                else
                {
                    resultBannerText.text = "OUT OF BALLS!\nTRY AGAIN!";
                    resultBannerText.color = new Color(1f, 0.3f, 0.3f);
                }
            }

            if (restartButtonPrompt != null)
            {
                restartButtonPrompt.SetActive(true);
            }

            OnRoundEvaluated?.Invoke(isWin, _currentScore);
            onRoundCompletedEvent?.Invoke(isWin);

            if (ballThrower != null)
            {
                ballThrower.SetCanThrow(false);
            }
        }

        public void ResetRound()
        {
            if (_settleCoroutine != null)
            {
                StopCoroutine(_settleCoroutine);
                _settleCoroutine = null;
            }

            _currentScore = 0;
            _toppledCount = 0;

            if (ballThrower == null)
            {
                ballThrower = FindAnyObjectByType<BallThrower>();
            }

            if (ballThrower != null)
            {
                ballThrower.ClearAllActiveBalls();
                ballThrower.ResetAmmo();
                ballThrower.SetCanThrow(true);
            }

            if (activeMode == CarnivalGameMode.StaticTargets)
            {
                for (int i = 0; i < staticTargets.Count; i++)
                {
                    if (staticTargets[i] != null)
                    {
                        staticTargets[i].ResetTarget();
                    }
                }
            }
            else if (activeMode == CarnivalGameMode.MovingTargets)
            {
                for (int i = 0; i < movingTargets.Count; i++)
                {
                    if (movingTargets[i] != null)
                    {
                        movingTargets[i].ResetTargetState();
                    }
                }
            }
            else
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] != null)
                    {
                        targets[i].ResetTarget();
                    }
                }
            }

            for (int i = 0; i < movingTracks.Count; i++)
            {
                if (movingTracks[i] != null)
                {
                    movingTracks[i].ResetPlatform();
                }
            }

            if (resultBannerText != null)
            {
                resultBannerText.gameObject.SetActive(false);
            }

            if (restartButtonPrompt != null)
            {
                restartButtonPrompt.SetActive(false);
            }

            if (audioSource != null && roundStartClip != null)
            {
                audioSource.PlayOneShot(roundStartClip);
            }

            SetState(RoundState.Ready);
            UpdateUI();

            OnScoreChanged?.Invoke(_currentScore);
            OnTargetsCountChanged?.Invoke(_toppledCount, TotalTargets);
            OnRoundReset?.Invoke();
        }

        private void SetState(RoundState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }

        private void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {_currentScore}";
            }

            if (ballsLeftText != null && ballThrower != null)
            {
                ballsLeftText.text = $"BALLS: {ballThrower.BallsRemaining}/{ballThrower.MaxBalls}";
            }

            if (targetsLeftText != null)
            {
                targetsLeftText.text = $"TARGETS: {_toppledCount}/{TotalTargets}";
            }
        }
    }
}
