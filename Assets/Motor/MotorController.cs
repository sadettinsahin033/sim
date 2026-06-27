using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ArduinoMotorController : MonoBehaviour
{
    private enum ArduinoAxisSource
    {
        StickX,
        StickY,
        Z
    }

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [Header("Force Fixed Local X Rotation")]
    [Tooltip("X'i -90 kalması gereken ana visual root objesi. Teker/gidon verme.")]
    [SerializeField] private Transform forceMinus90Target;
    [SerializeField] private bool forceLocalXRotation = true;
    [SerializeField] private float forcedLocalX = -90f;

    [Header("Input Mode")]
    [SerializeField] private bool useKeyboardInputForTest = true;

    [Header("Arduino Axis Mapping")]
    [Tooltip("Gaz Y ekseninden okunacak.")]
    [SerializeField] private ArduinoAxisSource throttleAxisSource = ArduinoAxisSource.StickY;

    [Tooltip("Sağ-sol direksiyon X ekseninden okunacak.")]
    [SerializeField] private ArduinoAxisSource steeringAxisSource = ArduinoAxisSource.StickX;

    [Tooltip("Fren genelde Z ekseni.")]
    [SerializeField] private ArduinoAxisSource brakeAxisSource = ArduinoAxisSource.Z;

    [Header("Throttle Y Start Calibration")]
    [Tooltip("Oyun başladığında Y ekseninin o anki değerini 0 gaz kabul eder.")]
    [SerializeField] private bool captureThrottleBaseOnStart = true;

    [Tooltip("Başlangıç Y değerinden ne kadar artınca tam gaz olsun.")]
    [SerializeField] private float throttleFullOffset = 0.6f;

    [Tooltip("Başlangıç noktasındaki küçük titreşimleri yok sayar.")]
    [SerializeField] private float throttleDeadZone = 0.02f;

    [Tooltip("Gaz ters çalışırsa aç.")]
    [SerializeField] private bool invertThrottleAxis = false;

    [SerializeField] private float throttleSmoothSpeed = 12f;

    [Header("Steering X Settings")]
    [Tooltip("Direksiyon merkezi. Joystick X ortası genelde 0'dır.")]
    [SerializeField] private float steeringCenter = 0f;

    [SerializeField] private float steeringMin = -1f;
    [SerializeField] private float steeringMax = 1f;

    [Tooltip("Direksiyon ortasındaki küçük titreşimi yok sayar.")]
    [SerializeField] private float steeringDeadZone = 0.02f;

    [SerializeField] private float steeringSmoothSpeed = 25f;
    [SerializeField] private bool invertSteer = false;

    [Header("Brake Z Start Calibration")]
    [Tooltip("Oyun başladığında Z ekseninin o anki değerini fren bırakılmış maksimum değer kabul eder.")]
    [SerializeField] private bool captureBrakeBaseOnStart = true;

    [Tooltip("Z bu değere yaklaşınca tam fren olur. Genelde 0.")]
    [SerializeField] private float brakeFullRaw = 0f;

    [Tooltip("Fren başlangıcındaki küçük titreşimi yok sayar.")]
    [SerializeField] private float brakeDeadZone = 0.02f;

    [Tooltip("Fren yumuşatma hızı.")]
    [SerializeField] private float brakeSmoothSpeed = 15f;

    [Tooltip("Fren ters çalışırsa aç.")]
    [SerializeField] private bool invertBrake = false;

    [Header("Movement Direction")]
    [Tooltip("Motor gücü hangi LOCAL eksene uygulansın? Z için (0,0,1).")]
    [SerializeField] private Vector3 movementLocalAxis = Vector3.forward;

    [Header("Movement Settings")]
    [SerializeField] private float accelerationForce = 9f;
    [SerializeField] private float reverseForce = 4f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float brakePower = 6f;

    [Header("Grip / Anti Slide Settings")]
    [SerializeField] private float lateralGrip = 12f;
    [SerializeField] private float velocityAlignStrength = 5f;
    [SerializeField] private float maxSidewaysSpeed = 0.4f;
    [SerializeField] private bool applyGripOnlyWhenGrounded = true;

    [Header("Ground / Stability")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckDistance = 0.7f;
    [SerializeField] private float groundCheckStartOffset = 0.2f;
    [SerializeField] private float groundedDownForce = 5f;
    [SerializeField] private float airDownForce = 20f;
    [SerializeField] private float maxGroundedUpwardVelocity = 0.1f;
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.6f, 0f);

    [Header("Steering Movement")]
    [SerializeField] private float turnSpeed = 42f;
    [SerializeField] private float minSpeedToSteer = 0.05f;

    [Header("Keyboard Test")]
    [SerializeField] private float keyboardSteerSmooth = 10f;
    [SerializeField] private float keyboardThrottleSmooth = 10f;

    [Header("Visual Steering")]
    [SerializeField] private Transform handlebarVisual;

    [Tooltip("Ön tekerin sağ-sol dönen parent objesi. Spin pivot ile aynı obje OLMAMALI.")]
    [SerializeField] private Transform frontWheelSteerRoot;

    [SerializeField] private float visualSteerAngle = 28f;
    [SerializeField] private float visualSteerSmooth = 30f;

    [Tooltip("Gidon hangi LOCAL eksende sağ-sol dönüyorsa onu yaz.")]
    [SerializeField] private Vector3 handlebarSteerAxis = Vector3.up;

    [Tooltip("Ön teker sağ-sol dönerken local X değişsin istiyorsan (1,0,0).")]
    [SerializeField] private Vector3 frontWheelSteerAxis = Vector3.right;

    [Tooltip("Sağa dönerken ön teker X eksiye gitsin diye açık kalsın.")]
    [SerializeField] private bool invertFrontWheelSteerVisual = true;

    [Header("Visual Rotation Corrections")]
    [SerializeField] private Vector3 handlebarLocalRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 frontWheelSteerLocalRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 frontWheelSpinLocalRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 rearWheelSpinLocalRotationOffset = Vector3.zero;

    [Header("Wheel Spin")]
    [Tooltip("Ön tekerin kendi merkezindeki spin pivotu. FrontWheelSteerRoot'un child'ı olmalı.")]
    [SerializeField] private Transform frontWheelSpinPivot;

    [SerializeField] private Transform rearWheelSpinPivot;
    [SerializeField] private float wheelRadius = 0.35f;
    [SerializeField] private float wheelSpinMultiplier = 1f;

    [Tooltip("Teker ileri giderken local X ekseninde dönsün istiyorsan (1,0,0).")]
    [SerializeField] private Vector3 frontWheelSpinAxis = Vector3.right;

    [Tooltip("Teker ileri giderken local X ekseninde dönsün istiyorsan (1,0,0).")]
    [SerializeField] private Vector3 rearWheelSpinAxis = Vector3.right;

    private float steerInput;
    private float throttleInput;
    private float reverseInput;
    private float brakeInput;

    private float rawX;
    private float rawY;
    private float rawZ;

    private float throttleBaseRaw;
    private bool throttleBaseCaptured;

    private float brakeReleasedRaw;
    private bool brakeBaseCaptured;

    private AxisControl zAxis;

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
                    "Hiyerarşi şu olmalı: FrontWheelSteerRoot > FrontWheelSpinPivot > FrontWheelMesh"
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

        zAxis = Joystick.current.TryGetChildControl<AxisControl>("z");
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

        rawX = targetSteer;
        rawY = targetThrottle;
        rawZ = targetBrake;
    }

    private void ReadArduinoInput()
    {
        ReadArduinoRawAxes();

        ReadArduinoThrottleFromY();
        ReadArduinoSteeringFromX();
        ReadArduinoBrakeFromZ();

        reverseInput = 0f;
    }

    private void ReadArduinoRawAxes()
    {
        rawX = 0f;
        rawY = 0f;
        rawZ = 0f;

        if (Joystick.current == null)
            return;

        if (Joystick.current.stick != null)
        {
            rawX = Joystick.current.stick.x.ReadUnprocessedValue();
            rawY = Joystick.current.stick.y.ReadUnprocessedValue();
        }

        if (zAxis != null)
            rawZ = zAxis.ReadUnprocessedValue();
    }

    private bool TryGetArduinoAxisValue(ArduinoAxisSource source, out float value)
    {
        value = 0f;

        switch (source)
        {
            case ArduinoAxisSource.StickX:
                value = rawX;
                return true;

            case ArduinoAxisSource.StickY:
                value = rawY;
                return true;

            case ArduinoAxisSource.Z:
                if (zAxis == null)
                    return false;

                value = rawZ;
                return true;
        }

        return false;
    }

    private void ReadArduinoThrottleFromY()
    {
        if (!TryGetArduinoAxisValue(throttleAxisSource, out float rawThrottle))
        {
            throttleInput = 0f;
            return;
        }

        if (invertThrottleAxis)
            rawThrottle *= -1f;

        if (captureThrottleBaseOnStart && !throttleBaseCaptured)
        {
            throttleBaseRaw = rawThrottle;
            throttleBaseCaptured = true;
            throttleInput = 0f;

            Debug.Log("Gaz Y başlangıç değeri 0 kabul edildi: " + throttleBaseRaw);
            return;
        }

        float deltaFromStart = rawThrottle - throttleBaseRaw;

        float targetThrottle = 0f;

        if (deltaFromStart > throttleDeadZone)
        {
            targetThrottle = Mathf.InverseLerp(
                throttleDeadZone,
                throttleFullOffset,
                deltaFromStart
            );

            targetThrottle = Mathf.Clamp01(targetThrottle);
        }

        throttleInput = Mathf.Lerp(
            throttleInput,
            targetThrottle,
            throttleSmoothSpeed * Time.fixedDeltaTime
        );
    }

    private void ReadArduinoSteeringFromX()
    {
        if (!TryGetArduinoAxisValue(steeringAxisSource, out float rawSteer))
        {
            steerInput = 0f;
            return;
        }

        float valueFromCenter = rawSteer - steeringCenter;

        float targetSteer = 0f;

        if (Mathf.Abs(valueFromCenter) > steeringDeadZone)
        {
            if (valueFromCenter < 0f)
            {
                float leftRange = Mathf.Abs(steeringCenter - steeringMin);

                if (leftRange < 0.0001f)
                    leftRange = 0.0001f;

                targetSteer = valueFromCenter / leftRange;
            }
            else
            {
                float rightRange = Mathf.Abs(steeringMax - steeringCenter);

                if (rightRange < 0.0001f)
                    rightRange = 0.0001f;

                targetSteer = valueFromCenter / rightRange;
            }
        }

        targetSteer = Mathf.Clamp(targetSteer, -1f, 1f);

        if (invertSteer)
            targetSteer *= -1f;

        steerInput = Mathf.Lerp(
            steerInput,
            targetSteer,
            steeringSmoothSpeed * Time.fixedDeltaTime
        );
    }

    private void ReadArduinoBrakeFromZ()
    {
        if (!TryGetArduinoAxisValue(brakeAxisSource, out float rawBrake))
        {
            brakeInput = Mathf.Lerp(
                brakeInput,
                0f,
                brakeSmoothSpeed * Time.fixedDeltaTime
            );

            return;
        }

        if (invertBrake)
            rawBrake *= -1f;

        if (captureBrakeBaseOnStart && !brakeBaseCaptured)
        {
            brakeReleasedRaw = rawBrake;
            brakeBaseCaptured = true;
            brakeInput = 0f;

            Debug.Log("Fren Z başlangıç değeri maksimum / bırakılmış kabul edildi: " + brakeReleasedRaw);
            return;
        }

        // İstenen fren mantığı:
        // Oyun başındaki Z değeri => fren 0
        // Z bu değerden aşağı indikçe => fren artar
        // Z brakeFullRaw değerine yaklaşınca => tam fren
        float normalizedBrake = Mathf.InverseLerp(
            brakeReleasedRaw,
            brakeFullRaw,
            rawBrake
        );

        normalizedBrake = Mathf.Clamp01(normalizedBrake);

        float targetBrake = 0f;

        if (normalizedBrake > brakeDeadZone)
        {
            targetBrake = Mathf.InverseLerp(
                brakeDeadZone,
                1f,
                normalizedBrake
            );

            targetBrake = Mathf.Clamp01(targetBrake);
        }

        brakeInput = Mathf.Lerp(
            brakeInput,
            targetBrake,
            brakeSmoothSpeed * Time.fixedDeltaTime
        );
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

        float steeringPower = Mathf.Lerp(
            moveFactor * 0.45f,
            speedFactor,
            0.65f
        );

        steeringPower = Mathf.Clamp01(steeringPower);

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

        Vector3 currentSidewaysVelocity = horizontalVelocity - forwardVelocity;

        if (currentSidewaysVelocity.magnitude > maxSidewaysSpeed)
        {
            currentSidewaysVelocity = currentSidewaysVelocity.normalized * maxSidewaysSpeed;
        }

        float gripAmount = Mathf.Clamp01(lateralGrip * Time.fixedDeltaTime);

        Vector3 correctedSidewaysVelocity = Vector3.Lerp(
            currentSidewaysVelocity,
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

    public void RecalibrateThrottleBase()
    {
        if (Joystick.current == null)
            return;

        ReadArduinoRawAxes();

        if (!TryGetArduinoAxisValue(throttleAxisSource, out float rawThrottle))
            return;

        if (invertThrottleAxis)
            rawThrottle *= -1f;

        throttleBaseRaw = rawThrottle;
        throttleBaseCaptured = true;
        throttleInput = 0f;

        Debug.Log("Gaz Y başlangıç değeri yeniden 0 kabul edildi: " + throttleBaseRaw);
    }

    public void RecalibrateBrakeBase()
    {
        if (Joystick.current == null)
            return;

        ReadArduinoRawAxes();

        if (!TryGetArduinoAxisValue(brakeAxisSource, out float rawBrake))
            return;

        if (invertBrake)
            rawBrake *= -1f;

        brakeReleasedRaw = rawBrake;
        brakeBaseCaptured = true;
        brakeInput = 0f;

        Debug.Log("Fren Z başlangıç değeri yeniden maksimum / bırakılmış kabul edildi: " + brakeReleasedRaw);
    }

    public float SteerInput => steerInput;
    public float ThrottleInput => throttleInput;
    public float ReverseInput => reverseInput;
    public float BrakeInput => brakeInput;

    public float RawX => rawX;
    public float RawY => rawY;
    public float RawZ => rawZ;

    public float ThrottleBaseRaw => throttleBaseRaw;
    public float BrakeReleasedRaw => brakeReleasedRaw;

    public float ThrottleDeltaFromStart
    {
        get
        {
            if (!TryGetArduinoAxisValue(throttleAxisSource, out float value))
                return 0f;

            if (invertThrottleAxis)
                value *= -1f;

            return value - throttleBaseRaw;
        }
    }

    public float BrakeDeltaFromReleased
    {
        get
        {
            if (!TryGetArduinoAxisValue(brakeAxisSource, out float value))
                return 0f;

            if (invertBrake)
                value *= -1f;

            return brakeReleasedRaw - value;
        }
    }

    public float BrakeNormalized => brakeInput;

    public float VisualSteerAngle => currentVisualSteerAngle;
    public bool IsGrounded => isGrounded;

    public Vector3 CurrentHorizontalVelocity => GetHorizontalVelocity();
    public Vector3 CurrentMoveDirection => GetMoveDirection();
    public float CurrentSpeed => GetHorizontalSpeed();

    public float CurrentSidewaysSpeed
    {
        get
        {
            Vector3 horizontalVelocity = GetHorizontalVelocity();
            Vector3 moveDirection = GetMoveDirection();

            float forwardSpeed = Vector3.Dot(horizontalVelocity, moveDirection);
            Vector3 forwardVelocity = moveDirection * forwardSpeed;
            Vector3 sidewaysVelocity = horizontalVelocity - forwardVelocity;

            return sidewaysVelocity.magnitude;
        }
    }
}