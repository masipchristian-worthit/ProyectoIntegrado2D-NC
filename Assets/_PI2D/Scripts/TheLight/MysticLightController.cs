using UnityEngine;
using DG.Tweening;

public class MysticLightController : MonoBehaviour
{
    [Header("Referencias Visuales")]
    [SerializeField] private Transform starCore; // El hijo que tiene el Sprite

    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 360f; // Grados por segundo
    [SerializeField] private float disappearOffset = 2f; // Margen extra para asegurar que salió de cámara

    private void OnEnable()
    {
        // 1. BLOQUEO DEL JUGADOR
        // Pasamos a mapa UI para que el personaje no se mueva, pero sin pausar el tiempo (TimeScale 1)
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);
        }

        // 2. CÁLCULO DE DESTINO (Borde derecho de la cámara)
        Camera cam = Camera.main;
        float screenRightWorldPos = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, cam.nearClipPlane)).x;
        float targetX = screenRightWorldPos + disappearOffset;

        // Calculamos la duración basada en la distancia y velocidad (t = d / v)
        float distance = Mathf.Abs(targetX - transform.position.x);
        float duration = distance / moveSpeed;

        // 3. ANIMACIÓN DE MOVIMIENTO (DOTween)
        transform.DOMoveX(targetX, duration)
            .SetEase(Ease.Linear) // Movimiento constante, sin aceleración
            .SetUpdate(UpdateType.Normal) // Respetamos TimeScale normal
            .OnComplete(FinishSequence);
    }

    private void Update()
    {
        // Rotación constante de la estrella (Sentido agujas del reloj = Z negativo)
        if (starCore != null)
        {
            starCore.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }

    private void FinishSequence()
    {
        // 4. DESBLOQUEO DEL JUGADOR
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
        }

        // 5. APAGADO
        gameObject.SetActive(false);
    }

    // Seguridad: Si se deshabilita el objeto externamente, devolvemos el control
    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            // Solo devolvemos a Gameplay si NO estamos en un diálogo real
            // (Evita conflictos si la luz se apaga justo al entrar en un diálogo)
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive)
            {
                InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
            }
        }
    }
}