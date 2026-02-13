using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class AutoDialogueTrigger : MonoBehaviour
{
    [Header("Diálogo Automático")]
    [SerializeField] private DialogueNode dialogue;

    [Header("Tiempos")]
    [Tooltip("Segundos de espera antes de que empiece la secuencia.")]
    [SerializeField] private float executionDelay = 0.5f; // Recomendado 0.5s para dar aire al entrar

    [Header("Audio Previo (Opcional)")]
    [Tooltip("El índice del sonido en el AudioManager. Pon -1 si solo quieres texto.")]
    [SerializeField] private int soundIndex = -1;

    [Header("Comportamiento al terminar")]
    [SerializeField] private bool deactivateGameObject = true;

    [Header("Requisito de Objeto (Opcional)")]
    [SerializeField] private bool requiresItem = false;
    [SerializeField] private string requiredItemID;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Filtro: Solo el Player y si no se ha activado antes
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            // Nota: No comprobamos DialogueManager.Instance aquí todavía 
            // porque en la Build podría ser null un milisegundo.
            // Lo comprobamos dentro de la corrutina.

            // --- CHECK DE INVENTARIO ---
            if (requiresItem)
            {
                // Si el manager no está listo o no tiene el item, abortamos
                if (InventoryManager.Instance != null && !InventoryManager.Instance.HasItem(requiredItemID))
                {
                    return;
                }
            }
            // ---------------------------

            StartCoroutine(TriggerSequenceRoutine());
        }
    }

    private IEnumerator TriggerSequenceRoutine()
    {
        hasTriggered = true;

        // -----------------------------------------------------------
        // 1. ESPERA DE SEGURIDAD (CRÍTICO PARA LA BUILD)
        // -----------------------------------------------------------

        // Esperamos a que el DialogueManager esté listo (evita errores al cambiar de escena)
        while (DialogueManager.Instance == null)
        {
            yield return null; // Esperar al siguiente frame
        }

        // Si hemos configurado sonido, esperamos también al AudioManager
        if (soundIndex >= 0)
        {
            while (AudioManager.Instance == null)
            {
                yield return null;
            }
        }

        // -----------------------------------------------------------
        // 2. DELAY ESTÉTICO
        // -----------------------------------------------------------
        if (executionDelay > 0f)
        {
            yield return new WaitForSeconds(executionDelay);
        }

        // -----------------------------------------------------------
        // 3. LÓGICA DE AUDIO (Solo si soundIndex >= 0)
        // -----------------------------------------------------------
        if (soundIndex >= 0)
        {
            AudioManager.Instance.PlaySFX(soundIndex);

            // Esperar a que termine el sonido ANTES de sacar el texto
            // (Si prefieres que salgan a la vez, borra este bloque if)
            if (soundIndex < AudioManager.Instance.sfxLibrary.Length)
            {
                AudioClip clip = AudioManager.Instance.sfxLibrary[soundIndex];
                if (clip != null)
                {
                    yield return new WaitForSeconds(clip.length);
                }
            }
        }

        // -----------------------------------------------------------
        // 4. ABRIR DIÁLOGO
        // -----------------------------------------------------------
        if (dialogue != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }

        // -----------------------------------------------------------
        // 5. DESACTIVAR
        // -----------------------------------------------------------
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