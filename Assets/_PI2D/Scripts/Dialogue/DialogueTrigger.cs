using UnityEngine;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Audio Previo")]
    [Tooltip("Sonido que suena ANTES de abrir el diálogo. El panel espera a que termine.")]
    [SerializeField] private AudioClip preDialogueSound;

    [Header("1. Diálogo Normal")]
    [SerializeField] private DialogueNode firstEncounterNode;

    [Header("2. Lógica 'Ya Visitado'")]
    [SerializeField] private bool useVisitedLogic = false;
    [SerializeField] private DialogueNode visitedNode;

    [Header("3. Lógica 'Requiere Objeto'")]
    [SerializeField] private bool requiresItem = false;
    [SerializeField] private string requiredItemID;
    [SerializeField] private DialogueNode lockedNode;

    private bool hasSpoken = false;
    private bool isPending = false; // Evita spamear mientras suena el audio previo

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerInteract"))
        {
            if (DialogueManager.Instance.IsDialogueActive || isPending) return;

            DialogueNode nodeToPlay = null;

            // Prioridades
            if (requiresItem)
            {
                if (InventoryManager.Instance != null && !InventoryManager.Instance.HasItem(requiredItemID))
                {
                    if (lockedNode != null) StartCoroutine(PlaySoundAndStart(lockedNode));
                    else Debug.LogWarning("Falta Locked Node");
                    return;
                }
            }

            if (useVisitedLogic && hasSpoken && visitedNode != null)
            {
                nodeToPlay = visitedNode;
            }
            else
            {
                nodeToPlay = firstEncounterNode;
                hasSpoken = true;
            }

            if (nodeToPlay != null)
            {
                StartCoroutine(PlaySoundAndStart(nodeToPlay));
            }
        }
    }

    private IEnumerator PlaySoundAndStart(DialogueNode node)
    {
        isPending = true; // Bloqueamos input

        // 1. Audio Previo
        if (preDialogueSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(preDialogueSound);
            yield return new WaitForSeconds(preDialogueSound.length);
        }

        // 2. Diálogo
        DialogueManager.Instance.StartDialogue(node);

        isPending = false; // Liberamos
    }

    public void ResetDialogueState() => hasSpoken = false;
}