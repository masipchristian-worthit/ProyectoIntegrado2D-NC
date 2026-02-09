using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class ShaderZoneTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Deja esto VACÍO para que lo coja automáticamente del DialogueManager.")]
    [SerializeField] private Material targetMaterial;

    [Tooltip("Arrastra aquí tu archivo MaterialPresetSO. OBLIGATORIO.")]
    [SerializeField] private MaterialPresetSO targetPreset;

    [Header("Configuración Zona")]
    [Tooltip("¿La transición es horizontal (X) o vertical (Y)?")]
    [SerializeField] private Orientation orientation = Orientation.Horizontal;

    [Header("Audio Ambiente")]
    [Tooltip("El NÚMERO del sonido en la lista 'Sfx Library' del AudioManager.")]
    [SerializeField] private int soundIndex = -1;

    [Range(0f, 1f)]
    [SerializeField] private float maxVolume = 1.0f;

    [Header("Balance de Audio")]
    [Tooltip("-1 = Izquierda Total | 0 = Centro | 1 = Derecha Total")]
    [Range(-1f, 1f)]
    [SerializeField] private float stereoPan = -0.4f; // Valor por defecto para 70% izq / 30% der

    private enum Orientation { Horizontal, Vertical }

    // Estructura interna
    private struct ShaderSnapshot
    {
        public Color lightColor;
        public Color darkColor;
        public float noiseSpeed;
        public float noiseScale;
        public float ditherThreshold;
        public float ditherStrength;
        public float softness;
        public float textureBlend;
    }

    // Variables de Estado
    private BoxCollider2D zoneCollider;
    private AudioSource audioSource;
    private Transform playerTransform;
    private bool isPlayerInside = false;

    // Variables para la interpolación
    private ShaderSnapshot startValues;
    private float startCoord;
    private float endCoord;

    void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();

        // 1. SPATIAL BLEND en 0 = Sonido 2D (Ambiente global)
        audioSource.spatialBlend = 0f;

        // 2. STEREO PAN = Define si suena más por la izquierda o derecha
        // -0.4 es aprox 70% Izquierda / 30% Derecha
        audioSource.panStereo = stereoPan;

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    void Start()
    {
        // Auto-referencia del material
        if (targetMaterial == null && DialogueManager.Instance != null)
        {
            targetMaterial = DialogueManager.Instance.GlobalMaterial;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetPreset == null)
        {
            Debug.LogWarning($"[ShaderZone] Falta MaterialPresetSO en {gameObject.name}");
            return;
        }

        // Reintento de referencia por si acaso
        if (targetMaterial == null && DialogueManager.Instance != null)
        {
            targetMaterial = DialogueManager.Instance.GlobalMaterial;
        }

        if (targetMaterial == null) return;

        if (collision.CompareTag("Player"))
        {
            playerTransform = collision.transform;
            isPlayerInside = true;

            CaptureCurrentSettings();
            CalculateEntryExitPoints();

            // Iniciar Audio desde el Manager
            if (AudioManager.Instance != null && soundIndex >= 0)
            {
                if (soundIndex < AudioManager.Instance.sfxLibrary.Length)
                {
                    audioSource.clip = AudioManager.Instance.sfxLibrary[soundIndex];

                    // Aseguramos que el pan y el modo 2D se aplican al clip nuevo
                    audioSource.spatialBlend = 0f;
                    audioSource.panStereo = stereoPan; // APLICAMOS EL PANNING AQUÍ TAMBIÉN

                    audioSource.volume = 0f;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning($"[ShaderZone] Índice de audio {soundIndex} fuera de rango.");
                }
            }
        }
    }

    void Update()
    {
        if (isPlayerInside && playerTransform != null && targetPreset != null && targetMaterial != null)
        {
            float playerCoord = (orientation == Orientation.Horizontal) ? playerTransform.position.x : playerTransform.position.y;
            float progress = Mathf.InverseLerp(startCoord, endCoord, playerCoord);

            ApplyLerpedSettings(progress);

            // Subimos volumen según progreso
            audioSource.volume = Mathf.Lerp(0f, maxVolume, progress);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (targetPreset == null) return;

        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            audioSource.Stop();

            float playerCoord = (orientation == Orientation.Horizontal) ? playerTransform.position.x : playerTransform.position.y;
            float distToEnd = Mathf.Abs(playerCoord - endCoord);
            float distToStart = Mathf.Abs(playerCoord - startCoord);

            if (distToEnd < distToStart)
            {
                ApplyFinalSettings();
                gameObject.SetActive(false);
            }
            else
            {
                RestoreStartSettings();
            }
        }
    }

    // --- HELPERS ---

    void CalculateEntryExitPoints()
    {
        Bounds b = zoneCollider.bounds;
        float playerPos = (orientation == Orientation.Horizontal) ? playerTransform.position.x : playerTransform.position.y;
        float min = (orientation == Orientation.Horizontal) ? b.min.x : b.min.y;
        float max = (orientation == Orientation.Horizontal) ? b.max.x : b.max.y;

        float distToMin = Mathf.Abs(playerPos - min);
        float distToMax = Mathf.Abs(playerPos - max);

        if (distToMin < distToMax)
        {
            startCoord = min;
            endCoord = max;
        }
        else
        {
            startCoord = max;
            endCoord = min;
        }
    }

    void CaptureCurrentSettings()
    {
        startValues.lightColor = targetMaterial.GetColor("_LightColor");
        startValues.darkColor = targetMaterial.GetColor("_DarkColor");
        startValues.noiseSpeed = targetMaterial.GetFloat("_NoiseSpeed");
        startValues.noiseScale = targetMaterial.GetFloat("_NoiseScale");
        startValues.ditherThreshold = targetMaterial.GetFloat("_DitherThreshold");
        startValues.ditherStrength = targetMaterial.GetFloat("_DitherStrength");
        startValues.softness = targetMaterial.GetFloat("_Softness");
        startValues.textureBlend = targetMaterial.GetFloat("_TextureBlend");
    }

    void ApplyLerpedSettings(float t)
    {
        targetMaterial.SetColor("_LightColor", Color.Lerp(startValues.lightColor, targetPreset.lightColor, t));
        targetMaterial.SetColor("_DarkColor", Color.Lerp(startValues.darkColor, targetPreset.darkColor, t));

        targetMaterial.SetFloat("_NoiseSpeed", Mathf.Lerp(startValues.noiseSpeed, targetPreset.noiseSpeed, t));
        targetMaterial.SetFloat("_NoiseScale", Mathf.Lerp(startValues.noiseScale, targetPreset.noiseScale, t));
        targetMaterial.SetFloat("_DitherThreshold", Mathf.Lerp(startValues.ditherThreshold, targetPreset.ditherThreshold, t));
        targetMaterial.SetFloat("_DitherStrength", Mathf.Lerp(startValues.ditherStrength, targetPreset.ditherStrength, t));
        targetMaterial.SetFloat("_Softness", Mathf.Lerp(startValues.softness, targetPreset.softness, t));
        targetMaterial.SetFloat("_TextureBlend", Mathf.Lerp(startValues.textureBlend, targetPreset.textureBlend, t));
    }

    void ApplyFinalSettings() => ApplyLerpedSettings(1.0f);
    void RestoreStartSettings() => ApplyLerpedSettings(0.0f);
}