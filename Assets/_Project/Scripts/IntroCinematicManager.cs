using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroCinematicManager : MonoBehaviour
{
    [Header("TV y Cámara")]
    public GameObject objetoTVQuad;
    public Camera mainCamera;

    [Header("Configuración de Puntos de Cámara")]
    public Transform puntoCercaTV;
    public Transform puntoLejosHabitacion;

    [Tooltip("Baja este número (ej. 0.8 o 1.0) para que la cámara se mueva más lento y de tiempo a procesar.")]
    public float velocidadMovimiento = 0.9f;

    private bool acercandoCamara = false;
    private bool alejandoCamara = false;

    [Header("UI y Diálogo Final")]
    public TextMeshProUGUI textoDialogo;
    public GameObject botonIrADormir;
    [TextArea] public string mensajeFinal = "Qué cosa tan extraña... Debería averiguar con los vecinos mañana para saber si fue cierta esa extraña advertencia.";

    private VideoPlayer videoPlayer;

    void Start()
    {
        if (textoDialogo != null) textoDialogo.gameObject.SetActive(false);
        if (botonIrADormir != null) botonIrADormir.SetActive(false);
        if (objetoTVQuad != null) objetoTVQuad.SetActive(false);

        if (objetoTVQuad != null)
        {
            videoPlayer = objetoTVQuad.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += AlTerminarVideo;
            }
        }

        if (puntoLejosHabitacion != null && mainCamera != null)
        {
            mainCamera.transform.position = puntoLejosHabitacion.position;
            mainCamera.transform.rotation = puntoLejosHabitacion.rotation;
        }

        // Aumentado a 3.0 segundos de suspenso antes de que arranque todo
        Invoke(nameof(IniciarCinematica), 3.0f);
    }

    void Update()
    {
        if (acercandoCamara && puntoCercaTV != null && mainCamera != null)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, puntoCercaTV.position, velocidadMovimiento * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, puntoCercaTV.rotation, velocidadMovimiento * Time.deltaTime);

            if (Vector3.Distance(mainCamera.transform.position, puntoCercaTV.position) < 0.05f)
            {
                acercandoCamara = false;
            }
        }

        if (alejandoCamara && puntoLejosHabitacion != null && mainCamera != null)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, puntoLejosHabitacion.position, velocidadMovimiento * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, puntoLejosHabitacion.rotation, velocidadMovimiento * Time.deltaTime);
        }
    }

    void IniciarCinematica()
    {
        if (objetoTVQuad != null) objetoTVQuad.SetActive(true);
        if (videoPlayer != null) videoPlayer.Play();

        acercandoCamara = true;
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        acercandoCamara = false;
        alejandoCamara = true;

        if (textoDialogo != null)
        {
            textoDialogo.gameObject.SetActive(true);
            textoDialogo.text = mensajeFinal;
        }

        if (botonIrADormir != null)
        {
            botonIrADormir.SetActive(true);
        }
    }

    public void CargarEscenaCalle(string nombreEscenaCalle)
    {
        SceneManager.LoadScene(nombreEscenaCalle);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= AlTerminarVideo;
        }
    }
}