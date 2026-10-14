using System.Collections.Generic;
using UnityEngine;

namespace CarnivalGame
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ToppleTarget))]
    public class MovingTargetConveyor : MonoBehaviour
    {
        private static readonly List<MovingTargetConveyor> AllConveyors = new List<MovingTargetConveyor>();

        [SerializeField] private float speed = 1.8f;
        [SerializeField] private float leftBoundX = -2.0f;
        [SerializeField] private float rightBoundX = 2.0f;
        [SerializeField] private float impactForceMultiplier = 1.0f;

        private Rigidbody _rigidbody;
        private ToppleTarget _toppleTarget;
        private Collider _collider;
        private Vector3 _startLocalPos;
        private Quaternion _startLocalRot;
        private Transform _originalParent;
        private bool _isMoving = true;
        private bool _isToppled;

        public bool IsMoving => _isMoving;
        public float Speed { get => speed; set => speed = value; }
        public float LeftBoundX { get => leftBoundX; set => leftBoundX = value; }
        public float RightBoundX { get => rightBoundX; set => rightBoundX = value; }
        public ToppleTarget ToppleTargetComponent => _toppleTarget;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _toppleTarget = GetComponent<ToppleTarget>();
            _collider = GetComponent<Collider>();

            _startLocalPos = transform.localPosition;
            _startLocalRot = transform.localRotation;
            _originalParent = transform.parent;
        }

        private void OnEnable()
        {
            if (!AllConveyors.Contains(this))
            {
                AllConveyors.Add(this);
            }

            if (_toppleTarget != null)
            {
                _toppleTarget.OnToppled += HandleToppled;
            }
        }

        private void OnDisable()
        {
            AllConveyors.Remove(this);

            if (_toppleTarget != null)
            {
                _toppleTarget.OnToppled -= HandleToppled;
            }
        }

        private void Start()
        {
            PrepareOnTrack();
        }

        public void PrepareOnTrack()
        {
            _isMoving = true;
            _isToppled = false;

            if (_rigidbody != null)
            {
                if (!_rigidbody.isKinematic)
                {
#if UNITY_6000_0_OR_NEWER
                    _rigidbody.linearVelocity = Vector3.zero;
#else
                    _rigidbody.velocity = Vector3.zero;
#endif
                    _rigidbody.angularVelocity = Vector3.zero;
                }
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }

            if (_collider != null)
            {
                for (int i = 0; i < AllConveyors.Count; i++)
                {
                    if (AllConveyors[i] != this && AllConveyors[i]._collider != null)
                    {
                        Physics.IgnoreCollision(_collider, AllConveyors[i]._collider, false);
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_isMoving || _isToppled) return;

            Vector3 currentPos = _rigidbody.position;
            currentPos.x += speed * Time.fixedDeltaTime;

            if (currentPos.x > rightBoundX)
            {
                currentPos.x = leftBoundX;
                _rigidbody.position = currentPos;
                transform.position = currentPos;
                return;
            }

            _rigidbody.MovePosition(currentPos);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isToppled) return;

            bool isBall = collision.gameObject.TryGetComponent<CarnivalBall>(out _) ||
                          collision.transform.root.GetComponentInChildren<CarnivalBall>() != null;

            if (isBall)
            {
                DetachFromConveyor(collision);
            }
        }

        private void HandleToppled(ToppleTarget target)
        {
            if (!_isToppled)
            {
                DetachFromConveyor(null);
            }
        }

        public void DetachFromConveyor(Collision collision)
        {
            if (_isToppled) return;
            _isToppled = true;
            _isMoving = false;

            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = true;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

#if UNITY_6000_0_OR_NEWER
                _rigidbody.linearDamping = 0.3f;
                _rigidbody.angularDamping = 0.5f;
#else
                _rigidbody.drag = 0.3f;
                _rigidbody.angularDrag = 0.5f;
#endif

                if (_collider != null)
                {
                    for (int i = 0; i < AllConveyors.Count; i++)
                    {
                        if (AllConveyors[i] != this && AllConveyors[i]._collider != null)
                        {
                            Physics.IgnoreCollision(_collider, AllConveyors[i]._collider, true);
                        }
                    }
                }

                Vector3 pushDirection = Vector3.forward;
                float forceMag = 1.8f;

                if (collision != null && collision.contactCount > 0)
                {
                    ContactPoint contact = collision.GetContact(0);
                    pushDirection = -contact.normal;
                    pushDirection.z = Mathf.Abs(pushDirection.z);
                    forceMag = Mathf.Clamp(collision.relativeVelocity.magnitude * 0.16f * impactForceMultiplier, 1.0f, 2.8f);
                }

                Vector3 appliedImpulse = (pushDirection.normalized * forceMag) + (Vector3.forward * 1.6f) + (Vector3.up * 0.35f);
                _rigidbody.AddForce(appliedImpulse, ForceMode.Impulse);
                _rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * 2.2f, ForceMode.Impulse);
            }
        }

        public void ResetTargetState(Vector3? spawnPositionOverride = null)
        {
            if (transform.parent != _originalParent)
            {
                transform.SetParent(_originalParent);
            }

            if (spawnPositionOverride.HasValue)
            {
                transform.position = spawnPositionOverride.Value;
            }
            else
            {
                transform.localPosition = _startLocalPos;
            }

            transform.localRotation = _startLocalRot;

            if (_toppleTarget != null)
            {
                _toppleTarget.ResetTarget();
            }

            if (_rigidbody != null)
            {
                _rigidbody.position = transform.position;
                _rigidbody.rotation = transform.rotation;
            }

            PrepareOnTrack();
        }
    }
}
