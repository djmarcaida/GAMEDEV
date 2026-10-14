using System;
using UnityEngine;
using UnityEngine.Events;

namespace CarnivalGame
{
    public enum TargetType
    {
        Can,
        MilkBottle,
        Duck,
        CarnivalPin,
        HeavyCylinder,
        BonusTarget
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ToppleTarget : MonoBehaviour
    {
        [SerializeField] private TargetType targetType = TargetType.Can;
        [SerializeField] private int customPointValue = 0;
        [Range(0f, 0.95f)] [SerializeField] private float toppleDotThreshold = 0.72f;
        [SerializeField] private float heightFallThreshold = 0.18f;
        [SerializeField] private float absoluteFloorYThreshold = -10f;
        [SerializeField] private float debounceDuration = 0.08f;
        [SerializeField] private AudioClip toppledAudioClip;
        [SerializeField] private ParticleSystem toppledVfx;
        [SerializeField] private UnityEvent onToppledUnityEvent;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _initialUpVector;
        private Transform _initialParent;
        private Rigidbody _rigidbody;
        private AudioSource _audioSource;
        private bool _isToppled;
        private float _toppleTimer;
        private float _spawnGraceTimer;

        public TargetType Type => targetType;
        public bool IsToppled => _isToppled;
        public Rigidbody TargetRigidbody => _rigidbody;

        public int PointValue
        {
            get
            {
                if (customPointValue > 0) return customPointValue;

                return targetType switch
                {
                    TargetType.MilkBottle => 150,
                    TargetType.Duck => 250,
                    TargetType.CarnivalPin => 200,
                    TargetType.HeavyCylinder => 300,
                    TargetType.BonusTarget => 500,
                    _ => 100
                };
            }
        }

        public static event Action<ToppleTarget> OnAnyTargetToppled;
        public event Action<ToppleTarget> OnToppled;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _rigidbody.centerOfMass = Vector3.zero;
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 0.2f;
                _audioSource.playOnAwake = false;
            }

            if (toppledAudioClip == null)
            {
                toppledAudioClip = CarnivalAudioHelper.CreateTargetHitClip();
            }

            CacheInitialState();
        }

        private void CacheInitialState()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _initialUpVector = transform.up;
            _initialParent = transform.parent;
        }

        private void OnEnable()
        {
            _isToppled = false;
            _toppleTimer = 0f;
            _spawnGraceTimer = 0.35f;
        }

        private void FixedUpdate()
        {
            if (_isToppled) return;

            if (_spawnGraceTimer > 0f)
            {
                _spawnGraceTimer -= Time.fixedDeltaTime;
                return;
            }

            EvaluateToppleState();
        }

        private void EvaluateToppleState()
        {
            float dotWithWorldUp = Vector3.Dot(transform.up, Vector3.up);
            float dotWithInitialUp = Vector3.Dot(transform.up, _initialUpVector);
            bool isTilted = dotWithWorldUp < toppleDotThreshold || dotWithInitialUp < toppleDotThreshold;

            bool hasDroppedFromShelf = transform.position.y < (_initialPosition.y - heightFallThreshold);
            bool isBelowFloor = transform.position.y < absoluteFloorYThreshold;

            if (isTilted || hasDroppedFromShelf || isBelowFloor)
            {
                _toppleTimer += Time.fixedDeltaTime;

                if (hasDroppedFromShelf || isBelowFloor || _toppleTimer >= debounceDuration)
                {
                    ConfirmToppled();
                }
            }
            else
            {
                _toppleTimer = 0f;
            }
        }

        private void ConfirmToppled()
        {
            if (_isToppled) return;

            _isToppled = true;

            if (_audioSource != null && toppledAudioClip != null)
            {
                _audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                _audioSource.PlayOneShot(toppledAudioClip);
            }

            if (toppledVfx != null)
            {
                toppledVfx.Play();
            }

            onToppledUnityEvent?.Invoke();
            OnToppled?.Invoke(this);
            OnAnyTargetToppled?.Invoke(this);
        }

        public void ResetTarget()
        {
            if (transform.parent != _initialParent)
            {
                transform.SetParent(_initialParent);
            }

            if (_rigidbody != null)
            {
#if UNITY_6000_0_OR_NEWER
                _rigidbody.linearVelocity = Vector3.zero;
#else
                _rigidbody.velocity = Vector3.zero;
#endif
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.Sleep();
            }

            transform.position = _initialPosition;
            transform.rotation = _initialRotation;

            if (_rigidbody != null)
            {
                _rigidbody.position = _initialPosition;
                _rigidbody.rotation = _initialRotation;
            }

            _isToppled = false;
            _toppleTimer = 0f;
            _spawnGraceTimer = 0.35f;
        }

        public void RebaseInitialTransform()
        {
            CacheInitialState();
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? _initialPosition : transform.position;
            Vector3 fallPlane = center - new Vector3(0f, heightFallThreshold, 0f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(fallPlane, new Vector3(0.5f, 0.02f, 0.5f));
            Gizmos.DrawLine(center, fallPlane);
        }
    }
}
