using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class QuestItem : MonoBehaviour
{
    [Header("Configuración Narrativa")]
    [Tooltip("El nombre de la 'Flag' o variable que se guardará en NarrativeManager.")]
    [SerializeField] private string storyFlagID;

    [Header("Control de Objetos (Múltiple)")]
    [Tooltip("Lista de objetos que se APAGARÁN (desaparecen) al recoger este ítem.")]
    [SerializeField] private GameObject[] objectsToDeactivate;

    [Tooltip("Lista de objetos que se ENCENDERÁN (aparecen) al recoger este ítem.")]
    [SerializeField] private GameObject[] objectsToActivate;

    [Header("Audio")]
    [Tooltip("El índice del sonido en el AudioManager.")]
    [SerializeField] private int pickupSoundIndex = -1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PlayerInteract"))
        {
            RecogerObjeto();
        }
    }

    private void RecogerObjeto()
    {
        // 1. GUARDAR EN NARRATIVE MANAGER
        if (!string.IsNullOrEmpty(storyFlagID) && NarrativeManager.Instance != null)
        {
            NarrativeManager.Instance.AddStoryFlag(storyFlagID);
            Debug.Log($"[QuestItem] Flag guardada: {storyFlagID}");
        }

        // 2. APAGAR OBJETOS (Recorre la lista y los desactiva todos)
        if (objectsToDeactivate != null)
        {
            foreach (GameObject obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"[QuestItem] Objeto desactivado: {obj.name}");
                }
            }
        }

        // 3. ENCENDER OBJETOS (Recorre la lista y los activa todos)
        if (objectsToActivate != null)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"[QuestItem] Objeto activado: {obj.name}");
                }
            }
        }

        // 4. AUDIO
        if (AudioManager.Instance != null && pickupSoundIndex >= 0)
        {
            AudioManager.Instance.PlaySFX(pickupSoundIndex);
        }

        // 5. DESAPARECER EL PROPIO ÍTEM
        gameObject.SetActive(false);
    }
}