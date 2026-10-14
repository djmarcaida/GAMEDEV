using System.Collections.Generic;
using UnityEngine;

namespace CarnivalGame
{
    public enum TrackMovementMode
    {
        PingPong,
        Loop,
        OneWay
    }

    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatformTrack : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Vector3 fallbackLocalOscillation = new Vector3(3f, 0f, 0f);
        [SerializeField] private TrackMovementMode movementMode = TrackMovementMode.PingPong;
        [SerializeField] private float speed = 2.0f;
        [SerializeField] private float waypointDwellTime = 0.5f;
        [SerializeField] private bool easeWaypoints = true;
        [SerializeField] private bool unparentToppledTargets = true;
        [SerializeField] private List<ToppleTarget> mountedTargets = new List<ToppleTarget>();

        private Rigidbody _rigidbody;
        private readonly List<Vector3> _resolvedWorldWaypoints = new List<Vector3>();
        private int _currentWaypointIndex;
        private int _direction = 1;
        private float _dwellTimer;
        private bool _isDwelling;
        private bool _isMoving = true;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        public bool IsMoving => _isMoving;
        public int WaypointCount => _resolvedWorldWaypoints.Count;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _startPosition = transform.position;
            _startRotation = transform.rotation;

            InitializePath();
            RegisterMountedTargets();
        }

        private void InitializePath()
        {
            _resolvedWorldWaypoints.Clear();

            if (waypoints != null && waypoints.Length > 1)
            {
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] != null)
                    {
                        _resolvedWorldWaypoints.Add(waypoints[i].position);
                    }
                }
            }

            if (_resolvedWorldWaypoints.Count < 2)
            {
                _resolvedWorldWaypoints.Clear();
                _resolvedWorldWaypoints.Add(_startPosition);
                _resolvedWorldWaypoints.Add(_startPosition + transform.TransformDirection(fallbackLocalOscillation));
            }

            _currentWaypointIndex = 1;
            _direction = 1;
        }

        private void RegisterMountedTargets()
        {
            if (mountedTargets.Count == 0)
            {
                GetComponentsInChildren(true, mountedTargets);
            }

            if (unparentToppledTargets)
            {
                for (int i = 0; i < mountedTargets.Count; i++)
                {
                    if (mountedTargets[i] != null)
                    {
                        mountedTargets[i].OnToppled += HandleMountedTargetToppled;
                    }
                }
            }
        }

        private void HandleMountedTargetToppled(ToppleTarget target)
        {
            if (unparentToppledTargets && target != null && target.transform.parent == transform)
            {
                target.transform.SetParent(null, true);
            }
        }

        private void FixedUpdate()
        {
            if (!_isMoving || _resolvedWorldWaypoints.Count < 2) return;

            if (_isDwelling)
            {
                _dwellTimer += Time.fixedDeltaTime;
                if (_dwellTimer >= waypointDwellTime)
                {
                    _isDwelling = false;
                    _dwellTimer = 0f;
                    AdvanceWaypointIndex();
                }
                return;
            }

            MoveTowardsCurrentWaypoint();
        }

        private void MoveTowardsCurrentWaypoint()
        {
            Vector3 targetPoint = _resolvedWorldWaypoints[_currentWaypointIndex];
            Vector3 currentPos = _rigidbody.position;

            float distance = Vector3.Distance(currentPos, targetPoint);
            float currentStepSpeed = speed;

            if (easeWaypoints && distance < 0.8f)
            {
                currentStepSpeed = Mathf.Max(speed * (distance / 0.8f), 0.3f);
            }

            Vector3 newPosition = Vector3.MoveTowards(currentPos, targetPoint, currentStepSpeed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(newPosition);

            if (Vector3.Distance(newPosition, targetPoint) <= 0.005f)
            {
                _rigidbody.MovePosition(targetPoint);

                if (waypointDwellTime > 0f)
                {
                    _isDwelling = true;
                    _dwellTimer = 0f;
                }
                else
                {
                    AdvanceWaypointIndex();
                }
            }
        }

        private void AdvanceWaypointIndex()
        {
            int total = _resolvedWorldWaypoints.Count;

            switch (movementMode)
            {
                case TrackMovementMode.PingPong:
                    if (_currentWaypointIndex + _direction >= total)
                    {
                        _direction = -1;
                    }
                    else if (_currentWaypointIndex + _direction < 0)
                    {
                        _direction = 1;
                    }
                    _currentWaypointIndex += _direction;
                    break;

                case TrackMovementMode.Loop:
                    _currentWaypointIndex = (_currentWaypointIndex + 1) % total;
                    break;

                case TrackMovementMode.OneWay:
                    if (_currentWaypointIndex + 1 < total)
                    {
                        _currentWaypointIndex++;
                    }
                    else
                    {
                        _isMoving = false;
                    }
                    break;
            }
        }

        public void SetMoving(bool active)
        {
            _isMoving = active;
        }

        public void ResetPlatform()
        {
            _rigidbody.position = _startPosition;
            _rigidbody.rotation = _startRotation;
            transform.position = _startPosition;
            transform.rotation = _startRotation;

            _currentWaypointIndex = 1;
            _direction = 1;
            _isDwelling = false;
            _dwellTimer = 0f;
            _isMoving = true;

            for (int i = 0; i < mountedTargets.Count; i++)
            {
                ToppleTarget target = mountedTargets[i];
                if (target != null && target.transform.parent != transform)
                {
                    target.transform.SetParent(transform, true);
                }
            }
        }

        private void OnDestroy()
        {
            if (unparentToppledTargets)
            {
                for (int i = 0; i < mountedTargets.Count; i++)
                {
                    if (mountedTargets[i] != null)
                    {
                        mountedTargets[i].OnToppled -= HandleMountedTargetToppled;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            List<Vector3> points = new List<Vector3>();

            if (waypoints != null && waypoints.Length > 1)
            {
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] != null) points.Add(waypoints[i].position);
                }
            }
            else
            {
                Vector3 origin = Application.isPlaying ? _startPosition : transform.position;
                points.Add(origin);
                points.Add(origin + transform.TransformDirection(fallbackLocalOscillation));
            }

            if (points.Count < 2) return;

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
                Gizmos.DrawWireSphere(points[i], 0.15f);
            }
            Gizmos.DrawWireSphere(points[^1], 0.15f);

            if (movementMode == TrackMovementMode.Loop && points.Count > 2)
            {
                Gizmos.color = new Color(0.2f, 1f, 0.5f, 0.6f);
                Gizmos.DrawLine(points[^1], points[0]);
            }
        }
    }
}
