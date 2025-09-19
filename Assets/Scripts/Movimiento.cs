using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class RamaOrgánica : MonoBehaviour
{
    [Header("Crecimiento")]
    public float growSpeed = 2f;
    public float stepSize = 0.1f;
    public float tipRadius = 0.1f;

    [Header("Curvatura y caída")]
    public float lateralFactor = 1f;
    public float caidaFactor = 1f;
    public float smoothFactor = 5f;

    [Header("Vibración rama principal")]
    public float vibracion = 0.05f;

    [Header("Flor")]
    public GameObject florPrefab;
    public float florOffset = 0.3f;

    [Header("Ramitas laterales")]
    public int segmentosRamita = 5;
    public float vibracionRamita = 0.05f;
    public float variacionLongitud = 0.2f;

    [Header("Sonido")]
    public AudioClip florSound; // 🎵 Clip de sonido para la flor

    private LineRenderer line;
    private Vector3 growDirection;
    private float distFromLastPoint = 0f;
    private GameObject tip;
    private Vector3 lastAnchorPosition;
    private bool tieneUltimaAncla = false;

    private AudioSource audioSource; // 🎵

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 1;
        line.SetPosition(0, transform.position);

        // ===== Asignar material y color verde =====
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.green;
        line.endColor = Color.green;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        // ==========================================

        growDirection = Vector3.up;

        tip = new GameObject("Tip");
        tip.transform.SetParent(transform);
        tip.transform.position = transform.position;

        SphereCollider sc = tip.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = tipRadius;

        Rigidbody rb = tip.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        var tipScript = tip.AddComponent<TipDetector>();
        tipScript.rama = this;

        // 🎵 Configurar el AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, v, 0f).normalized;

        if (inputDir != Vector3.zero)
        {
            growDirection = inputDir;

            Vector3 lastPos = line.GetPosition(line.positionCount - 1);
            Vector3 targetDir = growDirection;

            if (tieneUltimaAncla)
            {
                Vector3 toAnchor = lastAnchorPosition - lastPos;
                targetDir.x += -Mathf.Sign(toAnchor.x) * lateralFactor;
                targetDir.y -= toAnchor.magnitude * caidaFactor;
            }

            targetDir.x += Random.Range(-vibracion, vibracion);
            targetDir.y += Random.Range(-vibracion, vibracion);

            growDirection = Vector3.Lerp(
                growDirection,
                targetDir.normalized,
                Time.deltaTime * smoothFactor
            );

            float moveDist = growSpeed * Time.deltaTime;
            Vector3 newPos = lastPos + growDirection * moveDist;

            RaycastHit hit;
            if (Physics.Raycast(lastPos, growDirection, out hit, moveDist + tipRadius))
            {
                if (hit.collider.CompareTag("Obstaculo"))
                {
                    Debug.Log("⛔ Rama bloqueada por obstáculo");
                }
                else
                {
                    MoverRama(newPos, lastPos);
                }
            }
            else
            {
                MoverRama(newPos, lastPos);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CrearFlor();
        }
    }

    private void MoverRama(Vector3 newPos, Vector3 lastPos)
    {
        tip.transform.position = newPos;
        distFromLastPoint += (newPos - lastPos).magnitude;

        if (distFromLastPoint >= stepSize)
        {
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, newPos);
            distFromLastPoint = 0f;
        }
        else
        {
            line.SetPosition(line.positionCount - 1, newPos);
        }
    }

    public void CrearFlor()
    {
        Vector3 basePos = tip.transform.position;

        // 🌸 1) Crear FLOR ANCLA en el tip
        GameObject florAncla;
        if (florPrefab != null)
        {
            florAncla = Instantiate(florPrefab, basePos, Quaternion.identity);
        }
        else
        {
            florAncla = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            florAncla.transform.position = basePos;
            florAncla.transform.localScale = Vector3.one * 0.25f;
            florAncla.GetComponent<Renderer>().material.color = Color.yellow;
        }

        // ✅ Usar como nuevo anclaje
        AnclarEnFlor(florAncla.transform.position);

        // 🎵 Sonido
        if (florSound != null)
            audioSource.PlayOneShot(florSound);

        // 🌹 2) Generar adornos
        GenerarFloresDecorativas(basePos);

        Debug.Log("🌹 Flor ancla creada en " + basePos);
    }

    public void AnclarEnFlor(Vector3 florPos)
    {
        line.SetPosition(line.positionCount - 1, florPos);
        lastAnchorPosition = florPos;
        tieneUltimaAncla = true;

        Vector3 salida = florPos + growDirection.normalized * tipRadius;
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, salida);

        tip.transform.position = salida;
        distFromLastPoint = 0f;

        Debug.Log("🌸 Rama anclada en flor en " + florPos);
    }

    public void AnclarEnGrieta(Grieta grieta)
    {
        if (grieta.ocupada) return;

        grieta.Ocupada();
        Vector3 anclaPos = grieta.transform.position;

        line.SetPosition(line.positionCount - 1, anclaPos);
        lastAnchorPosition = anclaPos;
        tieneUltimaAncla = true;

        Vector3 salida = anclaPos + growDirection.normalized * tipRadius;
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, salida);

        tip.transform.position = salida;
        distFromLastPoint = 0f;

        // 🌹 También generar adornos en grieta
        GenerarFloresDecorativas(anclaPos);

        Debug.Log("🌸 Rama anclada en grieta: " + grieta.name);
    }

    private void GenerarFloresDecorativas(Vector3 basePos)
    {
        int numFloresExtra = Random.Range(3, 6); // 3 a 5

        for (int f = 0; f < numFloresExtra; f++)
        {
            // Offset en X, Y y Z
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(-1.5f, 0.5f)
            ).normalized * (florOffset + Random.Range(-variacionLongitud, variacionLongitud));

            Vector3 florPos = basePos + offset;

            GameObject flor;
            if (florPrefab != null)
            {
                flor = Instantiate(florPrefab, florPos, Quaternion.identity);
            }
            else
            {
                flor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flor.transform.position = florPos;
                flor.transform.localScale = Vector3.one * 0.2f;
                flor.GetComponent<Renderer>().material.color = Color.red;
            }

            // Crear ramita hacia la flor
            GameObject ramita = new GameObject("RamitaFlor");
            LineRenderer ramitaLine = ramita.AddComponent<LineRenderer>();
            ramitaLine.useWorldSpace = true;
            ramitaLine.startWidth = 0.05f;
            ramitaLine.endWidth = 0.03f;
            ramitaLine.positionCount = segmentosRamita;
            ramitaLine.material = new Material(Shader.Find("Sprites/Default"));
            ramitaLine.startColor = Color.green;
            ramitaLine.endColor = Color.green;

            for (int i = 0; i < segmentosRamita; i++)
            {
                float t = (float)i / (segmentosRamita - 1);
                Vector3 punto = Vector3.Lerp(basePos, florPos, t);
                punto.x += Random.Range(-vibracionRamita, vibracionRamita);
                punto.y += Random.Range(-vibracionRamita, vibracionRamita);
                punto.z += Random.Range(-vibracionRamita, vibracionRamita);
                ramitaLine.SetPosition(i, punto);
            }
            ramita.transform.SetParent(transform);
        }

        Debug.Log("🌹 Generadas " + numFloresExtra + " flores decorativas en " + basePos);
    }
}
