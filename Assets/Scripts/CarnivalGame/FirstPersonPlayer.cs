using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CarnivalGame
{
    public class FirstPersonPlayer : MonoBehaviour
    {
        [SerializeField] private Transform headTransform;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float lookSensitivity = 2.2f;
        [SerializeField] private float keyLookSpeed = 75f;
        [SerializeField] private float minPitch = -65f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float minYaw = -80f;
        [SerializeField] private float maxYaw = 80f;

        private float _yaw;
        private float _pitch;
        private bool _isInputLocked;

        public Camera TargetCamera => targetCamera;
        public Transform Head => headTransform != null ? headTransform : (targetCamera != null ? targetCamera.transform : transform);
        public bool IsInputLocked => _isInputLocked;

        public void Initialize(Transform head, Camera cam = null)
        {
            headTransform = head;
            targetCamera = cam != null ? cam : (head != null ? head.GetComponentInChildren<Camera>() : Camera.main);
            SyncInitialAngles();
        }

        private void Awake()
        {
            if (headTransform == null)
            {
                headTransform = transform.Find("Head");
            }

            if (targetCamera == null)
            {
                targetCamera = headTransform != null ? headTransform.GetComponentInChildren<Camera>() : GetComponentInChildren<Camera>();
                if (targetCamera == null)
                {
                    targetCamera = Camera.main;
                }
            }

            if (targetCamera != null)
            {
                targetCamera.nearClipPlane = 0.01f;
            }

            SyncInitialAngles();
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && !_isInputLocked)
            {
                SetCursorLocked(true);
            }
        }

        private void SyncInitialAngles()
        {
            Transform aimTransform = Head;
            if (aimTransform == null) return;

            Vector3 euler = aimTransform.localEulerAngles;
            _yaw = euler.y > 180f ? euler.y - 360f : euler.y;
            _pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        }

        private void Update()
        {
            if (IsClickToLockRequested() && Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorLocked(true);
            }

            if (_isInputLocked) return;

            Vector2 lookDelta = ReadLookDelta();
            if (lookDelta.sqrMagnitude < 0.0001f) return;

            _yaw = Mathf.Clamp(_yaw + lookDelta.x, minYaw, maxYaw);
            _pitch = Mathf.Clamp(_pitch - lookDelta.y, minPitch, maxPitch);

            Transform aimTransform = Head;
            if (aimTransform != null)
            {
                aimTransform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            if (headTransform != null && targetCamera != null && targetCamera.transform != headTransform)
            {
                targetCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private bool IsClickToLockRequested()
        {
            if (_isInputLocked) return false;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            return mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame);
#else
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
#endif
        }

        private Vector2 ReadLookDelta()
        {
            float deltaX = 0f;
            float deltaY = 0f;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 mDelta = mouse.delta.ReadValue();
                deltaX = mDelta.x * (lookSensitivity * 0.08f);
                deltaY = mDelta.y * (lookSensitivity * 0.08f);
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                float keyX = 0f;
                float keyY = 0f;
                if (keyboard.rightArrowKey.isPressed) keyX += 1f;
                if (keyboard.leftArrowKey.isPressed) keyX -= 1f;
                if (keyboard.upArrowKey.isPressed) keyY += 1f;
                if (keyboard.downArrowKey.isPressed) keyY -= 1f;

                deltaX += keyX * keyLookSpeed * Time.deltaTime;
                deltaY += keyY * keyLookSpeed * Time.deltaTime;
            }
#else
            deltaX = Input.GetAxisRaw("Mouse X") * (lookSensitivity * 1.5f);
            deltaY = Input.GetAxisRaw("Mouse Y") * (lookSensitivity * 1.5f);

            float keyX = 0f;
            float keyY = 0f;
            if (Input.GetKey(KeyCode.RightArrow)) keyX += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) keyX -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) keyY += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) keyY -= 1f;

            deltaX += keyX * keyLookSpeed * Time.deltaTime;
            deltaY += keyY * keyLookSpeed * Time.deltaTime;
#endif
            return new Vector2(deltaX, deltaY);
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void SetInputLocked(bool locked)
        {
            _isInputLocked = locked;
            SetCursorLocked(!locked);
        }
    }
}
