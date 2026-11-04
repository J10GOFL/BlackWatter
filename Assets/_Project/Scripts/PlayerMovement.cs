using UnityEngine;

/// <summary>
/// Movimiento del jugador relativo a la cámara, salto, gravedad, apuntado
/// isométrico independiente y parámetros del Animator.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public CharacterController controller;
    public Transform cam;
    public Animator anim;
    public CameraFollow cameraFollow;

    [Header("Velocidad")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float acceleration = 25f;
    public float deceleration = 35f;
    [Range(0f, 1f)] public float airControl = 0.35f;

    [Header("Giro")]
    public float turnSpeed = 720f;

    [Header("Apuntado Isométrico")]
    [Tooltip("Botón derecho del mouse por defecto (1). Mientras se mantiene, el personaje mira al cursor aunque se mueva.")]
    public int aimMouseButton = 1;
    [Tooltip("Capas que puede alcanzar la mira. Incluye el suelo y los objetos apuntables.")]
    public LayerMask aimLayers = Physics.DefaultRaycastLayers;
    [Min(1f)] public float aimRayDistance = 500f;

    [Header("Salto y Gravedad")]
    public float gravity = -20f;
    public float jumpHeight = 1.5f;
    [Min(0f)] public float retrasoImpulsoSalto = 0.05f;

    private Camera playerCamera;
    private Vector3 planarVelocity;
    private float verticalVelocity;
    private bool isGrounded;
    private bool saltoPendiente;
    private bool saltoEnCurso;
    private bool saltoDespegado;
    private float tiempoImpulsoSalto;
    private float tiempoLimiteDespegueSalto;
    private const float tiempoMaximoDespegueSalto = 0.3f;
    private readonly RaycastHit[] aimHits = new RaycastHit[32];

    /// <summary>Indica si el jugador mantiene el botón de apuntado en isométrica.</summary>
    public bool IsAiming { get; private set; }

    /// <summary>Última posición del mundo alcanzada por la mira isométrica.</summary>
    public Vector3 AimPoint { get; private set; }

    void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (cam != null)
            playerCamera = cam.GetComponent<Camera>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
        {
            if (cam == null)
                cam = playerCamera.transform;

            if (cameraFollow == null)
                cameraFollow = playerCamera.GetComponent<CameraFollow>();
        }
    }

    void Update()
    {
        if (controller == null)
            return;

        UpdateGroundedState();

        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        input = Vector2.ClampMagnitude(input, 1f);

        bool isIsometric =
            cameraFollow != null && cameraFollow.IsometricMode;

        Vector3 mousePoint = Vector3.zero;
        bool hasAimPoint =
            isIsometric && TryGetMouseGroundPoint(out mousePoint);

        if (!hasAimPoint)
            mousePoint = Vector3.zero;

        IsAiming = isIsometric &&
                   hasAimPoint &&
                   Input.GetMouseButton(aimMouseButton);
        AimPoint = hasAimPoint ? mousePoint : Vector3.zero;

        // El movimiento siempre usa la orientación de cámara, nunca la mira.
        Vector3 cameraForward = GetCameraForward();
        Vector3 cameraRight = Vector3.Cross(
            Vector3.up,
            cameraForward
        ).normalized;

        Vector3 moveDirection =
            cameraForward * input.y +
            cameraRight * input.x;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        bool hasMovementInput = input.sqrMagnitude > 0.01f;
        float targetSpeed = Input.GetKey(KeyCode.LeftShift)
            ? runSpeed
            : walkSpeed;

        Vector3 desiredPlanarVelocity = hasMovementInput
            ? moveDirection * targetSpeed
            : Vector3.zero;

        UpdatePlanarVelocity(
            desiredPlanarVelocity,
            hasMovementInput
        );

        UpdateFacing(
            isIsometric,
            hasAimPoint,
            mousePoint,
            hasMovementInput,
            moveDirection
        );

        HandleJumpAndGravity();

        Vector3 finalVelocity =
            planarVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);

        UpdateAnimator();
    }

    /// <summary>Lee el estado del CharacterController y actualiza el Animator.</summary>
    void UpdateGroundedState()
    {
        bool contactoSuelo = controller.isGrounded;

        if (saltoEnCurso)
        {
            if (!contactoSuelo)
                saltoDespegado = true;
            else if (saltoDespegado || Time.time >= tiempoLimiteDespegueSalto)
            {
                saltoEnCurso = false;
                saltoDespegado = false;
            }
        }

        isGrounded = contactoSuelo;

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (anim != null)
        {
            bool sueloParaAnimacion = isGrounded && !saltoPendiente && !saltoEnCurso;
            anim.SetBool("IsGrounded", sueloParaAnimacion);
        }
    }

    /// <summary>Gira hacia WASD o hacia el cursor según el estado de apuntado.</summary>
    void UpdateFacing(
        bool isIsometric,
        bool hasAimPoint,
        Vector3 mousePoint,
        bool hasMovementInput,
        Vector3 moveDirection
    )
    {
        if (isIsometric && hasAimPoint && (IsAiming || !hasMovementInput))
        {
            Vector3 aimDirection = mousePoint - transform.position;
            aimDirection.y = 0f;

            if (aimDirection.sqrMagnitude > 0.01f)
                RotateTowards(aimDirection);

            return;
        }

        if (hasMovementInput)
            RotateTowards(moveDirection);
    }

    /// <summary>Obtiene el frente horizontal de la cámara para calcular WASD.</summary>
    Vector3 GetCameraForward()
    {
        if (cam == null)
            return transform.forward;

        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);

        if (forward.sqrMagnitude < 0.001f)
            return transform.forward;

        return forward.normalized;
    }

    /// <summary>Acelera hacia la velocidad deseada o desacelera al soltar WASD.</summary>
    void UpdatePlanarVelocity(
        Vector3 desiredVelocity,
        bool hasMovementInput
    )
    {
        float control = isGrounded ? 1f : airControl;
        float rate = hasMovementInput ? acceleration : deceleration;

        planarVelocity = Vector3.MoveTowards(
            planarVelocity,
            desiredVelocity,
            rate * control * Time.deltaTime
        );
    }

    /// <summary>
    /// Busca primero una superficie con collider. Si no la encuentra, usa un
    /// plano horizontal como respaldo para que la mira siga funcionando.
    /// </summary>
    bool TryGetMouseGroundPoint(out Vector3 point)
    {
        point = Vector3.zero;

        if (playerCamera == null)
            return false;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        int hitCount = Physics.RaycastNonAlloc(
            ray,
            aimHits,
            aimRayDistance,
            aimLayers,
            QueryTriggerInteraction.Ignore
        );

        float nearestDistance = float.PositiveInfinity;
        int nearestHitIndex = -1;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = aimHits[i].collider;

            if (hitCollider == null || IsPlayerCollider(hitCollider.transform))
                continue;

            if (aimHits[i].distance < nearestDistance)
            {
                nearestDistance = aimHits[i].distance;
                nearestHitIndex = i;
            }
        }

        if (nearestHitIndex >= 0)
        {
            point = aimHits[nearestHitIndex].point;
            return true;
        }

        Plane fallbackPlane = new Plane(
            Vector3.up,
            new Vector3(0f, transform.position.y, 0f)
        );

        if (!fallbackPlane.Raycast(ray, out float planeDistance))
            return false;

        point = ray.GetPoint(planeDistance);
        return true;
    }

    /// <summary>Evita que la mira se quede con el collider del propio jugador.</summary>
    bool IsPlayerCollider(Transform hitTransform)
    {
        return hitTransform == transform ||
               hitTransform.IsChildOf(transform) ||
               transform.IsChildOf(hitTransform);
    }

    /// <summary>Gira el modelo horizontalmente hacia una dirección de mundo.</summary>
    void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    /// <summary>Aplica salto al pulsar Espacio y gravedad en cada frame.</summary>
    void HandleJumpAndGravity()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !saltoPendiente && !saltoEnCurso)
        {
            saltoPendiente = true;
            tiempoImpulsoSalto = Time.time + retrasoImpulsoSalto;

            // Inicia la animación de salto mientras el personaje sigue en el suelo.
            if (anim != null)
                anim.SetBool("IsGrounded", false);
        }

        if (saltoPendiente && Time.time >= tiempoImpulsoSalto)
        {
            saltoPendiente = false;

            if (isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                saltoEnCurso = true;
                saltoDespegado = false;
                tiempoLimiteDespegueSalto = Time.time + tiempoMaximoDespegueSalto;
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    /// <summary>Envía la velocidad horizontal y el contacto con suelo al Animator.</summary>
    void UpdateAnimator()
    {
        if (anim == null)
            return;

        anim.SetFloat("Speed", planarVelocity.magnitude);
    }
}