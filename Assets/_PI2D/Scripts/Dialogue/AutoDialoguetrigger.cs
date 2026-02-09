using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class AutoDialogueTrigger : MonoBehaviour
{
    [Header("Diálogo Automático")]
    [SerializeField] private DialogueNode dialogue;

    [Header("Tiempos")]
    [Tooltip("Segundos de espera antes de que empiece a sonar el audio o salga el texto.")]
    [SerializeField] private float executionDelay = 0f;

    [Header("Audio Previo")]
    [Tooltip("El NÚMERO del sonido en la lista 'Sfx Library' del AudioManager. Pon -1 si no quieres sonido.")]
    [SerializeField] private int soundIndex = -1;

    [Header("Comportamiento al terminar")]
    [SerializeField] private bool deactivateGameObject = true;

    [Header("Requisito de Objeto (Opcional)")]
    [SerializeField] private bool requiresItem = false;
    [SerializeField] private string requiredItemID;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. ¿Detecta colisión física?
        Debug.Log($"[TEST] Algo entró en el trigger: {collision.gameObject.name} | Tag: {collision.tag}");

        if (collision.CompareTag("Player") && !hasTriggered)
        {
            // 2. ¿Pasa el filtro de Tag?
            Debug.Log("[TEST] Tag Player correcto.");

            if (DialogueManager.Instance.IsDialogueActive)
            {
                Debug.Log("[TEST] BLOQUEADO: El DialogueManager dice que ya hay un diálogo activo.");
                return;
            }

            Debug.Log("[TEST] Iniciando secuencia...");
            StartCoroutine(TriggerSequenceRoutine());
        }
    }

    private IEnumerator TriggerSequenceRoutine()
    {
        hasTriggered = true;

        // 1. DELAY INICIAL (NUEVO)
        if (executionDelay > 0f)
        {
            yield return new WaitForSeconds(executionDelay);
        }

        // 2. LÓGICA DE AUDIO
        if (soundIndex >= 0 && AudioManager.Instance != null)
        {
            if (soundIndex < AudioManager.Instance.sfxLibrary.Length)
            {
                // A. Reproducir
                AudioManager.Instance.PlaySFX(soundIndex);

                // B. Esperar duración del audio
                AudioClip clip = AudioManager.Instance.sfxLibrary[soundIndex];
                if (clip != null)
                {
                    yield return new WaitForSeconds(clip.length);
                }
            }
            else
            {
                Debug.LogWarning($"[AutoDialogueTrigger] El índice {soundIndex} no existe en AudioManager.");
            }
        }

        // 3. ABRIR DIÁLOGO
        if (dialogue != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }

        // 4. DESACTIVAR
        if (deactivateGameObject)
        {
            gameObject.SetActive(false);
        }
        else
        {
            GetComponent<Collider2D>().enabled = false;
        }
    }
}