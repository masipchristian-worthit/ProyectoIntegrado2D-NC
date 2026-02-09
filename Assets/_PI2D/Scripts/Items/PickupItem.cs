using UnityEngine;
using DG.Tweening;

public class PickupItem : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("ID único del objeto (Debe coincidir con el de InventoryManager si lo usas).")]
    [SerializeField] private string itemID;

    [Header("Animación")]
    [SerializeField] private float pushBackDistance = 0.5f; // Cuánto retrocede primero
    [SerializeField] private float pushDuration = 0.2f;     // Tiempo de retroceso
    [SerializeField] private float suckDuration = 0.4f;     // Tiempo hacia el jugador

    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Si el inventario ya dice que lo tenemos, nos apagamos al inicio
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(itemID))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;

        if (collision.CompareTag("PlayerInteract"))
        {
            Collect(collision.transform.parent); // Asumimos que el collider es hijo del Player
        }
    }

    private void Collect(Transform playerTransform)
    {
        isCollected = true;

        // 1. CAMBIO VISUAL: Poner por delante de todo
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10;
        }

        // 2. CÁLCULO DE DIRECCIONES
        // Dirección desde el jugador hacia el objeto (para empujarlo en esa dirección)
        Vector3 directionAway = (transform.position - playerTransform.position).normalized;
        Vector3 pushTarget = transform.position + (directionAway * pushBackDistance);

        // 3. SECUENCIA DOTWEEN
        Sequence sequence = DOTween.Sequence();

        // Paso A: Retroceder (Efecto de "coger impulso")
        sequence.Append(transform.DOMove(pushTarget, pushDuration).SetEase(Ease.OutQuad));

        // Paso B: Ir hacia el centro del jugador
        sequence.Append(transform.DOMove(playerTransform.position, suckDuration).SetEase(Ease.InBack));

        // Paso C: Desvanecerse y apagarse (Inventario lo registrará al apagarse)
        sequence.Insert(pushDuration + suckDuration * 0.5f, transform.DOScale(0, suckDuration * 0.5f));

        sequence.OnComplete(() =>
        {
            // Al desactivarse, tu InventoryManager detectará que falta y lo guardará
            gameObject.SetActive(false);
        });
    }
}