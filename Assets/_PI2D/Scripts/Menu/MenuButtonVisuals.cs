using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;

public class MenuButtonVisuals : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [Header("Configuración Visual")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite; // Sprite al estar seleccionado
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField] private float animDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] private int hoverSoundIndex = 0;
    [SerializeField] private int clickSoundIndex = 1;

    [Header("Lógica de Botón")]
    [Tooltip("Si marcas esto, el botón cerrará el juego al terminar la animación.")]
    [SerializeField] private bool quitGameOnPress = false;

    [Tooltip("Arrastra aquí lo que pasa DESPUÉS de la animación (Cambiar escena, etc). Ignóralo si usas 'Quit Game'.")]
    public UnityEvent OnClickDelayed;

    private Image targetImage;
    private Vector3 originalScale;
    private Tween currentTween;

    void Awake()
    {
        targetImage = GetComponent<Image>();
        originalScale = transform.localScale;

        // Estado inicial limpio
        if (targetImage != null && normalSprite != null) targetImage.sprite = normalSprite;
    }

    void OnEnable()
    {
        // Al aparecer el menú, reseteamos a estado normal (pequeño)
        if (currentTween != null) currentTween.Kill();
        transform.localScale = originalScale;
        if (targetImage != null && normalSprite != null) targetImage.sprite = normalSprite;
    }

    void OnDisable()
    {
        if (currentTween != null) currentTween.Kill();
        transform.localScale = originalScale;
    }

    // --- LÓGICA DE SELECCIÓN (Navegación con Teclado/Mando) ---
    public void OnSelect(BaseEventData eventData)
    {
        if (currentTween != null) currentTween.Kill();

        // 1. Cambio de Sprite
        if (targetImage != null && hoverSprite != null)
            targetImage.sprite = hoverSprite;

        // 2. Escala (Multiplica)
        // Guardamos el tween para poder matarlo si cambias rápido
        currentTween = transform.DOScale(originalScale * hoverScaleMultiplier, animDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true); // Funciona en Pausa

        // 3. Sonido
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(hoverSoundIndex);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (currentTween != null) currentTween.Kill();

        // 1. Restaurar Sprite Normal
        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;

        // 2. Restaurar Escala Original
        currentTween = transform.DOScale(originalScale, animDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    // --- LÓGICA DE CLICK ("Accept" / Enter) ---
    public void OnSubmit(BaseEventData eventData)
    {
        if (currentTween != null) currentTween.Kill();

        // 1. Sonido
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clickSoundIndex);

        // 2. Animación Punch
        // Se encoge un poco (-0.2) y rebota
        transform.DOPunchScale(Vector3.one * -0.2f, 0.2f, 10, 1)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // A. Lógica de Salir del Juego
                if (quitGameOnPress)
                {
                    Debug.Log("[MenuButton] Cerrando aplicación...");
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                }

                // B. Ejecutar otros eventos (Cambio de escena, etc)
                OnClickDelayed.Invoke();

                // C. CORRECCIÓN IMPORTANTE: MANTENER ESCALA GRANDE
                // Como el botón sigue seleccionado, lo dejamos en tamaño "Hover"
                transform.localScale = originalScale * hoverScaleMultiplier;
            });
    }
}