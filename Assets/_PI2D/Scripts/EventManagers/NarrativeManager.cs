using UnityEngine;
using UnityEngine.SceneManagement;

public class NarrativeManager : MonoBehaviour
{
    public static NarrativeManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CheckForNarrativeEvents(DialogueNode finishedNode)
    {
        if (finishedNode == null) return;

        // LÓGICA DE CAMBIO DE ESCENA POR ID
        if (finishedNode.changeSceneOnEnd)
        {
            Debug.Log($"[NarrativeManager] Fin de diálogo. Viajando a ID: {finishedNode.targetSceneIndex}");

            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadSceneWithFade(finishedNode.targetSceneIndex);
            }
            else
            {
                // Fallback si no hay transición
                SceneManager.LoadScene(finishedNode.targetSceneIndex);
            }
        }

        // Spawn de objetos (si usas)
        if (finishedNode.prefabToSpawn != null && !string.IsNullOrEmpty(finishedNode.spawnPointTag))
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag(finishedNode.spawnPointTag);
            if (spawnPoint != null) Instantiate(finishedNode.prefabToSpawn, spawnPoint.transform.position, Quaternion.identity);
        }
    }
}