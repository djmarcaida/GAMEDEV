using UnityEngine;

/*
    This script provides jumping, crouching, and movement in Unity 3D - Gatsby & Antigravity
    Compatible with Unity 6.
*/

public class Player : MonoBehaviour
{
    // Camera Rotation & Perspective
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
    private float verticalRotation = 0f;
    private Transform cameraTransform;
    private Camera playerCamera;
    private Transform visualTransform;
    private Renderer visualRenderer;
    
    // Ground Movement
    [Header("Movement")]
    private Rigidbody rb;
    public float MoveSpeed = 5f;
    private float moveHorizontal;
    private float moveForward;

    // Jumping
    [Header("Jumping & Ground Check")]
    public float jumpForce = 12f;
    public float fallMultiplier = 2.5f; // Multiplies gravity when falling down
    public float ascendMultiplier = 2f; // Multiplies gravity if jump button is released early
    public LayerMask groundLayer;
    private bool isGrounded = true;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.2f;
    private float playerHeight;
    private float raycastDistance;

    // Crouching (Essential for dodging high lasers)
    [Header("Crouch Settings")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode alternateCrouchKey = KeyCode.C;
    public float crouchHeight = 1.0f;
    public float crouchSpeedMultiplier = 0.6f;
    private float standingHeight;
    private Vector3 standingCenter;
    private CapsuleCollider capsuleCollider;
    private Vector3 originalCameraLocalPos;
    private bool isCrouching = false;

    public bool IsGrounded => isGrounded;
    public bool IsCrouching => isCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cameraTransform = Camera.main != null ? Camera.main.transform : transform.Find("Main Camera");
        if (cameraTransform != null)
        {
            originalCameraLocalPos = cameraTransform.localPosition;
            playerCamera = cameraTransform.GetComponent<Camera>();
        }
        if (playerCamera == null) playerCamera = Camera.main;

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = isThirdPerson ? thirdPersonFOV : firstPersonFOV;
        }

        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            standingHeight = capsuleCollider.height;
            standingCenter = capsuleCollider.center;
            playerHeight = standingHeight * transform.localScale.y;
        }
        else
        {
            standingHeight = 2.0f;
            standingCenter = Vector3.zero;
            playerHeight = 2.0f;
        }

        // Set the raycast to be slightly beneath the player's feet
        raycastDistance = (playerHeight / 2) + 0.35f;

        // Auto-configure groundLayer if left empty or 0: include Default (0) and Ground (3)
        if (groundLayer.value == 0)
        {
            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            groundLayer = (1 << 0) | (groundLayerIndex != -1 ? (1 << groundLayerIndex) : 0);
        }

        // Ensure PlayerHealth component is attached
        if (GetComponent<PlayerHealth>() == null)
        {
            gameObject.AddComponent<PlayerHealth>();
        }

        // Setup visual character representation (Cylinder)
        SetupVisualRepresentation();
        UpdateVisualVisibility();

        // Snap camera immediately to start position with full-body framing
        if (cameraTransform != null && isThirdPerson)
        {
            Vector3 bodyCenter = transform.position;
            Quaternion cameraOrbit = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, 0f);
            Vector3 desiredPos = bodyCenter + cameraOrbit * thirdPersonOffset;
            Vector3 lookTarget = bodyCenter + cameraOrbit * new Vector3(0f, 0.12f, 0.6f);
            cameraTransform.position = desiredPos;
            cameraTransform.rotation = Quaternion.LookRotation(lookTarget - desiredPos);
        }

        // Hides the mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupVisualRepresentation()
    {
        // Clean up procedural humanoid model if present
        Transform humanoidModel = transform.Find("HumanoidModel");
        if (humanoidModel != null)
        {
            Destroy(humanoidModel.gameObject);
        }

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
            GameObject modelObj = new GameObject("Model");
            modelObj.transform.SetParent(transform, false);
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localRotation = Quaternion.identity;
            modelObj.transform.localScale = Vector3.one;

            MeshFilter newMf = modelObj.AddComponent<MeshFilter>();
            newMf.sharedMesh = mf.sharedMesh;
            MeshRenderer newMr = modelObj.AddComponent<MeshRenderer>();
            newMr.sharedMaterials = mr.sharedMaterials;
            newMr.shadowCastingMode = isThirdPerson ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            newMr.receiveShadows = true;

            // Disable parent renderer so only the Model child renders
            mr.enabled = false;
            visualTransform = modelObj.transform;
            visualRenderer = newMr;
        }
    }

    private void UpdateVisualVisibility()
    {
        if (visualRenderer == null && visualTransform != null)
        {
            visualRenderer = visualTransform.GetComponent<Renderer>();
        }
        if (visualRenderer != null)
        {
            visualRenderer.shadowCastingMode = isThirdPerson ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    void Update()
    {
        // Toggle perspective with hotkey (V by default)
        if (Input.GetKeyDown(togglePerspectiveKey))
        {
            isThirdPerson = !isThirdPerson;
            UpdateVisualVisibility();
            Debug.Log($"[Player] Perspective toggled: {(isThirdPerson ? "Third Person" : "First Person")}");
        }

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        RotateCamera();
        HandleCrouch();

        // Ground detection
        if (groundCheckTimer > 0f)
        {
            groundCheckTimer -= Time.deltaTime;
        }
        else
        {
            isGrounded = CheckGrounded();
        }

        // Jump Input
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[Player] Jump pressed! isGrounded: {isGrounded}, Timer: {groundCheckTimer}");
            
            if (isGrounded && !isCrouching)
            {
                Jump();
            }
        }
    }

    private bool CheckGrounded()
    {
        // Solid layer mask: groundLayer + Default (0) + Ground (3), excluding player's own layer & triggers
        int groundIndex = LayerMask.NameToLayer("Ground");
        int mask = groundLayer.value;
        if (mask == 0) mask = 1 << 0;
        mask |= (1 << 0); // Always allow Default layer
        if (groundIndex != -1) mask |= (1 << groundIndex); // Always allow Ground layer
        mask &= ~(1 << gameObject.layer); // Never hit self
        mask &= ~LayerMask.GetMask("Ignore Raycast");

        // 1. Sphere check at player's feet
        Vector3 feetPosition;
        float checkRadius = 0.3f;
        if (capsuleCollider != null)
        {
            feetPosition = transform.TransformPoint(capsuleCollider.center) - Vector3.up * (capsuleCollider.height * transform.lossyScale.y * 0.5f - 0.15f);
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
            {
                return true;
            }
        }

        // 2. Backup Raycast straight down from center
        Vector3 rayStart = transform.TransformPoint(capsuleCollider != null ? capsuleCollider.center : Vector3.zero);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != capsuleCollider && !hit.transform.IsChildOf(transform) && !hit.collider.isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (groundCheckTimer > 0f) return;
        // Verify collision contact normals pointing upward
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void HandleCrouch()
    {
        bool wantsCrouch = Input.GetKey(crouchKey) || Input.GetKey(alternateCrouchKey);

        if (wantsCrouch && !isCrouching)
        {
            isCrouching = true;
            if (capsuleCollider != null)
            {
                capsuleCollider.height = crouchHeight;
                // Move center down to keep bottom of collider anchored at the floor
                float heightDiff = standingHeight - crouchHeight;
                capsuleCollider.center = standingCenter - Vector3.up * (heightDiff * 0.5f);
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

    void FixedUpdate()
    {
        MovePlayer();
        ApplyJumpPhysics();
    }

    void LateUpdate()
    {
        UpdateCameraTransform();
    }

    void UpdateCameraTransform()
    {
        if (cameraTransform == null) return;

        // Smoothly adjust Field of View according to active perspective
        if (playerCamera != null)
        {
            float targetFOV = isThirdPerson ? thirdPersonFOV : firstPersonFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * cameraSmoothSpeed);
        }

        if (isThirdPerson)
        {
            // Center of player body (transform.position is at local y = 0, exactly middle of 2m height)
            float crouchDrop = isCrouching ? (standingHeight - crouchHeight) * 0.5f : 0f;
            Vector3 bodyCenter = transform.position - Vector3.up * crouchDrop;

            // Camera orbits around body center with mouse pitch (verticalRotation) and player yaw
            Quaternion cameraOrbit = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, 0f);
            Vector3 desiredPos = bodyCenter + cameraOrbit * thirdPersonOffset;

            // Look target: framed at upper chest (+0.12m above center), keeping entire body (head to feet) visible
            Vector3 lookTarget = bodyCenter + cameraOrbit * new Vector3(0f, 0.12f, 0.6f);
            Quaternion targetRot = Quaternion.LookRotation(lookTarget - desiredPos);

            // Camera collision prevention against environment
            Vector3 castDir = desiredPos - bodyCenter;
            float targetDist = castDir.magnitude;
            if (targetDist > 0.05f)
            {
                castDir.Normalize();
                int collisionMask = ~((1 << gameObject.layer) | LayerMask.GetMask("Ignore Raycast"));
                if (Physics.SphereCast(bodyCenter, cameraCollisionRadius, castDir, out RaycastHit hit, targetDist, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float safeDist = Mathf.Max(0.6f, hit.distance - 0.1f);
                    desiredPos = bodyCenter + castDir * safeDist;
                    targetRot = Quaternion.LookRotation(lookTarget - desiredPos);
                }
            }

            // Snap if teleported (e.g. after respawn), otherwise smooth follow
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
            // First Person mode
            float eyeY = isCrouching ? (originalCameraLocalPos.y - (standingHeight - crouchHeight) * 0.6f) : originalCameraLocalPos.y;
            Vector3 fpLocalPos = new Vector3(originalCameraLocalPos.x, eyeY, originalCameraLocalPos.z);
            cameraTransform.localPosition = fpLocalPos;
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    void MovePlayer()
    {
        Vector3 movement = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        float currentSpeed = isCrouching ? MoveSpeed * crouchSpeedMultiplier : MoveSpeed;
        Vector3 targetVelocity = movement * currentSpeed;

        // Apply movement to the Rigidbody horizontal axes
        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;

        // If we aren't moving and are on the ground, stop horizontal sliding
        if (isGrounded && moveHorizontal == 0 && moveForward == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void RotateCamera()
    {
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        float minAngle = isThirdPerson ? minVerticalAngle : -85f;
        float maxAngle = isThirdPerson ? maxVerticalAngle : 85f;
        verticalRotation = Mathf.Clamp(verticalRotation, minAngle, maxAngle);
    }

    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        // Apply vertical jump impulse directly, resetting any downward momentum for consistent jump height
        Vector3 vel = rb.linearVelocity;
        vel.y = jumpForce;
        rb.linearVelocity = vel;
        Debug.Log($"[Player] Jump executed! JumpForce: {jumpForce}, y-velocity: {rb.linearVelocity.y}");
    }

    void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0) 
        {
            // Falling: Apply fall multiplier for snappy descent
            rb.linearVelocity += Vector3.up * (Physics.gravity.y * Mathf.Max(0f, fallMultiplier - 1f) * Time.fixedDeltaTime);
        }
        else if (rb.linearVelocity.y > 0 && (!Input.GetButton("Jump") && !Input.GetKey(KeyCode.Space)))
        {
            // Jump button released early: short hop damping
            rb.linearVelocity += Vector3.up * (Physics.gravity.y * Mathf.Max(0f, ascendMultiplier - 1f) * Time.fixedDeltaTime);
        }
    }
}
