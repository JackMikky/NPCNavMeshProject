using UnityEngine;

/// <summary>
/// Controls a hovering drone that loosely follows a target.
/// The drone body only moves when the target gets too far away,
/// and the mounted camera always smoothly looks at the target
/// (useful for a UI RawImage fed by the camera's RenderTexture).
/// </summary>
public enum DroneRotationMode
{
    /// <summary>Body yaw and camera pitch automatically track the target.</summary>
    FollowTarget,
    /// <summary>Automatic rotation is disabled; body and camera keep their current orientation (e.g. for manual/free-look control).</summary>
    Manual
}

public class DroneFollower : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The transform the drone should follow and film.")]
    public Transform target;

    [Header("Camera")]
    [Tooltip("The camera mounted on the drone. Its Target Texture should be assigned to a RenderTexture used by a UI RawImage.")]
    public Transform cameraMount;

    [Tooltip("How fast the mounted camera pitches (up/down) to look at the target.")]
    public float cameraLookSpeed = 5f;

    [Tooltip("How fast the drone body yaws (left/right) to face the target.")]
    public float bodyYawSpeed = 5f;

    [Tooltip("Clamp the camera's local pitch angle (up/down look limits).")]
    public Vector2 pitchAngleLimits = new Vector2(-80f, 80f);

    [Tooltip("Whether the body/camera should automatically track the target, or be left alone for manual control.")]
    public DroneRotationMode rotationMode = DroneRotationMode.FollowTarget;

    [Header("Follow Settings")]
    [Tooltip("Desired hover offset relative to the target (world space).")]
    public Vector3 hoverOffset = new Vector3(0f, 4f, -3f);

    [Tooltip("The drone starts moving again once the distance to the target exceeds this value.")]
    public float followDistance = 8f;

    [Tooltip("The drone stops moving once it gets within this distance of the target (should be <= followDistance to avoid jitter).")]
    public float stopDistance = 4f;

    [Tooltip("Movement speed of the drone body when it needs to catch up.")]
    public float moveSpeed = 4f;

    [Tooltip("How quickly the drone accelerates/decelerates towards its move speed.")]
    public float acceleration = 3f;

    private bool isMoving = false;
    private float currentSpeed = 0f;

    private void Reset()
    {
        cameraMount = GetComponentInChildren<Camera>()?.transform;
    }

    private void Update()
    {
        if (target == null) return;

        UpdateMovement();
        UpdateCameraLook();
    }

    private void UpdateMovement()
    {
        Vector3 desiredPosition = target.position + hoverOffset;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Hysteresis: start moving when far, stop when close enough.
        if (!isMoving && distanceToTarget > followDistance)
        {
            isMoving = true;
        }
        else if (isMoving && distanceToTarget <= stopDistance)
        {
            isMoving = false;
        }

        if (isMoving)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, Time.deltaTime * acceleration);

            Vector3 newPosition = Vector3.MoveTowards(transform.position, desiredPosition, currentSpeed * Time.deltaTime);
            transform.position = newPosition;
        }
        else
        {
            // Decelerate to a stop while hovering in place.
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * acceleration);
        }
    }

    /// <summary>
    /// Yaw (left/right) is handled by rotating the drone body towards the target,
    /// while pitch (up/down) is handled locally by the camera mount so the body
    /// stays level and only the camera tilts to look up/down.
    /// </summary>
    private void UpdateCameraLook()
    {
        if (rotationMode != DroneRotationMode.FollowTarget) return;
        if (cameraMount == null) return;

        Vector3 toTarget = target.position - transform.position;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        // --- Body yaw (left/right) ---
        Vector3 flatDirection = new Vector3(toTarget.x, 0f, toTarget.z);
        if (flatDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredYawRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredYawRotation, Time.deltaTime * bodyYawSpeed);
        }

        // --- Camera pitch (up/down), computed in the drone's local space ---
        Vector3 localDirection = transform.InverseTransformDirection(toTarget.normalized);
        float pitchAngle = Mathf.Atan2(-localDirection.y, new Vector2(localDirection.x, localDirection.z).magnitude) * Mathf.Rad2Deg;
        pitchAngle = Mathf.Clamp(pitchAngle, pitchAngleLimits.x, pitchAngleLimits.y);

        Quaternion desiredLocalRotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        cameraMount.localRotation = Quaternion.Slerp(cameraMount.localRotation, desiredLocalRotation, Time.deltaTime * cameraLookSpeed);
    }

    /// <summary>Switches between automatic target-tracking rotation and manual (untouched) rotation.</summary>
    public void SetRotationMode(DroneRotationMode mode)
    {
        rotationMode = mode;
    }

    /// <summary>Convenience toggle between FollowTarget and Manual modes.</summary>
    public void ToggleRotationMode()
    {
        rotationMode = rotationMode == DroneRotationMode.FollowTarget
            ? DroneRotationMode.Manual
            : DroneRotationMode.FollowTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position, followDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, stopDistance);
    }
}
