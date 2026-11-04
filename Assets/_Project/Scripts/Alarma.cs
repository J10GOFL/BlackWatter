using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class Alarma : MonoBehaviour
{
    [Header("Ritmo de alarma")]
    [SerializeField, Min(0.01f)] private float tiempoEncendidas = 0.25f;
    [SerializeField, Min(0.01f)] private float tiempoApagadas = 0.25f;

    [Header("Sonido")]
    [SerializeField] private AudioClip sonidoAlarma;

    private Light[] luces;
    private AudioSource fuenteAudio;
    private bool yaSeActivo;

    private void Awake()
    {
        BoxCollider zona = GetComponent<BoxCollider>();
        zona.isTrigger = true;

        Rigidbody cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;

        Transform auto = transform.parent != null ? transform.parent : transform;
        luces = auto.GetComponentsInChildren<Light>(true);
        CambiarLuces(false);

        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null)
            fuenteAudio = gameObject.AddComponent<AudioSource>();

        fuenteAudio.playOnAwake = false;
        fuenteAudio.loop = true;
        fuenteAudio.spatialBlend = 1f;
        fuenteAudio.clip = sonidoAlarma;
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (yaSeActivo || otro.GetComponentInParent<PlayerMovement>() == null)
            return;

        yaSeActivo = true;
        if (sonidoAlarma != null)
            fuenteAudio.Play();

        StartCoroutine(ParpadearEnBucle());
    }

    private IEnumerator ParpadearEnBucle()
    {
        while (true)
        {
            CambiarLuces(true);
            yield return new WaitForSeconds(tiempoEncendidas);

            CambiarLuces(false);
            yield return new WaitForSeconds(tiempoApagadas);
        }
    }

    private void OnDisable()
    {
        CambiarLuces(false);
        if (fuenteAudio != null)
            fuenteAudio.Stop();
    }

    private void CambiarLuces(bool encendidas)
    {
        if (luces == null)
            return;

        foreach (Light luz in luces)
        {
            if (luz != null)
                luz.enabled = encendidas;
        }
    }
}
