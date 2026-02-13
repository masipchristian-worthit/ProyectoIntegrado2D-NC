using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Configuración General")]
    [Tooltip("Si es TRUE, el collider se desactivará para siempre tras hablar.")]
    [SerializeField] private bool disableColliderOnEnd = false;

    [Header("Audio Previo")]
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
    private bool isPending = false;
    private Collider2D myCollider;

    private void Awake() => myCollider = GetComponent<Collider2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerInteract"))
        {
            if (DialogueManager.Instance.IsDialogueActive || isPending) return;

            DialogueNode nodeToPlay = null;
            bool shouldDisable = false;

            // Check Objeto
            if (requiresItem)
            {
                if (InventoryManager.Instance != null && !InventoryManager.Instance.HasItem(requiredItemID))
                {
                    if (lockedNode != null) StartCoroutine(PlaySoundAndStart(lockedNode, false));
                    else Debug.LogWarning($"[DialogueTrigger] Falta Locked Node en {name}");
                    return;
                }
            }

            // Selección de nodo
            if (useVisitedLogic && hasSpoken && visitedNode != null)
            {
                nodeToPlay = visitedNode;
                shouldDisable = disableColliderOnEnd;
            }
            else
            {
                nodeToPlay = firstEncounterNode;
                hasSpoken = true;
                shouldDisable = disableColliderOnEnd;
            }

            if (nodeToPlay != null)
                StartCoroutine(PlaySoundAndStart(nodeToPlay, shouldDisable));
        }
    }

    private IEnumerator PlaySoundAndStart(DialogueNode node, bool disableCollider)
    {
        isPending = true;

        if (preDialogueSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(preDialogueSound);
            yield return new WaitForSeconds(preDialogueSound.length);
        }

        DialogueManager.Instance.StartDialogue(node);

        if (disableCollider && myCollider != null)
        {
            myCollider.enabled = false;
            Debug.Log($"[DialogueTrigger] Collider desactivado en {name}");
        }

        isPending = false;
    }

    public void ResetDialogueState()
    {
        hasSpoken = false;
        if (myCollider != null) myCollider.enabled = true;
    }
}