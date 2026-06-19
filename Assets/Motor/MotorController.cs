using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ArduinoMotorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [Header("Force Fixed Local X Rotation")]
    [SerializeField] private Transform forceMinus90Target;
    [SerializeField] private bool forceLocalXRotation = true;
    [SerializeField] private float forcedLocalX = -90f;

    [Header("Input Mode")]
    [SerializeField] private bool useKeyboardInputForTest = true;

    [Header("Movement Direction")]
    [SerializeField] private Vector3 movementLocalAxis = Vector3.forward;

    [Header("Movement Settings")]
    [SerializeField] private float accelerationForce = 9f;
    [SerializeField] private float reverseForce = 4f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float brakePower = 6f;

    [Header("Grip / Anti Slide Settings")]
    [SerializeField] private float lateralGrip = 8f;
    [SerializeField] private float velocityAlignStrength = 5f;
    [SerializeField] private float maxSidewaysSpeed = 0.6f;
    [SerializeField] private bool applyGripOnlyWhenGrounded = true;

    [Header("Ground / Stability")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckDistance = 0.7f;
    [SerializeField] private float groundCheckStartOffset = 0.2f;
    [SerializeField] private float groundedDownForce = 5f;
    [SerializeField] private float airDownForce = 20f;
    [SerializeField] private float maxGroundedUpwardVelocity = 0.1f;
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.6f, 0f);

    [Header("Steering Settings")]
    [SerializeField] private float turnSpeed = 45f;
    [SerializeField] private float minSpeedToSteer = 0.05f;
    [SerializeField] private bool invertSteer = false;

    [Header("Arduino Steering Calibration")]
    [SerializeField] private bool autoCalibrateSteering = true;
    [SerializeField] private float steeringMin = -0.15f;
    [SerializeField] private float steeringMax = 0.55f;
    [SerializeField] private float steeringSmoothSpeed = 14f;
    [SerializeField] private float zeroHoldTime = 0.06f;

    [Header("Keyboard Test")]
    [SerializeField] private float keyboardSteerSmooth = 10f;
    [SerializeField] private float keyboardThrottleSmooth = 10f;

    [Header("Visual Steering")]
    [SerializeField] private Transform handlebarVisual;

    [Tooltip("Ön tekerin sağ-sol dönen parent objesi. Spin pivot ile aynı obje OLMAMALI.")]
    [SerializeField] private Transform frontWheelSteerRoot;

    [SerializeField] private float visualSteerAngle = 35f;
    [SerializeField] private float visualSteerSmooth = 12f;

    [SerializeField] private Vector3 handlebarSteerAxis = Vector3.up;
    [SerializeField] private Vector3 frontWheelSteerAxis = Vector3.right;
    [SerializeField] private bool invertFrontWheelSteerVisual = true;

    [Header("Visual Rotation Corrections")]
    [SerializeField] private Vector3 handlebarLocalRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 frontWheelSteerLocalRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 frontWheelSpinLocalRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 rearWheelSpinLocalRotationOffset = Vector3.zero;

    [Header("Wheel Spin")]
    [SerializeField] private Transform frontWheelSpinPivot;
    [SerializeField] private Transform rearWheelSpinPivot;
    [SerializeField] private float wheelRadius = 0.35f;
    [SerializeField] private float wheelSpinMultiplier = 1f;

    [SerializeField] private Vector3 frontWheelSpinAxis = Vector3.right;
    [SerializeField] private Vector3 rearWheelSpinAxis = Vector3.right;

    [Header("Brake")]
    [SerializeField] private bool invertBrake = false;

    private float steerInput;
    private float throttleInput;
    private float reverseInput;
    private float brakeInput;

    private float rawX;
    private float rawY;
    private float rawZ;

    private float lastNonZeroSteer;
    private float lastNonZeroSteerTime;

    private AxisControl steeringAxis;
    private AxisControl brakeAxis;

    private Quaternion handlebarStartRotation;
    private Quaternion frontWheelSteerStartRotation;
    private Quaternion frontWheelSpinStartRotation;
    private Quaternion rearWheelSpinStartRotation;

    private Quaternion handlebarBaseRotation;
    private Quaternion frontWheelSteerBaseRotation;
    private Quaternion frontWheelSpinBaseRotation;
    private Quaternion rearWheelSpinBaseRotation;

    private float currentVisualSteerAngle;
    private float frontWheelSpinAngle;
    private float rearWheelSpinAngle;

    private bool isGrounded;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.centerOfMass = centerOfMassOffset;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        ValidateWheelHierarchy();
        CaptureVisualStartRotations();
        BuildVisualBaseRotations();
        ForceTargetLocalXRotation();
    }

    private void Start()
    {
        CacheControls();
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();

        if (useKeyboardInputForTest)
        {
            ReadKeyboardInput();
        }
        else
        {
            if (Joystick.current == null)
            {
                Debug.LogWarning("Arduino Leonardo bulunamadı.");
                return;
            }

            if (steeringAxis == null || brakeAxis == null)
                CacheControls();

            ReadArduinoInput();
        }

        ApplyThrottle();
        ApplyReverse();
        ApplySteering();
        ApplyLateralGrip();
        ApplyVelocityAlignment();
        ApplyBrake();
        ApplyGroundStabilization();
        LimitHorizontalSpeed();
        StabilizeBodyRotation();
    }

    private void LateUpdate()
    {
        BuildVisualBaseRotations();

        UpdateVisualSteering();
        UpdateWheelSpin();

        ForceTargetLocalXRotation();
    }

    private void ValidateWheelHierarchy()
    {
        if (frontWheelSteerRoot != null && frontWheelSpinPivot != null)
        {
            if (frontWheelSteerRoot == frontWheelSpinPivot)
            {
                Debug.LogError(
                    "HATA: FrontWheelSteerRoot ile FrontWheelSpinPivot aynı obje olamaz. " +
                    "FrontWheelSpinPivot, FrontWheelSteerRoot'un child objesi olmalı."
                );
            }

            if (!frontWheelSpinPivot.IsChildOf(frontWheelSteerRoot))
            {
                Debug.LogWarning(
                    "UYARI: FrontWheelSpinPivot, FrontWheelSteerRoot'un child'ı değil. " +
                    "Hiyerarşi: FrontWheelSteerRoot > FrontWheelSpinPivot > FrontWheelMesh"
                );
            }
        }
    }

    private void CaptureVisualStartRotations()
    {
        if (handlebarVisual != null)
            handlebarStartRotation = handlebarVisual.localRotation;

        if (frontWheelSteerRoot != null)
            frontWheelSteerStartRotation = frontWheelSteerRoot.localRotation;

        if (frontWheelSpinPivot != null)
            frontWheelSpinStartRotation = frontWheelSpinPivot.localRotation;

        if (rearWheelSpinPivot != null)
            rearWheelSpinStartRotation = rearWheelSpinPivot.localRotation;
    }

    private void BuildVisualBaseRotations()
    {
        handlebarBaseRotation =
            handlebarStartRotation * Quaternion.Euler(handlebarLocalRotationOffset);

        frontWheelSteerBaseRotation =
            frontWheelSteerStartRotation * Quaternion.Euler(frontWheelSteerLocalRotationOffset);

        frontWheelSpinBaseRotation =
            frontWheelSpinStartRotation * Quaternion.Euler(frontWheelSpinLocalRotationOffset);

        rearWheelSpinBaseRotation =
            rearWheelSpinStartRotation * Quaternion.Euler(rearWheelSpinLocalRotationOffset);
    }

    private void CacheControls()
    {
        if (Joystick.current == null)
            return;

        steeringAxis = Joystick.current.TryGetChildControl<AxisControl>("stick/x");
        brakeAxis = Joystick.current.TryGetChildControl<AxisControl>("z");
    }

    private void ReadKeyboardInput()
    {
        float targetSteer = 0f;
        float targetThrottle = 0f;
        float targetReverse = 0f;
        float targetBrake = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                targetSteer -= 1f;

            if (Keyboard.current.dKey.isPressed)
                targetSteer += 1f;

            if (Keyboard.current.wKey.isPressed)
                targetThrottle = 1f;

            if (Keyboard.current.sKey.isPressed)
                targetReverse = 1f;

            if (Keyboard.current.spaceKey.isPressed)
                targetBrake = 1f;
        }

        steerInput = Mathf.Lerp(
            steerInput,
            targetSteer,
            keyboardSteerSmooth * Time.fixedDeltaTime
        );

        throttleInput = Mathf.Lerp(
            throttleInput,
            targetThrottle,
            keyboardThrottleSmooth * Time.fixedDeltaTime
        );

        reverseInput = Mathf.Lerp(
            reverseInput,
            targetReverse,
            keyboardThrottleSmooth * Time.fixedDeltaTime
        );

        brakeInput = targetBrake;
    }

    private void ReadArduinoInput()
    {
        ReadArduinoSteering();
        ReadArduinoThrottle();
        ReadArduinoBrake();

        reverseInput = 0f;
    }

    private void ReadArduinoSteering()
    {
        if (steeringAxis == null)
        {
            steerInput = 0f;
            return;
        }

        rawX = steeringAxis.ReadUnprocessedValue();

        if (autoCalibrateSteering)
        {
            if (rawX < steeringMin)
                steeringMin = rawX;

            if (rawX > steeringMax)
                steeringMax = rawX;
        }

        float targetSteer = 0f;

        if (rawX < 0f)
        {
            float range = Mathf.Abs(steeringMin);

            if (range < 0.0001f)
                range = 0.0001f;

            targetSteer = rawX / range;
        }
        else if (rawX > 0f)
        {
            float range = Mathf.Abs(steeringMax);

            if (range < 0.0001f)
                range = 0.0001f;

            targetSteer = rawX / range;
        }

        targetSteer = Mathf.Clamp(targetSteer, -1f, 1f);

        if (invertSteer)
            targetSteer *= -1f;

        if (Mathf.Abs(targetSteer) > 0.0001f)
        {
            lastNonZeroSteer = targetSteer;
            lastNonZeroSteerTime = Time.time;
        }
        else
        {
            if (Time.time - lastNonZeroSteerTime <= zeroHoldTime)
                targetSteer = lastNonZeroSteer;
        }

        steerInput = Mathf.Lerp(
            steerInput,
            targetSteer,
            steeringSmoothSpeed * Time.fixedDeltaTime
        );
    }

    private void ReadArduinoThrottle()
    {
        rawY = Joystick.current.stick.y.ReadUnprocessedValue();

        throttleInput = 1f - Mathf.Clamp01(rawY);
        throttleInput = Mathf.Clamp01(throttleInput);
    }

    private void ReadArduinoBrake()
    {
        if (brakeAxis == null)
        {
            brakeInput = 0f;
            return;
        }

        rawZ = brakeAxis.ReadUnprocessedValue();

        if (invertBrake)
            rawZ *= -1f;

        brakeInput = Mathf.Clamp01((rawZ + 1f) / 2f);
    }

    private void ApplyThrottle()
    {
        if (throttleInput <= 0f)
            return;

        Vector3 horizontalVelocity = GetHorizontalVelocity();

        if (horizontalVelocity.magnitude >= maxSpeed)
            return;

        rb.AddForce(
            GetMoveDirection() * throttleInput * accelerationForce,
            ForceMode.Acceleration
        );
    }

    private void ApplyReverse()
    {
        if (reverseInput <= 0f)
            return;

        Vector3 horizontalVelocity = GetHorizontalVelocity();

        if (horizontalVelocity.magnitude >= maxSpeed * 0.5f)
            return;

        rb.AddForce(
            -GetMoveDirection() * reverseInput * reverseForce,
            ForceMode.Acceleration
        );
    }

    private void ApplySteering()
    {
        if (Mathf.Abs(steerInput) <= 0.0001f)
            return;

        float horizontalSpeed = GetHorizontalSpeed();

        if (horizontalSpeed < minSpeedToSteer && throttleInput <= 0f && reverseInput <= 0f)
            return;

        float speedFactor = Mathf.InverseLerp(0f, maxSpeed, horizontalSpeed);
        float moveFactor = Mathf.Max(throttleInput, reverseInput);
        float steeringPower = Mathf.Max(speedFactor, moveFactor);

        float directionMultiplier = reverseInput > throttleInput ? -1f : 1f;

        float turnAmount =
            steerInput *
            turnSpeed *
            steeringPower *
            directionMultiplier *
            Time.fixedDeltaTime;

        float currentY = rb.rotation.eulerAngles.y;
        float targetY = currentY + turnAmount;

        rb.MoveRotation(Quaternion.Euler(0f, targetY, 0f));
    }

    private void ApplyLateralGrip()
    {
        if (applyGripOnlyWhenGrounded && !isGrounded)
            return;

        Vector3 moveDirection = GetMoveDirection();
        Vector3 horizontalVelocity = GetHorizontalVelocity();

        float forwardSpeed = Vector3.Dot(horizontalVelocity, moveDirection);
        Vector3 forwardVelocity = moveDirection * forwardSpeed;

        Vector3 sidewaysVelocity = horizontalVelocity - forwardVelocity;

        if (sidewaysVelocity.magnitude > maxSidewaysSpeed)
        {
            sidewaysVelocity = sidewaysVelocity.normalized * maxSidewaysSpeed;
        }

        float gripAmount = Mathf.Clamp01(lateralGrip * Time.fixedDeltaTime);
        Vector3 correctedSidewaysVelocity = Vector3.Lerp(
            horizontalVelocity - forwardVelocity,
            sidewaysVelocity,
            gripAmount
        );

        correctedSidewaysVelocity = Vector3.Lerp(
            correctedSidewaysVelocity,
            Vector3.zero,
            gripAmount
        );

        Vector3 finalHorizontalVelocity = forwardVelocity + correctedSidewaysVelocity;

        rb.linearVelocity = new Vector3(
            finalHorizontalVelocity.x,
            rb.linearVelocity.y,
            finalHorizontalVelocity.z
        );
    }

    private void ApplyVelocityAlignment()
    {
        if (applyGripOnlyWhenGrounded && !isGrounded)
            return;

        Vector3 horizontalVelocity = GetHorizontalVelocity();

        if (horizontalVelocity.magnitude < 0.1f)
            return;

        Vector3 moveDirection = GetMoveDirection();
        float speed = horizontalVelocity.magnitude;

        Vector3 targetVelocity = moveDirection * speed;

        float alignAmount = Mathf.Clamp01(velocityAlignStrength * Time.fixedDeltaTime);

        Vector3 alignedVelocity = Vector3.Lerp(
            horizontalVelocity,
            targetVelocity,
            alignAmount
        );

        rb.linearVelocity = new Vector3(
            alignedVelocity.x,
            rb.linearVelocity.y,
            alignedVelocity.z
        );
    }

    private void ApplyBrake()
    {
        if (brakeInput <= 0f)
            return;

        Vector3 horizontalVelocity = GetHorizontalVelocity();

        horizontalVelocity = Vector3.Lerp(
            horizontalVelocity,
            Vector3.zero,
            brakeInput * brakePower * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(
            horizontalVelocity.x,
            rb.linearVelocity.y,
            horizontalVelocity.z
        );
    }

    private void ApplyGroundStabilization()
    {
        if (isGrounded)
        {
            if (rb.linearVelocity.y > maxGroundedUpwardVelocity)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    maxGroundedUpwardVelocity,
                    rb.linearVelocity.z
                );
            }

            rb.AddForce(Vector3.down * groundedDownForce, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(Vector3.down * airDownForce, ForceMode.Acceleration);
        }
    }

    private void LimitHorizontalSpeed()
    {
        Vector3 horizontalVelocity = GetHorizontalVelocity();

        if (horizontalVelocity.magnitude <= maxSpeed)
            return;

        Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;

        rb.linearVelocity = new Vector3(
            limitedVelocity.x,
            rb.linearVelocity.y,
            limitedVelocity.z
        );
    }

    private void StabilizeBodyRotation()
    {
        float currentY = rb.rotation.eulerAngles.y;

        rb.MoveRotation(Quaternion.Euler(0f, currentY, 0f));

        rb.angularVelocity = new Vector3(
            0f,
            rb.angularVelocity.y,
            0f
        );
    }

    private void UpdateVisualSteering()
    {
        float targetAngle = steerInput * visualSteerAngle;

        currentVisualSteerAngle = Mathf.Lerp(
            currentVisualSteerAngle,
            targetAngle,
            visualSteerSmooth * Time.deltaTime
        );

        if (handlebarVisual != null)
        {
            handlebarVisual.localRotation =
                handlebarBaseRotation *
                Quaternion.AngleAxis(
                    currentVisualSteerAngle,
                    handlebarSteerAxis.normalized
                );
        }

        if (frontWheelSteerRoot != null)
        {
            float frontWheelAngle = invertFrontWheelSteerVisual
                ? -currentVisualSteerAngle
                : currentVisualSteerAngle;

            frontWheelSteerRoot.localRotation =
                frontWheelSteerBaseRotation *
                Quaternion.AngleAxis(
                    frontWheelAngle,
                    frontWheelSteerAxis.normalized
                );
        }
    }

    private void UpdateWheelSpin()
    {
        float forwardSpeed = Vector3.Dot(
            GetHorizontalVelocity(),
            GetMoveDirection()
        );

        float circumference = 2f * Mathf.PI * wheelRadius;

        if (circumference <= 0.0001f)
            return;

        float degreePerSecond =
            (forwardSpeed / circumference) *
            360f *
            wheelSpinMultiplier;

        frontWheelSpinAngle -= degreePerSecond * Time.deltaTime;
        rearWheelSpinAngle -= degreePerSecond * Time.deltaTime;

        if (frontWheelSpinPivot != null)
        {
            frontWheelSpinPivot.localRotation =
                frontWheelSpinBaseRotation *
                Quaternion.AngleAxis(
                    frontWheelSpinAngle,
                    frontWheelSpinAxis.normalized
                );
        }

        if (rearWheelSpinPivot != null)
        {
            rearWheelSpinPivot.localRotation =
                rearWheelSpinBaseRotation *
                Quaternion.AngleAxis(
                    rearWheelSpinAngle,
                    rearWheelSpinAxis.normalized
                );
        }
    }

    private void ForceTargetLocalXRotation()
    {
        if (!forceLocalXRotation || forceMinus90Target == null)
            return;

        Vector3 euler = forceMinus90Target.localEulerAngles;

        forceMinus90Target.localEulerAngles = new Vector3(
            forcedLocalX,
            euler.y,
            euler.z
        );
    }

    private void UpdateGroundedState()
    {
        Vector3 rayStart = transform.position + Vector3.up * groundCheckStartOffset;

        isGrounded = Physics.Raycast(
            rayStart,
            Vector3.down,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private Vector3 GetMoveDirection()
    {
        Vector3 worldDirection = transform.TransformDirection(
            movementLocalAxis.normalized
        );

        worldDirection = Vector3.ProjectOnPlane(worldDirection, Vector3.up);

        if (worldDirection.sqrMagnitude < 0.0001f)
            worldDirection = transform.forward;

        return worldDirection.normalized;
    }

    private Vector3 GetHorizontalVelocity()
    {
        return new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );
    }

    private float GetHorizontalSpeed()
    {
        return GetHorizontalVelocity().magnitude;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 rayStart = transform.position + Vector3.up * groundCheckStartOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.blue;

        Vector3 moveDirection = Application.isPlaying
            ? GetMoveDirection()
            : transform.TransformDirection(movementLocalAxis.normalized);

        Gizmos.DrawLine(transform.position, transform.position + moveDirection.normalized * 2f);
    }
}