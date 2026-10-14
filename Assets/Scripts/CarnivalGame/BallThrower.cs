using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CarnivalGame
{
    public class BallThrower : MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private Transform throwOrigin;
        [SerializeField] private GameObject heldBallVisual;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float throwSpeed = 22f;
        [SerializeField] private float upwardLoft = 0.8f;
        [SerializeField] private float maxAimDistance = 40f;
        [SerializeField] private int maxBalls = 8;
        [SerializeField] private float throwCooldown = 0.5f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip throwAudioClip;

        private int _ballsRemaining;
        private float _cooldownTimer;
        private bool _canThrow = true;
        private FirstPersonPlayer _player;
        private readonly List<CarnivalBall> _activeBalls = new List<CarnivalBall>();

        public int BallsRemaining => _ballsRemaining;
        public int MaxBalls => maxBalls;
        public int ActiveBallsInPlay => _activeBalls.Count;
        public bool CanThrow => _canThrow && _ballsRemaining > 0 && _cooldownTimer <= 0f;

        public event Action<int, int> OnBallsRemainingChanged;
        public event Action<GameObject> OnBallFired;
        public event Action OnOutOfBalls;

        public void Initialize(GameObject prefab, Transform origin, GameObject heldVisual, Camera cam, int initialBalls = 8)
        {
            ballPrefab = prefab;
            throwOrigin = origin;
            heldBallVisual = heldVisual;
            playerCamera = cam;
            maxBalls = initialBalls;
            _ballsRemaining = initialBalls;

            UpdateHeldBallVisual();
        }

        private void Awake()
        {
            _player = GetComponent<FirstPersonPlayer>();
            if (_player == null)
            {
                _player = GetComponentInParent<FirstPersonPlayer>();
            }

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 0.2f;
                    audioSource.playOnAwake = false;
                }
            }

            if (throwAudioClip == null)
            {
                throwAudioClip = CarnivalAudioHelper.CreateThrowClip();
            }

            if (throwOrigin == null)
            {
                throwOrigin = transform;
            }

            _ballsRemaining = maxBalls;
        }

        private void Start()
        {
            UpdateHeldBallVisual();
            OnBallsRemainingChanged?.Invoke(_ballsRemaining, maxBalls);
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                {
                    UpdateHeldBallVisual();
                }
            }

            if (!CanThrow) return;

            if (IsFireRequested())
            {
                ExecuteThrow();
            }
        }

        private bool IsFireRequested()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            if (_player == null)
            {
                _player = GetComponent<FirstPersonPlayer>() ?? GetComponentInParent<FirstPersonPlayer>() ?? FindAnyObjectByType<FirstPersonPlayer>();
            }

            if (_player != null && _player.IsInputLocked)
            {
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) return true;
            if (Gamepad.current != null && (Gamepad.current.rightTrigger.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame)) return true;
            return false;
#else
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#endif
        }

        private void ExecuteThrow()
        {
            if (ballPrefab == null || _ballsRemaining <= 0) return;

            Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : transform.position;
            Vector3 aimOrigin = playerCamera != null ? playerCamera.transform.position : spawnPos;
            Vector3 aimForward = playerCamera != null ? playerCamera.transform.forward : transform.forward;

            Vector3 targetPoint = Physics.Raycast(aimOrigin, aimForward, out RaycastHit hit, maxAimDistance)
                ? hit.point
                : aimOrigin + (aimForward * maxAimDistance);

            Vector3 launchDirection = (targetPoint - spawnPos).normalized;
            Vector3 launchVelocity = (launchDirection * throwSpeed) + (Vector3.up * upwardLoft);

            GameObject ballInstance = Instantiate(ballPrefab, spawnPos, Quaternion.LookRotation(launchDirection));
            ballInstance.SetActive(true);

            if (ballInstance.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = launchVelocity;
#else
                rb.velocity = launchVelocity;
#endif
                rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 4f;
            }

            if (ballInstance.TryGetComponent<CarnivalBall>(out var carnivalBall))
            {
                _activeBalls.Add(carnivalBall);
                carnivalBall.OnBallCleanedUp += HandleBallCleanedUp;
            }

            if (audioSource != null && throwAudioClip != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(throwAudioClip);
            }

            _ballsRemaining--;
            _cooldownTimer = throwCooldown;
            UpdateHeldBallVisual();

            OnBallsRemainingChanged?.Invoke(_ballsRemaining, maxBalls);
            OnBallFired?.Invoke(ballInstance);

            if (_ballsRemaining <= 0)
            {
                OnOutOfBalls?.Invoke();
            }
        }

        private void HandleBallCleanedUp(CarnivalBall ball)
        {
            if (ball != null)
            {
                ball.OnBallCleanedUp -= HandleBallCleanedUp;
                _activeBalls.Remove(ball);
            }
        }

        private void UpdateHeldBallVisual()
        {
            if (heldBallVisual != null)
            {
                heldBallVisual.SetActive(_canThrow && _ballsRemaining > 0 && _cooldownTimer <= 0f);
            }
        }

        public void ResetAmmo(int? overrideCount = null)
        {
            _ballsRemaining = overrideCount ?? maxBalls;
            _cooldownTimer = 0f;
            UpdateHeldBallVisual();

            OnBallsRemainingChanged?.Invoke(_ballsRemaining, maxBalls);
        }

        public void ClearAllActiveBalls()
        {
            for (int i = _activeBalls.Count - 1; i >= 0; i--)
            {
                if (_activeBalls[i] != null)
                {
                    Destroy(_activeBalls[i].gameObject);
                }
            }
            _activeBalls.Clear();
        }

        public void SetCanThrow(bool enabled)
        {
            _canThrow = enabled;
            UpdateHeldBallVisual();
        }
    }
}
