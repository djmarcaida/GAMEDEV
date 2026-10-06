using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Camera & Perspective")]
    public bool isThirdPerson = true;
    public KeyCode togglePerspectiveKey = KeyCode.V;
    public float thirdPersonFOV = 95f;
    public float firstPersonFOV = 85f;
    public Vector3 thirdPersonOffset = new Vector3(0f, 0.85f, -3.8f);
    public Vector3 firstPersonOffset = new Vector3(0f, 0.55f, 0f);
    public float cameraCollisionRadius = 0.22f;
    public float cameraSmoothSpeed = 18f;
    public float minVerticalAngle = -35f;
    public float maxVerticalAngle = 60f;
    public float mouseSensitivity = 2f;

    [Header("Movement")]
    public float MoveSpeed = 5f;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float fallMultiplier = 2.5f;
    public float ascendMultiplier = 2f;
    public LayerMask groundLayer;

    [Header("Crouch")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode alternateCrouchKey = KeyCode.C;
    public float crouchHeight = 1.0f;
    public float crouchSpeedMultiplier = 0.6f;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Transform cameraTransform;
    private Camera playerCamera;
    private Transform visualTransform;
    private Renderer visualRenderer;

    private float moveHorizontal;
    private float moveForward;
    private float verticalRotation;
    private float standingHeight = 2.0f;
    private Vector3 standingCenter;
    private float playerHeight = 2.0f;
    private float raycastDistance;
    private Vector3 originalCameraLocalPos;

    private bool isGrounded = true;
    private float groundCheckTimer;
    private float groundCheckDelay = 0.2f;
    private bool isCrouching;

    public bool IsGrounded => isGrounded;
    public bool IsCrouching => isCrouching;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        cameraTransform = Camera.main != null ? Camera.main.transform : transform.Find("Main Camera");
        if (cameraTransform != null)
        {
            originalCameraLocalPos = cameraTransform.localPosition;
            playerCamera = cameraTransform.GetComponent<Camera>();
        }
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
            playerCamera.fieldOfView = isThirdPerson ? thirdPersonFOV : firstPersonFOV;

        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            standingHeight = capsuleCollider.height;
            standingCenter = capsuleCollider.center;
            playerHeight = standingHeight * transform.localScale.y;
        }

        raycastDistance = (playerHeight * 0.5f) + 0.35f;

        if (groundLayer.value == 0)
        {
            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            groundLayer = (1 << 0) | (groundLayerIndex != -1 ? (1 << groundLayerIndex) : 0);
        }

        if (GetComponent<PlayerHealth>() == null)
            gameObject.AddComponent<PlayerHealth>();

        SetupVisualRepresentation();
        UpdateVisualVisibility();

        if (cameraTransform != null && isThirdPerson)
        {
            Vector3 bodyCenter = transform.position;
            Quaternion orbit = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, 0f);
            Vector3 desiredPos = bodyCenter + orbit * thirdPersonOffset;
            Vector3 lookTarget = bodyCenter + orbit * new Vector3(0f, 0.12f, 0.6f);
            cameraTransform.position = desiredPos;
            cameraTransform.rotation = Quaternion.LookRotation(lookTarget - desiredPos);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupVisualRepresentation()
    {
        Transform humanoidModel = transform.Find("HumanoidModel");
        if (humanoidModel != null)
            Destroy(humanoidModel.gameObject);

        visualTransform = transform.Find("Model");
        if (visualTransform != null)
        {
            visualRenderer = visualTransform.GetComponent<Renderer>();
            return;
        }

        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mf != null && mr != null)
        {
            var modelObj = new GameObject("Model");
            modelObj.transform.SetParent(transform, false);
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localRotation = Quaternion.identity;
            modelObj.transform.localScale = Vector3.one;

            var newMf = modelObj.AddComponent<MeshFilter>();
            newMf.sharedMesh = mf.sharedMesh;

            var newMr = modelObj.AddComponent<MeshRenderer>();
            newMr.sharedMaterials = mr.sharedMaterials;
            newMr.shadowCastingMode = isThirdPerson
                ? UnityEngine.Rendering.ShadowCastingMode.On
                : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            newMr.receiveShadows = true;

            mr.enabled = false;
            visualTransform = modelObj.transform;
            visualRenderer = newMr;
        }
    }

    private void UpdateVisualVisibility()
    {
        if (visualRenderer == null && visualTransform != null)
            visualRenderer = visualTransform.GetComponent<Renderer>();

        if (visualRenderer != null)
        {
            visualRenderer.shadowCastingMode = isThirdPerson
                ? UnityEngine.Rendering.ShadowCastingMode.On
                : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(togglePerspectiveKey))
        {
            isThirdPerson = !isThirdPerson;
            UpdateVisualVisibility();
        }

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        RotateCamera();
        HandleCrouch();

        if (groundCheckTimer > 0f)
            groundCheckTimer -= Time.deltaTime;
        else
            isGrounded = CheckGrounded();

        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)) && isGrounded && !isCrouching)
        {
            Jump();
        }
    }

    private bool CheckGrounded()
    {
        int groundIndex = LayerMask.NameToLayer("Ground");
        int mask = groundLayer.value == 0 ? (1 << 0) : groundLayer.value;
        mask |= (1 << 0);
        if (groundIndex != -1) mask |= (1 << groundIndex);
        mask &= ~(1 << gameObject.layer);
        mask &= ~LayerMask.GetMask("Ignore Raycast");

        Vector3 feetPosition;
        float checkRadius = 0.3f;
        if (capsuleCollider != null)
        {
            feetPosition = transform.TransformPoint(capsuleCollider.center)
                - Vector3.up * (capsuleCollider.height * transform.lossyScale.y * 0.5f - 0.15f);
            checkRadius = Mathf.Max(0.2f, capsuleCollider.radius * transform.lossyScale.x * 0.85f);
        }
        else
        {
            feetPosition = transform.position - Vector3.up * (playerHeight * 0.5f - 0.15f);
        }

        Collider[] colliders = Physics.OverlapSphere(feetPosition, checkRadius, mask, QueryTriggerInteraction.Ignore);
        foreach (var col in colliders)
        {
            if (col != capsuleCollider && !col.transform.IsChildOf(transform) && !col.isTrigger)
                return true;
        }

        Vector3 rayStart = transform.TransformPoint(capsuleCollider != null ? capsuleCollider.center : Vector3.zero);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != capsuleCollider && !hit.transform.IsChildOf(transform) && !hit.collider.isTrigger)
                return true;
        }

        return false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (groundCheckTimer > 0f) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void HandleCrouch()
    {
        bool wantsCrouch = Input.GetKey(crouchKey) || Input.GetKey(alternateCrouchKey);

        if (wantsCrouch && !isCrouching)
        {
            isCrouching = true;
            if (capsuleCollider != null)
            {
                capsuleCollider.height = crouchHeight;
                float diff = standingHeight - crouchHeight;
                capsuleCollider.center = standingCenter - Vector3.up * (diff * 0.5f);
            }
            if (visualTransform != null)
            {
                float targetYScale = crouchHeight / standingHeight;
                float targetYPos = -((standingHeight - crouchHeight) * 0.5f);
                visualTransform.localScale = new Vector3(1f, targetYScale, 1f);
                visualTransform.localPosition = new Vector3(0f, targetYPos, 0f);
            }
        }
        else if (!wantsCrouch && isCrouching)
        {
            isCrouching = false;
            if (capsuleCollider != null)
            {
                capsuleCollider.height = standingHeight;
                capsuleCollider.center = standingCenter;
            }
            if (visualTransform != null)
            {
                visualTransform.localScale = Vector3.one;
                visualTransform.localPosition = Vector3.zero;
            }
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ApplyJumpPhysics();
    }

    private void LateUpdate()
    {
        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        if (cameraTransform == null) return;

        if (playerCamera != null)
        {
            float targetFOV = isThirdPerson ? thirdPersonFOV : firstPersonFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * cameraSmoothSpeed);
        }

        if (isThirdPerson)
        {
            float crouchDrop = isCrouching ? (standingHeight - crouchHeight) * 0.5f : 0f;
            Vector3 bodyCenter = transform.position - Vector3.up * crouchDrop;

            Quaternion orbit = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, 0f);
            Vector3 desiredPos = bodyCenter + orbit * thirdPersonOffset;
            Vector3 lookTarget = bodyCenter + orbit * new Vector3(0f, 0.12f, 0.6f);
            Quaternion targetRot = Quaternion.LookRotation(lookTarget - desiredPos);

            Vector3 castDir = desiredPos - bodyCenter;
            float dist = castDir.magnitude;
            if (dist > 0.05f)
            {
                castDir.Normalize();
                int mask = ~((1 << gameObject.layer) | LayerMask.GetMask("Ignore Raycast"));
                if (Physics.SphereCast(bodyCenter, cameraCollisionRadius, castDir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
                {
                    float safeDist = Mathf.Max(0.6f, hit.distance - 0.1f);
                    desiredPos = bodyCenter + castDir * safeDist;
                    targetRot = Quaternion.LookRotation(lookTarget - desiredPos);
                }
            }

            if (Vector3.Distance(cameraTransform.position, desiredPos) > 6f)
            {
                cameraTransform.position = desiredPos;
                cameraTransform.rotation = targetRot;
            }
            else
            {
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPos, Time.deltaTime * cameraSmoothSpeed);
                cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetRot, Time.deltaTime * cameraSmoothSpeed);
            }
        }
        else
        {
            float eyeY = isCrouching ? (originalCameraLocalPos.y - (standingHeight - crouchHeight) * 0.6f) : originalCameraLocalPos.y;
            cameraTransform.localPosition = new Vector3(originalCameraLocalPos.x, eyeY, originalCameraLocalPos.z);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    private void MovePlayer()
    {
        Vector3 movement = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        float currentSpeed = isCrouching ? MoveSpeed * crouchSpeedMultiplier : MoveSpeed;
        Vector3 targetVelocity = movement * currentSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;

        if (isGrounded && moveHorizontal == 0 && moveForward == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        float minAngle = isThirdPerson ? minVerticalAngle : -85f;
        float maxAngle = isThirdPerson ? maxVerticalAngle : 85f;
        verticalRotation = Mathf.Clamp(verticalRotation, minAngle, maxAngle);
    }

    private void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;

        Vector3 vel = rb.linearVelocity;
        vel.y = jumpForce;
        rb.linearVelocity = vel;
    }

    private void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * (Physics.gravity.y * Mathf.Max(0f, fallMultiplier - 1f) * Time.fixedDeltaTime);
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector3.up * (Physics.gravity.y * Mathf.Max(0f, ascendMultiplier - 1f) * Time.fixedDeltaTime);
        }
    }
}
