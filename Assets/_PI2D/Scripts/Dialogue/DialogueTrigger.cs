using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Árbol de Diálogo")]
    [Tooltip("El primer mensaje de la conversación. Despliega para añadir respuestas.")]
    [SerializeField] private DialogueNode rootNode;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica que chocamos con el trigger de interacción del Player
        if (collision.CompareTag("PlayerInteract"))
        {
            Debug.Log("Iniciando conversación ramificada...");
            DialogueManager.Instance.StartDialogue(rootNode);
        }
    }
}