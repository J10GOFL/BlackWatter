using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mantiene activas las luces cercanas al jugador o cuyo alcance cruza la cámara.
/// </summary>
public class LightDistanceCulling : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Si queda vacío, se busca el objeto que tenga PlayerMovement.")]
    [SerializeField] private Transform jugador;
    [Tooltip("Si queda vacío, se busca la cámara etiquetada MainCamera.")]
    [SerializeField] private Camera camara;

    [Header("Distancias")]
    [Tooltip("Distancia a la que una luz apagada vuelve a encenderse.")]
    [Min(0f)] [SerializeField] private float distanciaActivacion = 24f;
    [Tooltip("Distancia a la que una luz se apaga si tampoco está dentro del encuadre.")]
    [Min(0f)] [SerializeField] private float distanciaDesactivacion = 30f;
    [Min(0.1f)] [SerializeField] private float intervaloRevision = 0.5f;

    private Light[] luces;
    private readonly Plane[] planosCamara = new Plane[6];
    private float siguienteRevision;

    void Start()
    {
        BuscarReferencias();
        BuscarLuces();
        RevisarLuces();
    }

    void Update()
    {
        if (jugador == null || Time.time < siguienteRevision)
            return;

        RevisarLuces();
    }

    void OnDisable()
    {
        if (luces == null)
            return;

        // Restaura las luces gestionadas al desactivar este componente.
        foreach (Light luz in luces)
        {
            if (luz != null)
                luz.enabled = true;
        }
    }

    void BuscarReferencias()
    {
        PlayerMovement movimiento = FindFirstObjectByType<PlayerMovement>();
        if (jugador == null && movimiento != null)
            jugador = movimiento.transform;

        if (jugador == null)
        {
            CameraFollow seguimiento = FindFirstObjectByType<CameraFollow>();
            if (seguimiento != null)
                jugador = seguimiento.target;
        }

        if (camara == null && movimiento != null && movimiento.cam != null)
            camara = movimiento.cam.GetComponent<Camera>();

        if (camara == null)
            camara = Camera.main;

        if (camara == null)
        {
            CameraFollow seguimiento = FindFirstObjectByType<CameraFollow>();
            if (seguimiento != null)
                camara = seguimiento.GetComponent<Camera>();
        }
    }

    void BuscarLuces()
    {
        Light[] lucesDeEscena = FindObjectsByType<Light>(FindObjectsSortMode.None);
        var lucesGestionadas = new List<Light>(lucesDeEscena.Length);

        foreach (Light luz in lucesDeEscena)
        {
            if (luz == null || !luz.enabled)
                continue;

            // Las luces horneadas y mixtas no se controlan con este sistema.
            if (luz.lightmapBakeType != LightmapBakeType.Realtime)
                continue;

            // En MapaDemo predominan las luces puntuales.
            if (luz.type != LightType.Point)
                continue;

            lucesGestionadas.Add(luz);
        }

        luces = lucesGestionadas.ToArray();
    }

    void RevisarLuces()
    {
        if (jugador == null)
        {
            siguienteRevision = Time.time + Mathf.Max(0.1f, intervaloRevision);
            return;
        }

        if (camara != null)
            GeometryUtility.CalculateFrustumPlanes(camara, planosCamara);

        float distanciaActivacionSegura = Mathf.Max(0f, distanciaActivacion);
        float distanciaDesactivacionSegura = Mathf.Max(
            distanciaActivacionSegura,
            distanciaDesactivacion
        );
        float distanciaActivacionCuadrada = distanciaActivacionSegura * distanciaActivacionSegura;
        float distanciaDesactivacionCuadrada = distanciaDesactivacionSegura * distanciaDesactivacionSegura;
        Vector3 posicionJugador = jugador.position;

        foreach (Light luz in luces)
        {
            if (luz == null)
                continue;

            float distanciaCuadrada = (luz.transform.position - posicionJugador).sqrMagnitude;
            bool dentroDelEncuadre = camara != null && LuzCruzaEncuadre(luz);

            if (luz.enabled)
            {
                if (distanciaCuadrada > distanciaDesactivacionCuadrada && !dentroDelEncuadre)
                    luz.enabled = false;
            }
            else if (distanciaCuadrada < distanciaActivacionCuadrada || dentroDelEncuadre)
            {
                luz.enabled = true;
            }
        }

        siguienteRevision = Time.time + Mathf.Max(0.1f, intervaloRevision);
    }

    bool LuzCruzaEncuadre(Light luz)
    {
        // Usa el alcance de la luz como volumen para no apagar su contribución visible.
        float diametro = luz.range * 2f;
        var volumenDeLuz = new Bounds(luz.transform.position, Vector3.one * diametro);
        return GeometryUtility.TestPlanesAABB(planosCamara, volumenDeLuz);
    }
}
