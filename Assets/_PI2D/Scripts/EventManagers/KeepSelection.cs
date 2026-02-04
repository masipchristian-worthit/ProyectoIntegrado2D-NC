using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class KeepSelection : MonoBehaviour
{
    private EventSystem eventSystem;
    private GameObject lastSelected;

    void Awake()
    {
        eventSystem = GetComponent<EventSystem>();
    }

    void Update()
    {
        // 1. Si hay algo seleccionado, lo guardamos como "el último conocido"
        if (eventSystem.currentSelectedGameObject != null)
        {
            lastSelected = eventSystem.currentSelectedGameObject;
        }
        // 2. Si NO hay nada seleccionado (Unity lo deseleccionó), restauramos el último
        else if (lastSelected != null && lastSelected.activeInHierarchy)
        {
            eventSystem.SetSelectedGameObject(lastSelected);
        }
    }
}
