using UnityEngine;

public class SeguirNiebla : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private Transform jugador;

    [Header("Ajustes")]
    [SerializeField] private Vector3 desplazamiento = new Vector3(0f, 0.5f, 0f);

    private void Awake()
    {
        if (jugador == null)
        {
            PlayerMovement movimiento = FindFirstObjectByType<PlayerMovement>();
            if (movimiento != null)
                jugador = movimiento.transform;
        }

        ConfigurarParticulas();
    }

    private void LateUpdate()
    {
        if (jugador == null)
            return;

        transform.position = jugador.position + desplazamiento;
    }

    private void ConfigurarParticulas()
    {
        ParticleSystem[] sistemas = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem sistema in sistemas)
        {
            ParticleSystem.MainModule principal = sistema.main;
            principal.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
    }
}
