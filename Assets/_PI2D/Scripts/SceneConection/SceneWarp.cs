using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SceneWarp : MonoBehaviour
{
    [Header("Destinos")]
    [SerializeField] private int normalSceneIndex;
    [SerializeField] private int alternativeSceneIndex;

    [Header("Bloqueo por Di�logos")]
    [Tooltip("IMPRESCINDIBLE: Los nodos aqu� listados deben tener 'Lock After Completion' marcado en su archivo.")]
    [SerializeField] private bool waitForDialogues = false;
    [SerializeField] private List<DialogueNode> requiredDialogues;

    [Header("Flag Opcional")]
    [SerializeField] private string requiredFlagID;

    private bool isWarping = false;

    // 1. CUANDO EL OBJETO SE ENCIENDE (Por si aparece encima del jugador)
    private void OnEnable()
    {
        isWarping = false;
        // Hacemos un chequeo manual instant�neo por si el jugador ya est� dentro
        StartCoroutine(CheckOverlapRoutine());
    }

    // Peque�a espera para asegurar que las f�sicas se han inicializado
    private IEnumerator CheckOverlapRoutine()
    {
        yield return new WaitForFixedUpdate();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // Buscamos si el jugador est� tocando este collider AHORA MISMO
            Collider2D[] hits = new Collider2D[5];
            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();
            int count = col.Overlap(filter, hits);

            for (int i = 0; i < count; i++)
            {
                if (hits[i].CompareTag("Player") || hits[i].CompareTag("PlayerInteract"))
                {
                    TryWarp(); // Intentamos warp si lo encontramos
                    break;
                }
            }
        }
    }

    // 2. MIENTRAS EL JUGADOR EST� DENTRO (Cubre 'Opci�n A' y espera de lectura)
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isWarping) return;

        if (collision.CompareTag("PlayerInteract"))
        {
            TryWarp();
        }
    }

    // 3. LA L�GICA DE COMPROBACI�N
    private void TryWarp()
    {
        // A. Si hay cuadro de texto abierto, ABORTAMOS.
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;

        // B. Si faltan di�logos, ABORTAMOS.
        if (waitForDialogues)
        {
            if (!CheckDialoguesCompleted()) return;
        }

        // C. �VIAJE!
        StartCoroutine(WarpSequence());
    }

    private bool CheckDialoguesCompleted()
    {
        if (NarrativeManager.Instance == null) return true;

        foreach (DialogueNode node in requiredDialogues)
        {
            if (node != null)
            {
                // Si el NarrativeManager dice que NO est� terminado, bloqueamos.
                if (!NarrativeManager.Instance.IsDialogueFinished(node.name))
                {
                    // DEBUG: Descomenta esto si sigue fallando para ver cu�l falta
                    // Debug.Log($"[SceneWarp] Esperando a que termines: {node.name}");
                    return false;
                }
            }
        }
        return true;
    }

    private IEnumerator WarpSequence()
    {
        isWarping = true;
        int target = normalSceneIndex;

        // Ruta Alternativa
        if (!string.IsNullOrEmpty(requiredFlagID) && NarrativeManager.Instance != null)
        {
            if (NarrativeManager.Instance.HasFlag(requiredFlagID))
                target = alternativeSceneIndex;
        }

        Debug.Log($"[SceneWarp] Condiciones cumplidas. Viajando a: {target}");

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadSceneWithFade(target);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(target);

        yield return null;
    }
}