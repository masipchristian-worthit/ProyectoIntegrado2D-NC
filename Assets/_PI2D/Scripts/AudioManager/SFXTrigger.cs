using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SFXTrigger : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [Tooltip("El índice del audio en el array SFXLibrary del AudioManager")]
    public int sfxIndex = 0;
    
    [Tooltip("0 = Normal (puede esperar). 1 = Prioritario (Bloquea a los siguientes hasta terminar).")]
    [Range(0, 1)] 
    public int priority = 0;

    [Tooltip("Si es true, el audio solo sonará la primera vez que toques el trigger")]
    public bool playOnlyOnce = true;

    [Header("Control de Movimiento")]
    [Tooltip("Activa esto para modificar la velocidad del jugador MIENTRAS espera y mientras habla")]
    public bool modifyPlayerSpeed = false;
    
    [Tooltip("La velocidad que tendrá el jugador (0 para congelarlo)")]
    public float temporarySpeed = 0f;

    [Header("Estado (Debug)")]
    public bool hasPlayed = false;

    //Referenciar speed del Player
    PlayerController2D playerController;

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playOnlyOnce && hasPlayed) return;
            StartCoroutine(PlaySequenceRoutine());
        }
    }

    private IEnumerator PlaySequenceRoutine()
    {
        hasPlayed = true;

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("SFXTrigger: No hay AudioManager.");
            yield break;
        }

        // 1. CÁLCULO DE DURACIÓN (Antes de nada)
        float clipDuration = 0f;
        if (sfxIndex >= 0 && sfxIndex < AudioManager.Instance.sfxLibrary.Length)
        {
            AudioClip clip = AudioManager.Instance.sfxLibrary[sfxIndex];
            if (clip != null) clipDuration = clip.length;
        }
        else
        {
            Debug.LogError("SFXTrigger: Índice fuera de rango.");
            yield break;
        }

        // 2. APLICAR FRENO INMEDIATO (Requisito: Frenar aunque tenga que esperar)
        float originalSpeed = 10f;
        if (playerController != null)
        {
            originalSpeed = playerController.moveSpeed; // Guardar velocidad actual
            if (modifyPlayerSpeed)
            {
                playerController.moveSpeed = temporarySpeed; // Aplicar freno
            }
        }

        // 3. ESPERAR TURNO (Cola de prioridad)
        // Mientras haya OTRO audio prioritario sonando, esperamos aquí.
        // El jugador ya está frenado si modifyPlayerSpeed estaba activo.
        while (AudioManager.Instance.isPriorityPlaying)
        {
            yield return null;
        }

        // 4. BLOQUEAR CANAL (Si somos Prioridad 1)
        if (priority == 1)
        {
            AudioManager.Instance.isPriorityPlaying = true;
        }

        // 5. REPRODUCIR SONIDO
        AudioManager.Instance.PlaySFX(sfxIndex);

        // 6. ESPERAR DURACIÓN DEL AUDIO
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // 7. DESBLOQUEAR CANAL (Si éramos Prioridad 1)
        if (priority == 1)
        {
            AudioManager.Instance.isPriorityPlaying = false;
        }

        // 8. RESTAURAR VELOCIDAD
        if (modifyPlayerSpeed && playerController != null)
        {
            playerController.moveSpeed = originalSpeed;
        }
    }
}