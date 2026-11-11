using UnityEngine;

public class AutoLoop : MonoBehaviour
{
    [Header("Configuración de Velocidad")]
    public float velocidad = 15f;

    [Header("Puntos de Control")]
    public Transform puntoInicio;
    public Transform puntoFin;

    void Update()
    {
        if (puntoInicio == null || puntoFin == null) return;

        // Mueve el auto hacia el punto final a una velocidad constante
        transform.position = Vector3.MoveTowards(transform.position, puntoFin.position, velocidad * Time.deltaTime);

        // Hace que el auto mire hacia la dirección donde avanza
        Vector3 direccion = (puntoFin.position - transform.position).normalized;
        if (direccion != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccion);
        }

        // Si el auto llega muy cerca del punto final, se teletransporta de nuevo al inicio
        if (Vector3.Distance(transform.position, puntoFin.position) < 0.5f)
        {
            transform.position = puntoInicio.position;
        }
    }
}