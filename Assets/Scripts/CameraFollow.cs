using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Tercera Persona")]
    public float distance = 5f;
    public float heightOffset = 1.5f;
    public float mouseSensitivity = 3f;

    [Header("Vista Isométrica")]
    public float isometricHeight = 10f;
    public float isometricDistance = 10f;
    public float isometricAngle = 45f;

    [Header("Vista Cenital")]
    public float topDownHeight = 12f;

    [Header("Cambio de Cámara")]
    public KeyCode switchKey = KeyCode.V;

    // 0 = Tercera persona
    // 1 = Isométrica
    // 2 = Cenital
    private int cameraMode = 0;

    private float rotationY = 0f;
    private float rotationX = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            cameraMode++;

            // Volver al modo 0 después del modo 2
            if (cameraMode > 2)
            {
                cameraMode = 0;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        switch (cameraMode)
        {
            case 0:
                ThirdPersonCamera();
                break;

            case 1:
                IsometricCamera();
                break;

            case 2:
                TopDownCamera();
                break;
        }
    }

    // =========================================
    // TERCERA PERSONA
    // =========================================

    void ThirdPersonCamera()
    {
        rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX = Mathf.Clamp(rotationX, -40f, 60f);

        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        Vector3 targetPos = target.position + Vector3.up * heightOffset;

        Vector3 position =
            targetPos - (rotation * Vector3.forward * distance);

        transform.position = position;
        transform.rotation = rotation;
    }

    // =========================================
    // VISTA ISOMÉTRICA
    // =========================================

    void IsometricCamera()
    {
        // Rotación isométrica
        Quaternion rotation = Quaternion.Euler(
            isometricAngle,
            rotationY,
            0f
        );

        Vector3 targetPos = target.position;

        Vector3 position =
            targetPos - (rotation * Vector3.forward * isometricDistance);

        transform.position = position;

        // La cámara mira hacia el jugador
        transform.LookAt(target.position);
    }

    // =========================================
    // VISTA CENITAL
    // =========================================

    void TopDownCamera()
    {
        Vector3 position =
            target.position + Vector3.up * topDownHeight;

        transform.position = position;

        // Mirar directamente hacia abajo
        transform.rotation = Quaternion.Euler(
            90f,
            rotationY,
            0f
        );
    }
}