using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NarrativeManager : MonoBehaviour
{
    public static NarrativeManager Instance;

    // Listas de memoria
    private HashSet<string> storyFlags = new HashSet<string>();
    private HashSet<string> finishedDialogues = new HashSet<string>();
    private HashSet<string> blockedDialogues = new HashSet<string>(); // <--- FALTABA ESTO

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // --- GESTIÓN DE FLAGS ---
    public void AddStoryFlag(string flagID)
    {
        if (!string.IsNullOrEmpty(flagID) && !storyFlags.Contains(flagID))
            storyFlags.Add(flagID);
    }
    public bool HasFlag(string flagID) => storyFlags.Contains(flagID);

    // --- GESTIÓN DE DIÁLOGOS ---
    public bool IsDialogueFinished(string dialogueName) => finishedDialogues.Contains(dialogueName);

    // --- FUNCIONES DE BLOQUEO QUE FALTABAN ---
    public void BlockDialogue(string dialogueName)
    {
        if (!blockedDialogues.Contains(dialogueName))
        {
            blockedDialogues.Add(dialogueName);
        }
    }

    public bool IsDialogueBlocked(string dialogueName)
    {
        return blockedDialogues.Contains(dialogueName);
    }
    // ------------------------------------------

    public void CheckForNarrativeEvents(DialogueNode finishedNode)
    {
        if (finishedNode == null) return;

        if (!finishedDialogues.Contains(finishedNode.name))
            finishedDialogues.Add(finishedNode.name);

        // Activar Collider por Prefab
        if (finishedNode.objectToActivate != null)
        {
            string targetName = finishedNode.objectToActivate.name;
            GameObject obj = GameObject.Find(targetName);

            if (obj != null)
            {
                Collider2D col = obj.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
            }
        }

        // Cambio de escena
        if (finishedNode.changeSceneOnEnd && finishedNode.targetSceneIndex >= 0)
        {
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.LoadSceneWithFade(finishedNode.targetSceneIndex);
            else
                SceneManager.LoadScene(finishedNode.targetSceneIndex);
        }
    }
}