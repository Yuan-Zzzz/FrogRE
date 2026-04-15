using QFramework;
using QFramework.Example;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float heightOffset = 15f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float backwardOffset = 8f;

    [Header("Mouse Follow Settings")]
    [SerializeField] private bool followMouse = true;
    [SerializeField] private float mouseInfluence = 0.3f;
    [SerializeField] private float maxMouseDistance = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private float minTiltAngle = 60f;
    [SerializeField] private float maxTiltAngle = 80f;
    [SerializeField] private float rotationSmoothSpeed = 3f;

    [Header("Collision Settings")]
    [SerializeField] private float cameraDistance = 15f;
    [SerializeField] private float collisionRadius = 0.5f;
    [SerializeField] private LayerMask collisionLayers;

    private Vector3 heightVelocity;
    private Vector3 positionVelocity;
    private Vector3 desiredPosition;
    private Vector3 adjustedPosition;
    private float currentTiltAngle;
    private float currentHeightOffset;
    private float currentYaw;

    private Rigidbody targetRigidbody;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody>();

            currentHeightOffset = target.position.y + heightOffset;
            Vector3 backwardDir = Vector3.back * backwardOffset;
            Vector3 startPos = target.position + backwardDir;
            startPos.y = currentHeightOffset;
            transform.position = startPos;
            currentTiltAngle = maxTiltAngle;
            currentYaw = transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(currentTiltAngle, currentYaw, 0);
        }

        if (collisionLayers == 0)
            collisionLayers = ~LayerMask.GetMask("Ignore Raycast");
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate rotation based on target movement direction
        UpdateTiltAngle();

        // Get mouse position on ground plane
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // Calculate blended center point between target and mouse
        Vector3 blendTarget = target.position;
        if (followMouse && mouseWorldPos != Vector3.zero)
        {
            Vector3 offsetFromTarget = mouseWorldPos - target.position;
            if (maxMouseDistance > 0 && offsetFromTarget.magnitude > maxMouseDistance)
            {
                offsetFromTarget = offsetFromTarget.normalized * maxMouseDistance;
                mouseWorldPos = target.position + offsetFromTarget;
            }
            blendTarget = Vector3.Lerp(target.position, mouseWorldPos, mouseInfluence);
        }

        // Target position calculation
        Vector3 backwardDir = Quaternion.Euler(0, currentYaw, 0) * Vector3.back;
        Vector3 targetPos = blendTarget + backwardDir * backwardOffset;
        targetPos.y = blendTarget.y + heightOffset; // Direct target height without extra smoothdamp

        // Raycast for collision
        RaycastHit hit;
        float adjustedDistance = cameraDistance;
        Vector3 direction = Quaternion.Euler(currentTiltAngle, 0, 0) * Vector3.down;

        if (Physics.SphereCast(blendTarget, collisionRadius, direction, out hit, cameraDistance, collisionLayers))
        {
            adjustedDistance = Mathf.Max(0.5f, hit.distance - collisionRadius);
        }

        // Final position smoothing (one smoothdamp to rule them all)
        float smoothTime = 1f / followSpeed;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref positionVelocity, smoothTime);

        transform.rotation = Quaternion.Euler(currentTiltAngle, currentYaw, 0);
    }

    Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return Vector3.zero;
    }

    void UpdateTiltAngle()
    {
        if (target == null) return;

        if (targetRigidbody == null) targetRigidbody = target.GetComponent<Rigidbody>();

        if (targetRigidbody != null)
        {
            // Get target velocity magnitude
            float speed = targetRigidbody.velocity.magnitude;

            // Map speed to angle range (faster = more tilt toward min angle for dynamic feel)
            float targetAngle = Mathf.Lerp(maxTiltAngle, minTiltAngle, Mathf.Clamp01(speed / 10f));

            // Smooth transition
            currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetAngle, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
