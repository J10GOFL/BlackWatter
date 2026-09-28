using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam; 
    public Animator anim; // El "cable" para conectar el cerebro
    
    public float walkSpeed = 5f; 
    public float runSpeed = 10f; 
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float turnSmoothTime = 0.1f; 

    private float turnSmoothVelocity;
    private Vector3 velocity;
    private bool isGrounded;

    void Update()
    {
        // 1. Detección de suelo
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Le avisamos al cerebro (Animator) si estamos tocando el piso
        anim.SetBool("IsGrounded", isGrounded);

        // 2. Leer las teclas
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        float currentSpeed = 0f; // Empezamos en 0 (Descanso)

        // 3. Movimiento Horizontal
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            currentSpeed = walkSpeed; // Cambia a 5 (Caminar)
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = runSpeed; // Cambia a 10 (Correr)
            }

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        // Le pasamos la velocidad actual al cerebro para que mueva las piernas
        anim.SetFloat("Speed", currentSpeed);

        // 4. Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5. Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}