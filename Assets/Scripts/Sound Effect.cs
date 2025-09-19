using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RamaSonido : MonoBehaviour
{
    private RamaOrgánica rama;
    private AudioSource audioSource;

    [Header("Sensibilidad del movimiento")]
    public float minMovimiento = 0.01f;

    private Transform tip;
    private Vector3 ultimaPosicion;

    void Start()
    {
        rama = GetComponent<RamaOrgánica>();
        audioSource = GetComponent<AudioSource>();

        tip = transform.Find("Tip");
        if (tip == null) Debug.LogError("No se encontró el objeto Tip en la rama.");
        ultimaPosicion = tip != null ? tip.position : transform.position;

        audioSource.loop = true;   // 👈 importante para que suene en bucle
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        Vector3 posicionActual = tip != null ? tip.position : rama.transform.position;
        float velocidad = (posicionActual - ultimaPosicion).magnitude / Time.deltaTime;

        if (velocidad > minMovimiento)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }

        ultimaPosicion = posicionActual;
    }
}
