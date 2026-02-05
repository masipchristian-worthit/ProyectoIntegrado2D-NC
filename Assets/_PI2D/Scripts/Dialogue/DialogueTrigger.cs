using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Estado del NPC")]
    [SerializeField] private DialogueNode firstEncounterNode; // Primera vez
    [SerializeField] private DialogueNode visitedNode;        // Veces siguientes

    private bool hasSpoken = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerInteract")) // O tu lógica de input
        {
            // Lógica de selección de diálogo
            DialogueNode nodeToPlay = (hasSpoken && visitedNode != null) ? visitedNode : firstEncounterNode;

            DialogueManager.Instance.StartDialogue(nodeToPlay);

            // Marcamos como hablado para la próxima vez
            hasSpoken = true;
        }
    }

    // Opcional: Si necesitas resetearlo desde un evento externo
    public void ResetDialogueState() => hasSpoken = false;
}