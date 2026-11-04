using UnityEngine;

/// <summary>
/// Gestiona la cámara en tercera persona e isométrica, incluyendo colisiones
/// con el escenario, zoom y rotación isométricos y un cursor visible.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Tercera Persona")]
    [Min(0f)] public float distance = 5f;
    public float heightOffset = 1.5f;
    [Min(0f)] public float mouseSensitivity = 3f;
    public float minPitch = -30f;
    public float maxPitch = 65f;
    [Min(0.1f)] public float minThirdPersonDistance = 2.5f;
    [Min(0.1f)] public float maxThirdPersonDistance = 8f;
    [Min(0f)] public float thirdPersonZoomSpeed = 2f;

    [Header("Respuesta al Movimiento")]
    [Min(0f)] public float movementLookAheadDistance = 0.15f;
    [Min(0.01f)] public float movementLookAheadSmoothTime = 0.2f;
    [Tooltip("Balanceo muy leve al correr. No se aplica durante el salto.")]
    [Min(0f)] public float runSwayAmount = 0.015f;
    [Min(0f)] public float runSwayFrequency = 7f;

    [Header("Vista Isométrica")]
    public float isometricHeight = 10f;
    [Min(0f)] public float isometricDistance = 10f;
    public float isometricYaw = 45f;
    [Min(0f)] public float isometricZoomSpeed = 5f;
    [Min(0.1f)] public float minIsometricDistance = 5f;
    [Min(0.1f)] public float maxIsometricDistance = 18f;
    [Min(1f)] public float isometricRotationStep = 45f;
    public KeyCode rotateIsometricLeftKey = KeyCode.Q;
    public KeyCode rotateIsometricRightKey = KeyCode.E;

    [Header("Cursor Isométrico")]
    [Tooltip("Textura opcional. Si queda vacía, se genera un cursor de mira visible al iniciar.")]
    public Texture2D isometricCursor;
    public Vector2 cursorHotspot = new Vector2(12f, 12f);

    [Header("Colisión de Cámara")]
    [Tooltip("Capas que pueden bloquear la cámara. Se ignora la jerarquía del objetivo.")]
    public LayerMask collisionLayers = Physics.DefaultRaycastLayers;
    [Min(0.01f)] public float collisionRadius = 0.2f;
    [Min(0f)] public float collisionPadding = 0.08f;

    [Header("Suavizado de Cámara")]
    [Min(0f)] public float positionSmoothTime = 0.08f;
    [Min(0f)] public float rotationSharpness = 15f;
    [Min(0f)] public float collisionRecoverySmoothTime = 0.22f;

    [Header("Cambio de Cámara")]
    [Tooltip("Alterna entre tercera persona e isométrica.")]
    public KeyCode switchKey = KeyCode.V;

    // 0 = tercera persona; 1 = isométrica.
    private int cameraMode;
    private float thirdPersonYaw;
    private float thirdPersonPitch;
    private Vector3 positionVelocity;
    private Vector3 targetMovement;
    private Vector3 movementLookAhead;
    private Vector3 movementLookAheadVelocity;
    private Vector3 previousTargetPosition;
    private bool hasPreviousTargetPosition;
    private bool recoveringFromCollision;
    private float runSwayTime;
    private CharacterController targetController;
    private Texture2D generatedCursor;
    private readonly RaycastHit[] collisionHits = new RaycastHit[24];

    /// <summary>Indica si está activa la vista isométrica.</summary>
    public bool IsometricMode => cameraMode == 1;

    void Awake()
    {
        if (isometricCursor == null)
        {
            generatedCursor = CreateDefaultIsometricCursor();
            isometricCursor = generatedCursor;
            cursorHotspot = new Vector2(12f, 12f);
        }
    }

    void Start()
    {
        if (target != null)
        {
            targetController = target.GetComponentInParent<CharacterController>();
            if (targetController == null)
                targetController = target.GetComponentInChildren<CharacterController>();

            previousTargetPosition = target.position;
            hasPreviousTargetPosition = true;
        }

        thirdPersonYaw = transform.eulerAngles.y;
        thirdPersonPitch = transform.eulerAngles.x;

        // Mantiene la orientación inicial de tercera persona en el rango firmado.
        if (thirdPersonPitch > 180f)
            thirdPersonPitch -= 360f;

        UpdateCursorMode();
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            cameraMode = 1 - cameraMode;
            positionVelocity = Vector3.zero;
            UpdateCursorMode();
        }

        if (cameraMode == 0)
        {
            // Controles originales de tercera persona: mouse para orbitar.
            thirdPersonYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            thirdPersonPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            thirdPersonPitch = Mathf.Clamp(
                thirdPersonPitch,
                minPitch,
                maxPitch
            );

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                distance = Mathf.Clamp(
                    distance - scroll * thirdPersonZoomSpeed,
                    minThirdPersonDistance,
                    maxThirdPersonDistance
                );
            }
        }
        else
        {
            UpdateIsometricCameraInput();
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        ActualizarMovimientoObjetivo();

        if (cameraMode == 0)
            UpdateThirdPersonCamera();
        else
            UpdateIsometricCamera();
    }

    void UpdateIsometricCameraInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.001f)
        {
            isometricDistance = Mathf.Clamp(
                isometricDistance - scroll * isometricZoomSpeed,
                minIsometricDistance,
                maxIsometricDistance
            );
        }

        if (Input.GetKeyDown(rotateIsometricLeftKey))
            isometricYaw -= isometricRotationStep;

        if (Input.GetKeyDown(rotateIsometricRightKey))
            isometricYaw += isometricRotationStep;
    }

    /// <summary>
    /// Bloquea y oculta el cursor en tercera persona. En isométrica muestra
    /// la textura asignada o el punto de mira generado automáticamente.
    /// </summary>
    void UpdateCursorMode()
    {
        bool isThirdPerson = cameraMode == 0;

        Cursor.lockState = isThirdPerson
            ? CursorLockMode.Locked
            : CursorLockMode.None;
        Cursor.visible = !isThirdPerson;

        if (isThirdPerson)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        else if (isometricCursor != null)
        {
            Cursor.SetCursor(
                isometricCursor,
                cursorHotspot,
                CursorMode.Auto
            );
        }
    }

    /// <summary>Cámara original de tercera persona, con la colisión ya añadida.</summary>
    void UpdateThirdPersonCamera()
    {
        Quaternion rotation = Quaternion.Euler(
            thirdPersonPitch,
            thirdPersonYaw,
            0f
        );

        Vector3 lookAheadTarget = Vector3.zero;
        if (targetMovement.sqrMagnitude > 0.04f)
            lookAheadTarget = targetMovement.normalized * movementLookAheadDistance;

        movementLookAhead = Vector3.SmoothDamp(
            movementLookAhead,
            lookAheadTarget,
            ref movementLookAheadVelocity,
            movementLookAheadSmoothTime
        );

        Vector3 focus = target.position + Vector3.up * heightOffset + movementLookAhead;
        Vector3 desiredPosition =
            focus - rotation * Vector3.forward * distance;

        bool estaEnSuelo = targetController == null || targetController.isGrounded;
        bool estaCorriendo = estaEnSuelo &&
                             Input.GetKey(KeyCode.LeftShift) &&
                             targetMovement.sqrMagnitude > 1f;

        if (estaCorriendo && runSwayAmount > 0f)
        {
            runSwayTime += Time.deltaTime * runSwayFrequency;
            float balanceoVertical = Mathf.Sin(runSwayTime * 2f) * runSwayAmount;
            float balanceoLateral = Mathf.Sin(runSwayTime) * runSwayAmount * 0.5f;
            desiredPosition += Vector3.up * balanceoVertical +
                               rotation * Vector3.right * balanceoLateral;
        }

        MoveCamera(focus, desiredPosition, rotation);
    }

    /// <summary>Calcula el desplazamiento horizontal del objetivo para la cámara.</summary>
    void ActualizarMovimientoObjetivo()
    {
        if (!hasPreviousTargetPosition)
        {
            previousTargetPosition = target.position;
            hasPreviousTargetPosition = true;
            targetMovement = Vector3.zero;
            return;
        }

        Vector3 desplazamiento = target.position - previousTargetPosition;
        desplazamiento.y = 0f;
        targetMovement = Time.deltaTime > 0f
            ? desplazamiento / Time.deltaTime
            : Vector3.zero;
        previousTargetPosition = target.position;
    }

    /// <summary>Coloca la cámara isométrica con orientación independiente del mouse.</summary>
    void UpdateIsometricCamera()
    {
        Vector3 focus = target.position + Vector3.up * heightOffset;
        Quaternion yawRotation = Quaternion.Euler(
            0f,
            isometricYaw,
            0f
        );

        Vector3 horizontalOffset =
            yawRotation * Vector3.back * isometricDistance;
        Vector3 desiredPosition =
            focus + horizontalOffset + Vector3.up * isometricHeight;

        Quaternion desiredRotation = Quaternion.LookRotation(
            focus - desiredPosition,
            Vector3.up
        );

        MoveCamera(focus, desiredPosition, desiredRotation);
    }

    /// <summary>
    /// Resuelve colisiones y suaviza el seguimiento. La cámara se retrae de
    /// inmediato al detectar una pared para evitar atravesarla.
    /// </summary>
    void MoveCamera(
        Vector3 focus,
        Vector3 desiredPosition,
        Quaternion desiredRotation
    )
    {
        bool obstructed = TryResolveCameraCollision(
            focus,
            desiredPosition,
            out Vector3 safePosition
        );

        if (obstructed)
        {
            transform.position = safePosition;
            positionVelocity = Vector3.zero;
            recoveringFromCollision = true;
        }
        else if (positionSmoothTime <= 0f && !recoveringFromCollision)
        {
            transform.position = desiredPosition;
        }
        else
        {
            float tiempoSuavizado = recoveringFromCollision
                ? collisionRecoverySmoothTime
                : positionSmoothTime;

            if (tiempoSuavizado <= 0f)
            {
                transform.position = desiredPosition;
                recoveringFromCollision = false;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref positionVelocity,
                    tiempoSuavizado
                );

                if (recoveringFromCollision &&
                    (transform.position - desiredPosition).sqrMagnitude < 0.0025f)
                {
                    recoveringFromCollision = false;
                }
            }
        }

        float rotationStep =
            1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationStep
        );
    }

    /// <summary>
    /// Barre una esfera desde el punto de enfoque hasta la cámara y acerca
    /// la cámara al obstáculo más cercano que pertenezca a las capas elegidas.
    /// </summary>
    bool TryResolveCameraCollision(
        Vector3 focus,
        Vector3 desiredPosition,
        out Vector3 safePosition
    )
    {
        safePosition = desiredPosition;

        Vector3 offset = desiredPosition - focus;
        float desiredDistance = offset.magnitude;

        if (desiredDistance <= 0.001f)
            return false;

        Vector3 direction = offset / desiredDistance;
        int hitCount = Physics.SphereCastNonAlloc(
            focus,
            collisionRadius,
            direction,
            collisionHits,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore
        );

        float nearestHitDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = collisionHits[i].collider;

            if (hitCollider == null || IsTargetCollider(hitCollider.transform))
                continue;

            if (collisionHits[i].distance < nearestHitDistance)
                nearestHitDistance = collisionHits[i].distance;
        }

        if (float.IsPositiveInfinity(nearestHitDistance))
            return false;

        float safeDistance = Mathf.Clamp(
            nearestHitDistance - collisionPadding,
            0f,
            desiredDistance
        );

        safePosition = focus + direction * safeDistance;
        return true;
    }

    /// <summary>Determina si un collider pertenece a la jerarquía del objetivo.</summary>
    bool IsTargetCollider(Transform hitTransform)
    {
        return hitTransform == target ||
               hitTransform.IsChildOf(target) ||
               target.IsChildOf(hitTransform);
    }

    /// <summary>Crea una mira de alto contraste para usar sin importar un PNG.</summary>
    Texture2D CreateDefaultIsometricCursor()
    {
        const int size = 24;
        const float center = (size - 1) * 0.5f;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        );
        texture.name = "BlackWater_IsometricCursor";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color outline = Color.black;
        Color white = Color.white;
        Color centerColor = new Color(1f, 0.78f, 0.12f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                bool crossOuter =
                    ((Mathf.Abs(dx) <= 1.5f && Mathf.Abs(dy) >= 7f && Mathf.Abs(dy) <= 11f) ||
                     (Mathf.Abs(dy) <= 1.5f && Mathf.Abs(dx) >= 7f && Mathf.Abs(dx) <= 11f));
                bool crossInner =
                    ((Mathf.Abs(dx) <= 0.65f && Mathf.Abs(dy) >= 7f && Mathf.Abs(dy) <= 10f) ||
                     (Mathf.Abs(dy) <= 0.65f && Mathf.Abs(dx) >= 7f && Mathf.Abs(dx) <= 10f));

                Color pixel = clear;

                if (crossOuter || (radius >= 5.5f && radius <= 7.5f))
                    pixel = outline;

                if (crossInner || radius <= 5.5f)
                    pixel = white;

                if (radius <= 2.5f)
                    pixel = centerColor;

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply(false, false);
        return texture;
    }

    /// <summary>Dibuja el brazo de seguimiento y la esfera de colisión en Scene.</summary>
    void OnDrawGizmosSelected()
    {
        if (target == null)
            return;

        Vector3 focus = target.position + Vector3.up * heightOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(focus, transform.position);
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDestroy()
    {
        if (generatedCursor != null)
            Destroy(generatedCursor);
    }
}
