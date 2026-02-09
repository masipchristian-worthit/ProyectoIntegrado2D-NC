using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ProximityCanvasFade : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("El objeto padre del panel que tiene el componente CanvasGroup.")]
    [SerializeField] private CanvasGroup uiCanvasGroup;

    [Header("Configuración")]
    [Tooltip("Si es TRUE, se desvanece al acercarse a los bordes laterales.")]
    [SerializeField] private bool fadeOnX = true;

    [Tooltip("Si es TRUE, se desvanece al acercarse a los bordes superior/inferior.")]
    [SerializeField] private bool fadeOnY = true;

    [Tooltip("Curva de desvanecimiento (1 = Lineal, 2 = Exponencial).")]
    [Range(0.1f, 3f)]
    [SerializeField] private float fadePower = 1f;

    private BoxCollider2D triggerBox;
    private Transform playerTransform;
    private bool isPlayerInside = false;

    void Awake()
    {
        triggerBox = GetComponent<BoxCollider2D>();
        triggerBox.isTrigger = true; // Aseguramos que sea Trigger

        // Inicializar apagado
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.blocksRaycasts = false; // Para que no bloquee el ratón si es invisible
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerTransform = collision.transform;
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerTransform = null;

            // Apagado limpio al salir
            if (uiCanvasGroup != null) uiCanvasGroup.alpha = 0f;
        }
    }

    void Update()
    {
        if (isPlayerInside && playerTransform != null && uiCanvasGroup != null)
        {
            CalculateOpacity();
        }
    }

    void CalculateOpacity()
    {
        // 1. Datos de la caja
        Vector2 center = triggerBox.bounds.center;
        Vector2 extents = triggerBox.bounds.extents; // Distancia del centro al borde
        Vector2 playerPos = playerTransform.position;

        float alphaX = 1f;
        float alphaY = 1f;

        // 2. Cálculo Eje X
        if (fadeOnX)
        {
            float dist = Mathf.Abs(playerPos.x - center.x);
            // Fórmula: 1 - (distancia / radio máximo)
            alphaX = 1f - Mathf.Clamp01(dist / extents.x);
        }

        // 3. Cálculo Eje Y
        if (fadeOnY)
        {
            float dist = Mathf.Abs(playerPos.y - center.y);
            alphaY = 1f - Mathf.Clamp01(dist / extents.y);
        }

        // 4. Combinar (Usamos el valor más bajo para que CUALQUIER borde transparente el texto)
        float finalAlpha = Mathf.Min(alphaX, alphaY);

        // Opcional: Aplicar potencia para hacer la transición más suave o abrupta
        finalAlpha = Mathf.Pow(finalAlpha, fadePower);

        // 5. Aplicar al UI
        uiCanvasGroup.alpha = finalAlpha;
    }
}