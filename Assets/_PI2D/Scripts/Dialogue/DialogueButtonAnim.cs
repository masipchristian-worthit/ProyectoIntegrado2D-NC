using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DialogueButtonAnim : MonoBehaviour
{
    private Tween currentTween;

    // Al desactivarse, matamos cualquier animación pendiente
    void OnDisable()
    {
        if (currentTween != null) currentTween.Kill();
        transform.localScale = Vector3.one; // Reset de seguridad
    }

    // --- ANIMACIÓN DE CLICK (KNOCKBACK) ---
    // Esta función la llama el DialogueManager al pulsar aceptar
    public void PlayClickAnimation(System.Action onComplete)
    {
        if (currentTween != null) currentTween.Kill();

        // Efecto Punch: Se encoge un 20% y rebota rápido
        currentTween = transform.DOPunchScale(Vector3.one * -0.2f, 0.2f, 10, 1)
            .SetUpdate(true) // Ignora el TimeScale 0 del diálogo
            .OnComplete(() => onComplete?.Invoke());
    }
}