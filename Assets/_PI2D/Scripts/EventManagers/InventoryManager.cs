using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public class ItemReference
    {
        public string itemID;      // "LlaveSotano"
        public GameObject objRef;  // El objeto físico en ESTA escena
    }

    [Header("Inventario de ESTA Escena")]
    [Tooltip("Arrastra aquí los objetos recolectables que existen en esta escena.")]
    [SerializeField] private List<ItemReference> sceneItems = new List<ItemReference>();

    // Memoria Global (Persiste entre escenas)
    private HashSet<string> collectedItems = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si entramos a una escena nueva, el nuevo Manager le pasa sus items al Global
            Instance.UpdateSceneReferences(sceneItems);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CheckPersistence();
    }

    void Update()
    {
        CheckCollection();
    }

    public void UpdateSceneReferences(List<ItemReference> newItems)
    {
        sceneItems = newItems;
        CheckPersistence();
    }

    // 1. Al entrar: Apagar lo que ya tenemos
    private void CheckPersistence()
    {
        foreach (var item in sceneItems)
        {
            if (item.objRef != null)
            {
                if (collectedItems.Contains(item.itemID))
                {
                    item.objRef.SetActive(false); // Ya lo cogiste, que no aparezca
                }
                else
                {
                    item.objRef.SetActive(true); // No lo cogiste, asegúrate de que se vea
                }
            }
        }
    }

    // 2. En juego: Si se apaga, es que lo cogiste
    private void CheckCollection()
    {
        foreach (var item in sceneItems)
        {
            if (item.objRef != null)
            {
                // Si está apagado Y NO lo teníamos en la lista -> Lo registramos
                if (!item.objRef.activeSelf && !collectedItems.Contains(item.itemID))
                {
                    collectedItems.Add(item.itemID);
                    Debug.Log($"[Inventory] Item recogido: {item.itemID}");
                }
            }
        }
    }

    public bool HasItem(string id) => collectedItems.Contains(id);
}