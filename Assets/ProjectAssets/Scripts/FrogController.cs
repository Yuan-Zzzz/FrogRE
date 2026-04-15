using UnityEngine;

public class FrogController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float baseJumpForce = 10f;
    [SerializeField] private float scaleBonusJumpForce = 6f;
    [SerializeField] private float maxChargeTime = 0.6f;
    [SerializeField] private float minJumpRatio = 0.25f;
    [SerializeField] private float jumpRewardToShoot = 0.4f;
    [SerializeField] private float upImpulse = 7.5f;
    [SerializeField] private float scaleBonusUpImpulse = 1.8f;
    [SerializeField] private float groundCheckLength = 1.2f;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckExtraDistance = 0.2f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private LayerMask groundLayerMask;
    
    [Header("Air Feel (Anti-Floaty)")]
    [SerializeField] private float riseGravityMultiplier = 1.8f;
    [SerializeField] private float apexGravityMultiplier = 2.2f;
    [SerializeField] private float fallGravityMultiplier = 3.4f;
    [SerializeField] private float apexVelocityThreshold = 1.1f;
    [SerializeField] private float maxFallSpeed = 25f;
    [SerializeField] private float groundedPlanarDamping = 6f;

    [Header("Visual Feedback")]
    [SerializeField] private float prepareScaleY = 0.35f;
    [SerializeField] private float normalScaleY = 1f;
    [SerializeField] private float scaleSmoothSpeed = 12f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform viewTransform;
    [SerializeField] private Transform chargeIndicator;
    [SerializeField] private ParticleSystem jumpDustParticle;

    [Header("Debug(ReadOnly)")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isCharging;
    [SerializeField] private bool isJumpQueued;
    [SerializeField] private float chargeTimer;
    [SerializeField] private float jumpQueuedAtTime;
    [SerializeField] private float lastGroundedTime;

    private Rigidbody rb;
    private Collider cachedCollider;
    private FrogRE.FrogShooter frogShooter;
    private Camera cachedMainCamera;
    private Vector3 baseViewScale;
    private Vector3 targetViewScale;
    private Renderer chargeIndicatorRenderer;
    private MaterialPropertyBlock chargeMpb;

    private static readonly int ChargeId = Shader.PropertyToID("_Charge");
    private static readonly int ActiveId = Shader.PropertyToID("_Active");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
        frogShooter = GetComponent<FrogRE.FrogShooter>();
        cachedMainCamera = Camera.main;

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }

        if (viewTransform == null)
        {
            var foundView = transform.Find("View");
            if (foundView != null)
            {
                viewTransform = foundView;
            }
        }

        if (viewTransform != null)
        {
            baseViewScale = viewTransform.localScale;
            targetViewScale = GetNormalViewScale();
        }

        if (chargeIndicator != null)
        {
            chargeIndicatorRenderer = chargeIndicator.GetComponentInChildren<Renderer>();
            chargeMpb = new MaterialPropertyBlock();
            SetChargeMaterialState(0f, false);
        }
    }

    private void Update()
    {
        UpdateGroundState();
        HandleMouseLook();
        HandleJumpInput();
        UpdateViewScale();
        UpdateChargeIndicator();
    }

    private void FixedUpdate()
    {
        if (isJumpQueued)
        {
            TryPerformQueuedJump();
        }

        ApplyJumpFeelPhysics();
    }

    private void HandleMouseLook()
    {
        if (rb == null)
        {
            return;
        }

        if (cachedMainCamera == null)
        {
            cachedMainCamera = Camera.main;
            if (cachedMainCamera == null)
            {
                return;
            }
        }

        var ray = cachedMainCamera.ScreenPointToRay(Input.mousePosition);
        var groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (!groundPlane.Raycast(ray, out var enter))
        {
            return;
        }

        var hitPoint = ray.GetPoint(enter);
        var lookDir = hitPoint - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        var targetRotation = Quaternion.LookRotation(lookDir.normalized);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed));
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && CanStartCharge())
        {
            chargeTimer = maxChargeTime;
            isCharging = false;
            targetViewScale = GetNormalViewScale();
            isJumpQueued = true;
            jumpQueuedAtTime = Time.time;
            return;
        }

        if (Input.GetMouseButtonDown(1) && CanStartCharge())
        {
            isCharging = true;
            chargeTimer = 0f;
            targetViewScale = new Vector3(baseViewScale.x, baseViewScale.y * prepareScaleY, baseViewScale.z);
        }

        if (isCharging && Input.GetMouseButton(1))
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer > maxChargeTime)
            {
                chargeTimer = maxChargeTime;
            }
        }

        if (!isCharging || !Input.GetMouseButtonUp(1))
        {
            return;
        }

        isCharging = false;
        targetViewScale = GetNormalViewScale();
        isJumpQueued = true;
        jumpQueuedAtTime = Time.time;
    }

    private bool CanStartCharge()
    {
        return isGrounded || Time.time - lastGroundedTime <= coyoteTime;
    }

    private void TryPerformQueuedJump()
    {
        if (Time.time - jumpQueuedAtTime > jumpBufferTime)
        {
            isJumpQueued = false;
            return;
        }

        if (!CanStartCharge())
        {
            return;
        }

        PerformJump();
    }

    private void PerformJump()
    {
        if (rb == null)
        {
            isJumpQueued = false;
            chargeTimer = 0f;
            return;
        }

        var chargeRatio = Mathf.Clamp01(chargeTimer / Mathf.Max(0.01f, maxChargeTime));
        var currentScale = transform.localScale.x;
        var chargedRatio = Mathf.Max(minJumpRatio, chargeRatio);
        var forwardImpulse = (baseJumpForce + currentScale * scaleBonusJumpForce) * chargedRatio;
        var verticalImpulse = (upImpulse + currentScale * scaleBonusUpImpulse) * chargedRatio;

        // Keep a small amount of existing planar motion so jump feels responsive, not sticky.
        var retainedPlanarVelocity = Vector3.ProjectOnPlane(rb.velocity, Vector3.up) * 0.3f;
        rb.velocity = retainedPlanarVelocity;
        rb.AddForce(transform.forward * forwardImpulse + Vector3.up * verticalImpulse, ForceMode.Impulse);
        isJumpQueued = false;
        chargeTimer = 0f;
        SetChargeMaterialState(0f, false);

        if (jumpDustParticle != null)
        {
            var shape = jumpDustParticle.shape;
            shape.scale = new Vector3(currentScale, shape.scale.y, shape.scale.z);
            jumpDustParticle.Play();
        }

        if (frogShooter != null)
        {
            frogShooter.ApplyJumpShootBonus(jumpRewardToShoot);
        }
    }

    private void UpdateGroundState()
    {
        if (groundLayerMask.value == 0)
        {
            // Fallback to everything to avoid "wrong layer causes never grounded".
            groundLayerMask = ~0;
        }

        var origin = transform.position + Vector3.up * 0.05f;
        float castDistance = groundCheckLength;
        float castRadius = groundCheckRadius;

        if (cachedCollider != null)
        {
            // Make ground check scale with frog size.
            var bounds = cachedCollider.bounds;
            origin = bounds.center;
            castDistance = bounds.extents.y + groundCheckExtraDistance;
            castRadius = Mathf.Max(groundCheckRadius, Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.5f);
        }

        isGrounded = Physics.SphereCast(
            origin,
            castRadius,
            Vector3.down,
            out _,
            castDistance,
            groundLayerMask,
            QueryTriggerInteraction.Ignore
        );

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private void UpdateViewScale()
    {
        if (viewTransform == null)
        {
            return;
        }

        viewTransform.localScale = Vector3.Lerp(
            viewTransform.localScale,
            targetViewScale,
            scaleSmoothSpeed * Time.deltaTime
        );
    }

    private Vector3 GetNormalViewScale()
    {
        return new Vector3(baseViewScale.x, baseViewScale.y * normalScaleY, baseViewScale.z);
    }

    private void UpdateChargeIndicator()
    {
        if (chargeIndicator == null)
        {
            return;
        }

        if (!isCharging)
        {
            chargeIndicator.localScale = Vector3.one;
            SetChargeMaterialState(0f, false);
            return;
        }

        var ratio = Mathf.Clamp01(chargeTimer / Mathf.Max(0.01f, maxChargeTime));
        var radius = Mathf.Lerp(1f, 2.5f, ratio);
        chargeIndicator.localScale = new Vector3(radius, radius, radius);
        chargeIndicator.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
        SetChargeMaterialState(ratio, true);
    }

    private void SetChargeMaterialState(float ratio, bool isActive)
    {
        if (chargeIndicatorRenderer == null)
        {
            return;
        }

        chargeIndicatorRenderer.GetPropertyBlock(chargeMpb);
        chargeMpb.SetFloat(ChargeId, ratio);
        chargeMpb.SetFloat(ActiveId, isActive ? 1f : 0f);
        chargeIndicatorRenderer.SetPropertyBlock(chargeMpb);
    }

    private void ApplyJumpFeelPhysics()
    {
        if (rb == null)
        {
            return;
        }

        var velocity = rb.velocity;
        if (isGrounded)
        {
            // Reduce ground sliding so the character feels heavier and more controllable.
            if (!isCharging)
            {
                var dampFactor = Mathf.Clamp01(groundedPlanarDamping * Time.fixedDeltaTime);
                velocity.x = Mathf.Lerp(velocity.x, 0f, dampFactor);
                velocity.z = Mathf.Lerp(velocity.z, 0f, dampFactor);
                rb.velocity = velocity;
            }
            return;
        }

        float gravityMultiplier;
        if (velocity.y > apexVelocityThreshold)
        {
            gravityMultiplier = riseGravityMultiplier;
        }
        else if (velocity.y < -apexVelocityThreshold)
        {
            gravityMultiplier = fallGravityMultiplier;
        }
        else
        {
            gravityMultiplier = apexGravityMultiplier;
        }

        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        if (rb.velocity.y < -maxFallSpeed)
        {
            var clampedVelocity = rb.velocity;
            clampedVelocity.y = -maxFallSpeed;
            rb.velocity = clampedVelocity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var origin = transform.position + Vector3.up * 0.05f;
        var distance = groundCheckLength;
        var radius = groundCheckRadius;
        if (cachedCollider != null)
        {
            origin = cachedCollider.bounds.center;
            distance = cachedCollider.bounds.extents.y + groundCheckExtraDistance;
            radius = Mathf.Max(groundCheckRadius, Mathf.Min(cachedCollider.bounds.extents.x, cachedCollider.bounds.extents.z) * 0.5f);
        }

        Gizmos.DrawWireSphere(origin + Vector3.down * distance, radius);
        Gizmos.DrawLine(origin, origin + Vector3.down * distance);
    }
}
