using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 5f; // Distancia de la cámara al jugador
    public float heightOffset = 1.5f; // Altura para apuntar a la cabeza/pecho, no a los pies
    public float mouseSensitivity = 3f; // Sensibilidad del ratón

    private float rotationY = 0f;
    private float rotationX = 0f;

    void Start()
    {
        // Bloquea y oculta el cursor del ratón en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Leer el movimiento del ratón
        rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 2. Limitar el movimiento vertical para que la cámara no dé vueltas completas de campana
        rotationX = Mathf.Clamp(rotationX, -40f, 60f);

        // 3. Calcular la nueva rotación
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        // 4. Calcular la posición deseada orbitando alrededor del jugador
        Vector3 targetPos = target.position + (Vector3.up * heightOffset);
        Vector3 position = targetPos - (rotation * Vector3.forward * distance);

        // 5. Aplicar posición y rotación
        transform.position = position;
        transform.rotation = rotation;
    }
}