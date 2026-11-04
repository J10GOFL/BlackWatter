using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SonidoPasos : MonoBehaviour
{
    [Header("Sonido al caminar")]
    [SerializeField] private AudioSource fuente;
    [SerializeField] private AudioClip sonidoCaminar;

    [Header("Sonidos al correr")]
    [SerializeField] private AudioClip sonidoCarreraPaso1;
    [SerializeField] private AudioClip sonidoCarreraPaso2;

    [Header("Sonidos del salto")]
    [SerializeField] private AudioClip sonidoAlIniciarSalto;
    [SerializeField] private AudioClip sonidoAlAterrizar;

    [Header("Control de pisadas")]
    [SerializeField, Min(0f)] private float intervaloMinimoEntrePasos = 0.08f;

    private float ultimoPaso = -1f;

    private void Awake()
    {
        if (fuente == null)
            fuente = GetComponent<AudioSource>();
    }

    // Asignar este método al evento de pisada al caminar.
    public void Paso()
    {
        ReproducirPaso(sonidoCaminar);
    }

    // Asignar este método al primer apoyo de la animación de correr.
    public void Paso1()
    {
        ReproducirPaso(sonidoCarreraPaso1);
    }

    // Asignar este método al segundo apoyo de la animación de correr.
    public void Paso2()
    {
        ReproducirPaso(sonidoCarreraPaso2);
    }

    // Debe coincidir con el evento Inicio de la animación de salto.
    public void Inicio()
    {
        Reproducir(sonidoAlIniciarSalto);
    }

    // Debe coincidir con el evento SaltoPiso de la animación de salto.
    public void SaltoPiso()
    {
        Reproducir(sonidoAlAterrizar);
    }

    private void ReproducirPaso(AudioClip sonido)
    {
        if (Time.time - ultimoPaso < intervaloMinimoEntrePasos)
            return;

        ultimoPaso = Time.time;
        Reproducir(sonido);
    }

    private void Reproducir(AudioClip sonido)
    {
        if (fuente != null && sonido != null)
            fuente.PlayOneShot(sonido);
    }
}