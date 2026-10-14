using System;
using UnityEngine;

namespace CarnivalGame
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class CarnivalBall : MonoBehaviour
    {
        [SerializeField] private float maxLifetime = 12f;
        [SerializeField] private float stationaryVelocityThreshold = 0.12f;
        [SerializeField] private float stationarySettleDuration = 2.5f;
        [SerializeField] private float killHeightY = -5f;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip targetHitClip;
        [SerializeField] private AudioClip shelfHitClip;
        [Range(0f, 1f)] [SerializeField] private float minHitVolume = 0.2f;
        [Range(0f, 1f)] [SerializeField] private float maxHitVolume = 1f;

        private Rigidbody _rigidbody;
        private float _aliveTimer;
        private float _stationaryTimer;
        private bool _isSettled;

        public Rigidbody BallRigidbody => _rigidbody;
        public bool IsSettled => _isSettled;

        public event Action<CarnivalBall> OnBallCleanedUp;

        private Vector3 Velocity
        {
            get
            {
#if UNITY_6000_0_OR_NEWER
                return _rigidbody.linearVelocity;
#else
                return _rigidbody.velocity;
#endif
            }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
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

            if (targetHitClip == null)
            {
                targetHitClip = CarnivalAudioHelper.CreateTargetHitClip();
            }

            if (shelfHitClip == null)
            {
                shelfHitClip = CarnivalAudioHelper.CreateShelfHitClip();
            }
        }

        private void OnEnable()
        {
            _aliveTimer = 0f;
            _stationaryTimer = 0f;
            _isSettled = false;
        }

        private void Update()
        {
            _aliveTimer += Time.deltaTime;

            if (_aliveTimer >= maxLifetime || transform.position.y < killHeightY)
            {
                RecycleBall();
                return;
            }

            UpdateStationaryStatus();
        }

        private void UpdateStationaryStatus()
        {
            float thresholdSqr = stationaryVelocityThreshold * stationaryVelocityThreshold;

            if (Velocity.sqrMagnitude <= thresholdSqr)
            {
                _stationaryTimer += Time.deltaTime;
                if (_stationaryTimer >= stationarySettleDuration && !_isSettled)
                {
                    _isSettled = true;
                    RecycleBall();
                }
            }
            else
            {
                _stationaryTimer = 0f;
                _isSettled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < 0.5f || audioSource == null) return;

            float volume = Mathf.Clamp01(impactSpeed / 10f) * (maxHitVolume - minHitVolume) + minHitVolume;

            if (collision.gameObject.TryGetComponent<ToppleTarget>(out _))
            {
                if (targetHitClip != null)
                {
                    audioSource.pitch = UnityEngine.Random.Range(0.92f, 1.08f);
                    audioSource.PlayOneShot(targetHitClip, volume);
                }
            }
            else
            {
                if (shelfHitClip != null)
                {
                    audioSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
                    audioSource.PlayOneShot(shelfHitClip, volume * 0.7f);
                }
            }
        }

        public void RecycleBall()
        {
            OnBallCleanedUp?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
